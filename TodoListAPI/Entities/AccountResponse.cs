namespace TodoListAPI.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<Todo> Todos { get; set; }
    }
}
