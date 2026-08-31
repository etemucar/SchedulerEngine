namespace SchedulerEngine.Core.Repository
{
    public interface IIncludableQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
    {
    }
}
