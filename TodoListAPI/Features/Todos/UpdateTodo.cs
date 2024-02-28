using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using TodoListAPI.Contracts;
using TodoListAPI.Database;
using TodoListAPI.Entities;
using TodoListAPI.Shared;

namespace TodoListAPI.Features.Todos
{
    public static class UpdateTodo
    {
        public class Command : IRequest<Result<Todo>>
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsCompleted { get; set; } = false;
            public List<string> Tags { get; set; } = new();
            public DateTime DeadLineUtc { get; set; } = DateTime.UtcNow;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Id)
                    .Must(guid => Guid.TryParse(guid, out _))
                    .WithMessage("Guid has not correct format");
                RuleFor(c => c.Title).NotEmpty().MaximumLength(50);
                RuleFor(c => c.Description).NotEmpty().MaximumLength(200);
                RuleFor(c => c.Tags)
                    .NotEmpty()
                    .ForEach(t => t.NotEmpty().MaximumLength(10));
                RuleFor(c => c.DeadLineUtc).GreaterThan(DateTime.UtcNow);
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<Todo>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result<Todo>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Todo>(new Error("UpdateTodo.Validation", validationResult.ToString()));
                }

                var todoToUpdate = await _dbContext.Todos.FindAsync(Guid.Parse(request.Id));

                if (todoToUpdate is null)
                {
                    return Result.Failure<Todo>(Error.NotFound);
                }

                todoToUpdate.Title = request.Title;
                todoToUpdate.Description = request.Description;
                todoToUpdate.IsComplited = request.IsCompleted;
                todoToUpdate.Tags = request.Tags;
                todoToUpdate.DeadlineUtc = request.DeadLineUtc;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return todoToUpdate;
            }
        }
    }
    public class UpdateTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/todos/{id}", async (UpdateTodoRequest request, string id, ISender sender) =>
            {
                var command = new UpdateTodo.Command
                {
                    Id = id,
                    Title = request.Title,
                    Description = request.Description,
                    IsCompleted = request.IsCompleted,
                    Tags = request.Tags,
                    DeadLineUtc = request.DeadLineUtc
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
                return Results.Ok(result.Value);
            });
        }
    }
}
