using CloudOpsProvisioner.Data;
using CloudOpsProvisioner.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudOpsProvisioner.Services;

public class ProvisioningRequestStore
{
    private readonly AppDbContext _dbContext;

    public ProvisioningRequestStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProvisioningRequest>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProvisioningRequests
            .AsNoTracking()
            .OrderByDescending(request => request.RequestedAt)
            .ThenByDescending(request => request.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _dbContext.ProvisioningRequests.AddAsync(
            request, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TryUpdateStatusAsync(
        Guid requestId,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newStatus);

        var request = await _dbContext.ProvisioningRequests
            .FirstOrDefaultAsync(
                item => item.Id == requestId,
                cancellationToken);

        if (request is null)
        {
            return false;
        }

        var allowed = request.Status switch
        {
            ProvisioningRequestStatuses.Pending =>
                newStatus == ProvisioningRequestStatuses.Processing,

            ProvisioningRequestStatuses.Processing =>
                newStatus == ProvisioningRequestStatuses.Completed ||
                newStatus == ProvisioningRequestStatuses.Failed,

            _ => false
        };

        if (!allowed)
        {
            return false;
        }

        request.Status = newStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
