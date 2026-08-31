using FluentValidation;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Validators;

public class AddOrUpdateRecurringJobCommandValidator : AbstractValidator<AddOrUpdateRecurringJobCommand>
{
    public AddOrUpdateRecurringJobCommandValidator()
    {
        RuleFor(x => x.RecurringJobId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CronExpression).NotEmpty();
        RuleFor(x => x.TaskName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.TimeZoneId)
            .Must(BeAValidTimeZone)
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZoneId))
            .WithMessage(x => $"'{x.TimeZoneId}' geçerli bir IANA saat dilimi id'si değil (örn. \"Europe/Istanbul\").");
    }

    private static bool BeAValidTimeZone(string? timeZoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId!);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}