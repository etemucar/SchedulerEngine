using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Core.Seeding
{
    public static class InitialSeedData {
        public static readonly List<Language> Languages = new()
        {
            new() { Id = ReferenceDataIds.Language.Turkish, LanguageCd = "tr", Name = "Türkçe" },
            new() { Id = ReferenceDataIds.Language.English, LanguageCd = "en", Name = "English" },
            new() { Id = ReferenceDataIds.Language.Russian, LanguageCd = "ru", Name = "Русский" }
        };

        public static readonly List<LocalizableFields> LocalizableFields = new()
        {
            new() { Id = ReferenceDataIds.LocalizableFields.StatusName, EntityType = "Status", EntityField = "Name" },
            new() { Id = ReferenceDataIds.LocalizableFields.StatusDescription, EntityType = "Status", EntityField = "Description" },
        };

        public static readonly List<PartyRoleType> PartyRoleTypes = new()
        {
            new() { Id = ReferenceDataIds.PartyRoleType.SiteAdmin, PartyRoleTypeCd = "SITE_ADMIN", Name = "Site Yöneticis" },
            new() { Id = ReferenceDataIds.PartyRoleType.User, PartyRoleTypeCd = "USER", Name = "Uygulama Kullanıcısı" },
            new() { Id = ReferenceDataIds.PartyRoleType.Customer, PartyRoleTypeCd = "CUSTOMER", Name = "Müşteri" },
            new() { Id = ReferenceDataIds.PartyRoleType.BillAccount, PartyRoleTypeCd = "BILL_ACCOUNT", Name = "Fatura Hesabı" },
            new() { Id = ReferenceDataIds.PartyRoleType.ExternalService, PartyRoleTypeCd = "EXTERNAL_SERVICE", Name = "Dış servis hesabı" }
        };

    }

}
