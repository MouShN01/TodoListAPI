namespace TodoListAPI.Contracts
{
    public class CreateTodoRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public DateTime DeadLineUtc { get; set; } = DateTime.UtcNow;
    }
}
