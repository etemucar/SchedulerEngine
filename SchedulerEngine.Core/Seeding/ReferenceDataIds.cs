namespace SchedulerEngine.Core.Seeding;

/// <summary>
/// Seed verilerinde kullanılan sabit Id'ler. InitialSeedData.cs'teki tüm
/// listeler buradan besleniyor — aynı Id iki yerde farklı yazılıp
/// tutarsızlık çıkmasın diye tek kaynak burası.
///
/// NOT: Currency Id 1/2/3 (Try/Usd/Eur) InitialSeedData.cs'te zaten mevcuttu
/// (eski Code="TRL" -> "TRY" olarak düzeltildi, Id değişmedi). Yeni eklenen
/// 22 para birimi Id=4'ten devam ediyor.
/// </summary>
public static class ReferenceDataIds
{

    public static class Language    
    {
        public const int Turkish = 1;
        public const int English = 2;
        public const int Russian = 3;
    }

    public static class LocalizableFields
    {
        public const int StatusName = 1;
        public const int StatusDescription = 2;
    }

    // --- PartyRoleType Id'leri ---
    public static class PartyRoleType
    {
        public const int SiteAdmin      = 1;
        public const int User           = 2;
        public const int Customer       = 3;
        public const int BillAccount    = 4;
        public const int ExternalService = 5; // FinYo, DocDes gibi servis-servis çağıran sistemler

        public const string SiteAdminCd      = "SITE_ADMIN";
        public const string UserCd           = "USER";
        public const string CustomerCd       = "CUSTOMER";
        public const string BillAccountCd    = "BILL_ACCOUNT";
        public const string ExternalServiceCd = "EXTERNAL_SERVICE";

        // Token'a rol claim'i gömerken PartyRoleTypeId'den Cd üretmek için —
        // ekstra bir DB sorgusuna gerek kalmasın diye (InitialSeedData ile senkron tutulmalı).
        public static string ToCode(int partyRoleTypeId) => partyRoleTypeId switch
        {
            SiteAdmin        => SiteAdminCd,
            User             => UserCd,
            Customer         => CustomerCd,
            BillAccount      => BillAccountCd,
            ExternalService  => ExternalServiceCd,
            _ => throw new ArgumentOutOfRangeException(
                nameof(partyRoleTypeId), $"Bilinmeyen PartyRoleType Id: {partyRoleTypeId}")
        };
    }
}