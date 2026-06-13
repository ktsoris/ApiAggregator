using ApiAggregator.Domain.Entities;

namespace ApiAggregator.Application.Common.Interfaces;

public interface IApiRequestLogRepository
{
    Task AddAsync(ApiRequestLog log, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiRequestLog>> GetByApiNameAsync(string apiName, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiRequestLog>> GetRecentAsync(TimeSpan window, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}