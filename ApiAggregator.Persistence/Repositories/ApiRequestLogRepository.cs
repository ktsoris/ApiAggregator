using ApiAggregator.Application.Common.Interfaces;
using ApiAggregator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiAggregator.Persistence.Repositories;

public sealed class ApiRequestLogRepository : IApiRequestLogRepository
{
    private readonly ApiAggregatorDbContext _context;

    public ApiRequestLogRepository(ApiAggregatorDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ApiRequestLog log, CancellationToken cancellationToken)
    {
        await _context.ApiRequestLogs.AddAsync(log, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiRequestLog>> GetByApiNameAsync(
        string apiName,
        CancellationToken cancellationToken)
    {
        return await _context.ApiRequestLogs
            .Where(x => x.ApiName == apiName)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApiRequestLog>> GetRecentAsync(
        TimeSpan window,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow - window;

        return await _context.ApiRequestLogs
            .Where(x => x.RequestedAt >= cutoff)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}