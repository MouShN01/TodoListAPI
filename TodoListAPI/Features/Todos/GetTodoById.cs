using Carter;
using FluentValidation;
using MediatR;
using TodoListAPI.Database;
using TodoListAPI.Entities;
using TodoListAPI.Shared;

namespace TodoListAPI.Features.Todos
{
    public static class GetTodoById
    {
        public class Query : IRequest<Result<Todo>>
        {
            public string Id { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(c => c.Id)
                    .Must(guid => Guid.TryParse(guid, out _))
                    .WithMessage("Guid has not correct format");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, Result<Todo>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result<Todo>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<Todo>(new Error("GetTodoById.Validation", validationResult.ToString()));
                }

                var todoToGet = await _dbContext.Todos.FindAsync(Guid.Parse(request.Id));
                if (todoToGet is null)
                {
                    return Result.Failure<Todo>(Error.NotFound);
                }

                return todoToGet;
            }
        }
    }
    public class GetTodoByIdEndpoint:ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/todos/{id}", async (string id, ISender sender) =>
            {
                var command = new GetTodoById.Query
                {
                    Id = id
                };

                var result = await sender.Send(command);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Value);
                }
                if (result.Error.Equals(Error.NotFound))
                {
                    return Results.NotFound(result.Error);
                }
                return Results.BadRequest(result.Error);
            });
        }
    }
}
