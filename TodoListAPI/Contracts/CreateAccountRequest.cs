﻿namespace TodoListAPI.Contracts;

public class CreateAccountRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}