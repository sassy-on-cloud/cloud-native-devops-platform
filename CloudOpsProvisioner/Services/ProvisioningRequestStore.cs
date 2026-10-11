using CloudOpsProvisioner.Models;

namespace CloudOpsProvisioner.Services;

public class ProvisioningRequestStore
{
    private readonly List<ProvisioningRequest> _requests = new();
    private readonly object _lock = new();

    public IReadOnlyList<ProvisioningRequest> GetAll()
    {
        lock (_lock)
        {
            // Return a snapshot instead of exposing the internal list.
            return _requests.ToList().AsReadOnly();
        }
    }

    public void Add(ProvisioningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_lock)
        {
            // Keep the newest request first.
            _requests.Insert(0, request);
        }
    }

    public bool TryUpdateStatus(Guid requestId, string newStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newStatus);

        lock (_lock)
        {
            var request = _requests.FirstOrDefault(
                item => item.Id == requestId);

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
            return true;
        }
    }
}
