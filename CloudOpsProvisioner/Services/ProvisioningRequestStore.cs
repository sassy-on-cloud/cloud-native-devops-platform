using CloudOpsProvisioner.Models;

namespace CloudOpsProvisioner.Services;

public class ProvisioningRequestStore
{
    private readonly List<ProvisioningRequest> _requests = new();

    public IReadOnlyList<ProvisioningRequest> GetAll()
    {
        return _requests.AsReadOnly();
    }

    public void Add(ProvisioningRequest request)
    {
        _requests.Insert(0, request);
    }
}
