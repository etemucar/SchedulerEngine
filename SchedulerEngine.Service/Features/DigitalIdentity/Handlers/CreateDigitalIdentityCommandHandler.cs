using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Enums;
using SchedulerEngine.Core.Security;
using SchedulerEngine.Core.Services;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Dtos.Requests;

namespace SchedulerEngine.Service.Features.Handlers;

public class CreateDigitalIdentityCommandHandler
    : IRequestHandler<CreateDigitalIdentityCommand, DigitalIdentityResponse>
{
    private readonly IRepository<DigitalIdentity, Guid>  _digitalIdentityRepository;
    private readonly IRepository<ApplicationUser, int>   _userRepository;
    private readonly IRepository<PartyRole, int>         _partyRoleRepository;
    private readonly ICurrentUserService                 _currentUserService;
    private readonly IPasswordHasher                     _passwordHasher;
    private readonly ILogger<CreateDigitalIdentityCommandHandler> _logger;

    public CreateDigitalIdentityCommandHandler(
        IRepository<DigitalIdentity, Guid>               digitalIdentityRepository,
        IRepository<ApplicationUser, int>                userRepository,
        IRepository<PartyRole, int>                      partyRoleRepository,
        ICurrentUserService                              currentUserService,
        IPasswordHasher                                  passwordHasher,
        ILogger<CreateDigitalIdentityCommandHandler>     logger)
    {
        _digitalIdentityRepository = digitalIdentityRepository;
        _userRepository            = userRepository;
        _partyRoleRepository       = partyRoleRepository;
        _currentUserService        = currentUserService;
        _passwordHasher            = passwordHasher;
        _logger                    = logger;
    }

    public async Task<DigitalIdentityResponse> Handle(
        CreateDigitalIdentityCommand request,
        CancellationToken cancellationToken)
    {
        // 0. Yetki kontrolü — sadece SITE_ADMIN başkası adına DigitalIdentity oluşturabilir
        var actingPartyRoleId = await _currentUserService.GetPartyRoleIdAsync(cancellationToken);
        var actingRole = await _partyRoleRepository.FindOneAsync(
            pr => pr.Id == actingPartyRoleId,
            include: q => q.Include(pr => pr.PartyRoleType),
            ct: cancellationToken);

        if (actingRole?.PartyRoleType?.PartyRoleTypeCd != "SITE_ADMIN")
            throw new UnauthorizedException("Bu işlem için yetkiniz yok.");

        // 1. PartyRole var mı kontrol et
        var partyRole = await _partyRoleRepository.FindOneAsync(
            pr => pr.Id == request.PartyRoleId,
            ct: cancellationToken);

        if (partyRole is null)
            throw new NotFoundException($"PartyRole bulunamadı. Id: {request.PartyRoleId}");

        // 2. Credential'ları oluştur
        var credentials = request.Credentials.Select(cr => new Credential
        {
            CredentialType = cr.CredentialType,
            TrustLevel     = cr.TrustLevel,
            Characteristics = cr.Characteristics.Select(ch => new CredentialCharacteristic
            {
                Name = ch.Name switch
                {
                    "password" => "passwordHash",
                    "apiKey"   => "apiKeyHash",
                    _          => ch.Name
                },
                Value = ch.Name switch
                {
                    "password" => _passwordHasher.Hash(ch.Value),
                    "apiKey"   => _passwordHasher.Hash(ch.Value),
                    _          => ch.Value
                }
            }).ToList(),
            ContactMedia = cr.ContactMedia.Select(cm => MapContactMedium(cm, partyRole.PartyId)).ToList()
        }).ToList();

        // 3. DigitalIdentity oluştur
        var digitalIdentity = new DigitalIdentity
        {
            Nickname            = request.Nickname,
            Status              = GeneralStatus.Active,
            DigitalIdentityDate = DateTime.UtcNow,
            PartyRoleId         = request.PartyRoleId,
            Credentials         = credentials
        };

        await _digitalIdentityRepository.AddAsync(digitalIdentity, cancellationToken);

        // 4. ApplicationUser oluştur
        var applicationUser = new ApplicationUser
        {
            DigitalIdentityId = digitalIdentity.Id
        };

        await _userRepository.AddAsync(applicationUser, cancellationToken);

        _logger.LogInformation(
            "DigitalIdentity oluşturuldu. Id: {Id}, PartyRoleId: {PartyRoleId}, OluşturanRoleId: {ActingRoleId}",
            digitalIdentity.Id, request.PartyRoleId, actingPartyRoleId);

        return MapToResponse(digitalIdentity);
    }

    private static ContactMedium MapContactMedium(ContactMediumRequest cm, int partyId)
    {
        var mediumType = Enum.Parse<ContactMediumType>(cm.MediumType, ignoreCase: true);

        string? email       = null;
        string? phoneNumber = null;
        string? url         = null;

        switch (mediumType)
        {
            case ContactMediumType.EmailAddress:
                email = cm.Characteristic.GetValueOrDefault("emailAddress")?.ToString();
                break;
            case ContactMediumType.PhoneNumber:
                phoneNumber = cm.Characteristic.GetValueOrDefault("phoneNumber")?.ToString();
                break;
            case ContactMediumType.Url:
                url = cm.Characteristic.GetValueOrDefault("url")?.ToString();
                break;
        }

        return new ContactMedium
        {
            PartyId     = partyId,
            MediumType  = mediumType,
            IsPreferred = cm.Preferred,
            Email       = email,
            PhoneNumber = phoneNumber,
            Url         = url
        };
    }

    private static DigitalIdentityResponse MapToResponse(DigitalIdentity d) => new()
    {
        Id                  = d.Id,
        Nickname            = d.Nickname,
        Status              = d.Status,
        DigitalIdentityDate = d.DigitalIdentityDate,
        PartyRoleId         = d.PartyRoleId
    };
}