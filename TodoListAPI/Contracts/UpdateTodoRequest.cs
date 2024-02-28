namespace TodoListAPI.Contracts
{
    public class UpdateTodoRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public List<string> Tags { get; set; } = new();
        public DateTime DeadLineUtc { get; set; } = DateTime.UtcNow;
    }
}
