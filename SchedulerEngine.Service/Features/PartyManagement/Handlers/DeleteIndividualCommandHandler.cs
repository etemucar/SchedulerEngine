using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class DeleteIndividualCommandHandler : IRequestHandler<DeleteIndividualCommand, bool>
{
    private readonly IRepository<Individual, int> _individualRepository;
    private readonly ILogger<DeleteIndividualCommandHandler> _logger;

    public DeleteIndividualCommandHandler(
        IRepository<Individual, int> individualRepository,
        ILogger<DeleteIndividualCommandHandler> logger)
    {
        _individualRepository = individualRepository;
        _logger               = logger;
    }

    public async Task<bool> Handle(DeleteIndividualCommand request, CancellationToken cancellationToken)
    {
        var individual = await _individualRepository.GetByIdAsync(request.Id, cancellationToken);

        if (individual is null)
            return false;

        await _individualRepository.RemoveAsync(individual, cancellationToken);

        _logger.LogInformation("Individual silindi. IndividualId: {IndividualId}", individual.Id);

        return true;
    }
}
