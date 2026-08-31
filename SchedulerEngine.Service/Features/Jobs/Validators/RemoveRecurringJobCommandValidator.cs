using FluentValidation;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Validators;

public class RemoveRecurringJobCommandValidator : AbstractValidator<RemoveRecurringJobCommand>
{
    public RemoveRecurringJobCommandValidator()
    {
        RuleFor(x => x.RecurringJobId).NotEmpty();
    }
}