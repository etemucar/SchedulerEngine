using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetIndividualQueryHandler : IRequestHandler<GetIndividualQuery, IndividualResponse>
{
    private readonly IRepository<Individual, int> _individualRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetIndividualQueryHandler> _logger;

    public GetIndividualQueryHandler(
        IRepository<Individual, int> individualRepository,
        IMapper mapper,
        ILogger<GetIndividualQueryHandler> logger)
    {
        _individualRepository = individualRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IndividualResponse> Handle(GetIndividualQuery request, CancellationToken cancellationToken)
    {
        var individual = await _individualRepository.GetByIdAsync(request.Id, cancellationToken);

        if (individual is null)
            throw new NotFoundException($"Individual bulunamadı: {request.Id}");

        return _mapper.Map<IndividualResponse>(individual);
    }
}