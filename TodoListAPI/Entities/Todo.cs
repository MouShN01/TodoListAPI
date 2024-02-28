namespace TodoListAPI.Entities
{
    public class Todo
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool IsComplited {  get; set; }
        public List<string > Tags { get; set; }
        public DateTime DeadlineUtc{ get; set; }
    }
}
