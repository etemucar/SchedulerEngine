namespace SchedulerEngine.Core.Responses;

public interface IDataResponse<out T> : IResponse
{
    T? Data { get; }
}
