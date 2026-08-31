using FluentValidation;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Validators;

public class ScheduleExternalTaskCommandValidator : AbstractValidator<ScheduleExternalTaskCommand>
{
    public ScheduleExternalTaskCommandValidator()
    {
        RuleFor(x => x.TaskName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DelayMinutes).GreaterThan(0);
    }
}