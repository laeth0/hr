using Hr.BLL.Common;

namespace Hr.BLL.Errors
{
    public static class AddressErrors
    {
        public static Error NotFound(Guid employeeId) =>
            new("Address.NotFound", $"No address found for employee '{employeeId}'.", ErrorType.NotFound);

        public static Error AlreadyExists(Guid employeeId) =>
            new("Address.AlreadyExists", $"Employee '{employeeId}' already has an address.", ErrorType.Conflict);
    }
}
