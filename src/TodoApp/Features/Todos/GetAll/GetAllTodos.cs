using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Repository;

namespace TodoApp.Features.Todos.GetAll;

public record GetAllTodosQuery(bool? IsCompleted = null) : IRequest<List<TodoListItem>>;

public record TodoListItem(Guid Id, string Title, bool IsCompleted, DateTime CreatedAt);

public class GetAllTodosHandler : IRequestHandler<GetAllTodosQuery, List<TodoListItem>>
{
    private readonly TodoDbContext _db;

    public GetAllTodosHandler(TodoDbContext db) => _db = db;

    public async Task<List<TodoListItem>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Todos.AsQueryable();

        if (request.IsCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == request.IsCompleted.Value);

        return await query
            .Select(t => new TodoListItem(t.Id, t.Title, t.IsCompleted, t.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
public static class GetAllTodosEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/todos", async (bool? isCompleted, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllTodosQuery(isCompleted));
            return Results.Ok(result);
        })
        .WithName("GetAllTodos")
        .WithTags("Todos")
        .Produces<List<TodoListItem>>();
    }
}
