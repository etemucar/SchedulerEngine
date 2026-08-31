using FluentValidation;
using SchedulerEngine.Core.Seeding;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Validators;

public class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
{
    public RegisterOrganizationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PartyRoleTypeId).GreaterThan(0);

        // ApplicationUser sadece ExternalService DIŞINDAKİ rollerde oluşur -
        // o durumda LanguageId zorunlu (handler'daki kuralla birebir aynı).
        RuleFor(x => x.LanguageId)
            .NotNull()
            .When(x => x.PartyRoleTypeId != ReferenceDataIds.PartyRoleType.ExternalService)
            .WithMessage("LanguageId zorunludur (ApplicationUser oluşturulacak - PartyRoleTypeId ExternalService değil).");

        RuleForEach(x => x.Credentials).ChildRules(credential =>
        {
            credential.RuleFor(c => c.CredentialType).IsInEnum();
        });
    }
}