using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoListAPI.Contracts;
using TodoListAPI.Database;
using TodoListAPI.Shared;

namespace TodoListAPI.Features.Todos
{
    public static class GetAllTodos
    {
        public class Query:IRequest<Result<TodosPageResponse>>
        {
            public int PageSize { get; set; } = 5;
            public int PageNumber { get; set; } = 1;
        }

        public class Validator : AbstractValidator<Query> 
        {
            public Validator()
            {
                RuleFor(q => q.PageSize).GreaterThan(0).LessThan(100);
                RuleFor(q => q.PageNumber).GreaterThan(0);
            }
        }

        internal sealed class Handler : IRequestHandler<Query, Result<TodosPageResponse>>
        {
            private readonly ApplicationDbContext _context;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext context, IValidator<Query> validator)
            {
                _context = context;
                _validator = validator;
            }
          
            public async Task<Result<TodosPageResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if(!validationResult.IsValid) 
                {
                    return Result.Failure<TodosPageResponse>(new Error("GetAllTodos.Validation", validationResult.ToString()));
                }

                var count = await _context.Todos.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(count / (double)request.PageSize);

                if(request.PageNumber > totalPages)
                {
                    return Result.Failure<TodosPageResponse>(new Error("GetAllTodos.Handle", "Not existed page"));
                }

                var todosPage = new TodosPageResponse
                {
                    Todos = await _context.Todos
                        .OrderByDescending(t => t.CreatedOnUtc)
                        .Skip((request.PageNumber - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToListAsync(cancellationToken),
                    PageNumber = request.PageNumber,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                };
                return todosPage;
            }
        }
    }

    public class GetAllTodosEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/todos", async(int pageSize, int pageNumber, ISender sender) =>
            {
                var query = new GetAllTodos.Query { PageNumber = pageNumber, PageSize = pageSize };
                var result = await sender.Send(query);

                if(result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Ok(result.Value);
            });
        }
    }

}
