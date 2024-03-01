namespace TodoListAPI.Contracts;

public class AccountResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}