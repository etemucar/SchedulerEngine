using FluentValidation;
using SchedulerEngine.Core.Enums;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Validators;

public class UpdateDigitalIdentityStatusCommandValidator
    : AbstractValidator<UpdateDigitalIdentityStatusCommand>
{
    public UpdateDigitalIdentityStatusCommandValidator()
    {
        RuleFor(x => x.DigitalIdentityId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
