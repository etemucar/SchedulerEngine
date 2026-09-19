using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;

namespace SchedulerEngine.Service.Features.Handlers;

public class PatchIndividualCommandHandler : IRequestHandler<PatchIndividualCommand, IndividualResponse>
{
    private readonly IRepository<Individual, int> _individualRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PatchIndividualCommandHandler> _logger;

    public PatchIndividualCommandHandler(
        IRepository<Individual, int> individualRepository,
        IMapper mapper,
        ILogger<PatchIndividualCommandHandler> logger)
    {
        _individualRepository = individualRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IndividualResponse> Handle(PatchIndividualCommand request, CancellationToken cancellationToken)
    {
        var individual = await _individualRepository.GetByIdAsync(request.Id, cancellationToken);

        if (individual is null)
            throw new NotFoundException($"Individual bulunamadı: {request.Id}");

        // Sadece gelen alanları güncelle (merge-patch)
        if (request.GivenName is not null) individual.GivenName = request.GivenName;
        if (request.FamilyName is not null) individual.FamilyName = request.FamilyName;
        if (request.MiddleName is not null) individual.MiddleName = request.MiddleName;
        if (request.Title is not null) individual.Title = request.Title;
        if (request.Gender is not null) individual.Gender = request.Gender;
        if (request.Nationality is not null) individual.Nationality = request.Nationality;
        if (request.BirthDate is not null) individual.BirthDate = request.BirthDate;
        if (request.PlaceOfBirth is not null) individual.PlaceOfBirth = request.PlaceOfBirth;
        if (request.CountryOfBirth is not null) individual.CountryOfBirth = request.CountryOfBirth;
        if (request.MaritalStatus is not null) individual.MaritalStatus = request.MaritalStatus;

        individual.ValidForStart = request.ValidForStart ?? individual.ValidForStart;
        individual.ValidForEnd = request.ValidForEnd ?? individual.ValidForEnd;

        await _individualRepository.UpdateAsync(individual, cancellationToken);

        _logger.LogInformation("Individual güncellendi. IndividualId: {IndividualId}", individual.Id);

        return _mapper.Map<IndividualResponse>(individual);
    }
}