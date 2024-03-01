using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TodoListAPI.Contracts;
using TodoListAPI.Database;
using TodoListAPI.Entities;
using TodoListAPI.Shared;

namespace TodoListAPI.Features.Accounts;

public static class CreateAccount
{
    public class Command : IRequest<Result<AccountResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Email).NotEmpty().EmailAddress();
            RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
        }
    }

    internal sealed class Handler :IRequestHandler<Command, Result<AccountResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<AccountResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Failure<AccountResponse>(new Error("CreateAccountResponse.Validation", validationResult.ToString()));
            }

            var isExistedAccount = _dbContext.Accounts
                .Any(account => account.Email == request.Email);

            if (isExistedAccount)
            {
                return Result.Failure<AccountResponse>(Error.AccountAlreadyExists);
            }

            var account = new Account
            {
                Email = request.Email,
                Password = request.Password,
                Todos = new()
            };

            _dbContext.Add(account);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return account.Adapt<AccountResponse>();
        }
    }
}
public class CreateAccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/accounts", async (CreateAccountRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateAccount.Command>();
            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}