using SchedulerEngine.Core.Base;

namespace SchedulerEngine.Core.Model;

public class PartyRoleAccount : ModelBase<int>
{
    public int PartyRoleId { get; set; }
    public string CurrencyCode { get; set; } = null!;

    public virtual PartyRole PartyRole { get; set; } = null!;

}
