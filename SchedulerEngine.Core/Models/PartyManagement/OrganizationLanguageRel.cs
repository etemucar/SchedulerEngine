using SchedulerEngine.Core.Base;

namespace SchedulerEngine.Core.Model;

public class OrganizationLanguageRel : ModelBase<int>
{
    public int OrganizationId { get; set; }
    public int LanguageId { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual Language Language { get; set; } = null!;
}
