namespace Hr.BLL.Common
{
    public sealed class Error
    {
        public string Code { get; }
        public string Message { get; }
        public ErrorType Type { get; }

        // Present only when Type == Validation; holds individual field-level messages.
        public IReadOnlyList<string>? Details { get; }

        public Error(
            string code,
            string message,
            ErrorType type = ErrorType.Failure,
            IReadOnlyList<string>? details = null)
        {
            Code = code;
            Message = message;
            Type = type;
            Details = details;
        }

        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

        // Factory for FluentValidation results — keeps the conversion in one place.
        public static Error Validation(IEnumerable<string> errors) =>
            new("Validation.Failed",
                "One or more validation errors occurred.",
                ErrorType.Validation,
                errors.ToList().AsReadOnly());
    }
}
