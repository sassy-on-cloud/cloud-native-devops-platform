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
}
