namespace TodoListAPI.Shared
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "Idi Nahui");
        public static readonly Error ConditionNotMet = new("Error.ConditionNotMet", "The specified condition was not met.");
        public static readonly Error NotFound = new("Error.NotFound", "Todo with specified id was not found");
    }
}
