using Hr.BLL.Common;
using Hr.DAL.Enums;

namespace Hr.BLL.Errors
{
    public static class LeaveErrors
    {
        public static Error NotFound(Guid id) =>
            new("Leave.NotFound", $"Leave '{id}' was not found.", ErrorType.NotFound);

        public static readonly Error DateOverlap =
            new("Leave.DateOverlap", "The requested dates overlap with an existing leave.");

        public static Error InsufficientBalance(int requested, int remaining) =>
            new("Leave.InsufficientBalance",
                $"Insufficient annual leave balance. Requested: {requested} day(s), remaining: {remaining}.");

        public static Error NotPending(LeaveStatus current) =>
            new("Leave.NotPending",
                $"Only pending leaves can be processed. Current status: {current}.");

        public static Error CannotCancel(LeaveStatus current) =>
            new("Leave.CannotCancel",
                $"A {current.ToString().ToLower()} leave cannot be cancelled.");
    }
}
