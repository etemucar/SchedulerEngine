using FluentValidation;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Validators;

public class EnqueueExternalTaskCommandValidator : AbstractValidator<EnqueueExternalTaskCommand>
{
    public EnqueueExternalTaskCommandValidator()
    {
        RuleFor(x => x.TaskName).NotEmpty().MaximumLength(200);
    }
}