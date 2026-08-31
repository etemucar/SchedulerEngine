
using SchedulerEngine.Core.Base;

namespace SchedulerEngine.Core.Model;

public class LocalizableFields : ModelBase<int>
{
    public string EntityType { get; set; } = null!;
    public string EntityField { get; set; } = null!;

}
