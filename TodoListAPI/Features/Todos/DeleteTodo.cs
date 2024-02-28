
using Carter;
using FluentValidation;
using MediatR;
using TodoListAPI.Contracts;
using TodoListAPI.Database;
using TodoListAPI.Entities;
using TodoListAPI.Shared;

namespace TodoListAPI.Features.Todos
{
    public static class DeleteTodo
    {
        public class Command : IRequest<Result>
        {
            public string Id { get; set; } = string.Empty;
        }
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Id)
                    .Must(guid => Guid.TryParse(guid, out _))
                    .WithMessage("Guid has not correct format");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Result.Failure(new Error("DeleteTodo.Validation", validationResult.ToString()));
                }

                var todoToDelete = await _dbContext.Todos.FindAsync(Guid.Parse(request.Id));

                if (todoToDelete is null)
                {
                    return Result.Failure(Error.NotFound);
                }

                _dbContext.Remove(todoToDelete);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }

    }

    public class DeleteTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/todos/{id}", async (string id, ISender sender) =>
            {
                var command = new DeleteTodo.Command
                {
                    Id = id
                };

                var result = await sender.Send(command);

                if (result.IsFailure)
                {
                    if (result.Error.Equals(Error.NotFound))
                    {
                        return Results.NotFound(result.Error);
                    }
                    return Results.BadRequest(result.Error);
                }
                return Results.NoContent();
            });
        }
    }
}
