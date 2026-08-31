using SchedulerEngine.Core.Base;

namespace SchedulerEngine.Core.Model;

public class RelatedParty : ModelBase<int>
{
    public int PartyId { get; set; }

    // İlişkili taraf — doğrudan Party FK
    public int RelatedPartyId { get; set; }

    // Bu ilişkideki rolü: "spouse", "employer", "subsidiary" vb.
    public string Role { get; set; } = null!;

    // Geçerlilik süresi (TM Forum'da TimePeriod)
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public virtual Party Party { get; set; } = null!;
    public virtual Party Related { get; set; } = null!;

}
