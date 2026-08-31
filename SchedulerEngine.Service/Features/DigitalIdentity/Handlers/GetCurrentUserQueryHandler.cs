using MediatR;
using Microsoft.EntityFrameworkCore;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Service.Dtos.Responses;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IRepository<ApplicationUser, int> _userRepository;

    public GetCurrentUserQueryHandler(IRepository<ApplicationUser, int> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindOneAsync(
            u => u.Id == request.UserId,
            include: q => q
                .Include(u => u.Language)
                .Include(u => u.DigitalIdentity)
                    .ThenInclude(d => d.PartyRole)
                        .ThenInclude(pr => pr.Party)
                            .ThenInclude(p => p.Individual)
                .Include(u => u.DigitalIdentity)
                    .ThenInclude(d => d.Credentials)
                        .ThenInclude(c => c.ContactMedia),
            ct: cancellationToken);

        if (user is null)
            throw new NotFoundException("Kullanıcı bulunamadı.");

        var individual = user.DigitalIdentity.PartyRole?.Party?.Individual;
        var contactMedium = user.DigitalIdentity.Credentials
            .SelectMany(c => c.ContactMedia)
            .FirstOrDefault();

        return new CurrentUserResponse
        {
            Id         = user.Id,
            Identifier = contactMedium?.Email ?? contactMedium?.PhoneNumber ?? string.Empty,
            GivenName  = individual?.GivenName  ?? string.Empty,
            FamilyName = individual?.FamilyName ?? string.Empty,
            Locale     = user.Language.LanguageCd
        };
    }
}
