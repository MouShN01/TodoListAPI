using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using TodoListAPI.Database;
using TodoListAPI.Shared;
using TodoListAPI.Entities;
using TodoListAPI.Contracts;

namespace TodoListAPI.Features.Todos
{
    public static class CreateTodo
    {
        public class Command:IRequest<Result<Todo>>
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<string> Tags { get; set; } = [];
            public DateTime DeadLineUtc { get; set; } = DateTime.UtcNow;
        }

        public class Validator: AbstractValidator<Command>
        {
            public Validator()
            {
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
                if(!validationResult.IsValid) 
                {
                    return Result.Failure<Todo>(new Error("CreateTodo.Validation", validationResult.ToString()));
                }

                var todo = new Todo
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Description = request.Description,
                    CreatedOnUtc = DateTime.UtcNow,
                    Tags = request.Tags,
                    DeadlineUtc = request.DeadLineUtc
                };

                _dbContext.Add(todo);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return todo;
            }
        }
    }

    public class CreateTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/todos", async (CreateTodoRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateTodo.Command>();

                var result = await sender.Send(command);

                if(result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }
                return Results.Ok(result.Value);
            });
        }
    }
}
