using MediatR;
using TodoApp.Repository;

namespace TodoApp.Features.Todos.GetById;

public record GetTodoByIdQuery(Guid Id) : IRequest<TodoDetailResponse?>;

public record TodoDetailResponse(Guid Id, string Title, string? Description, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);

public class GetTodoByIdHandler : IRequestHandler<GetTodoByIdQuery, TodoDetailResponse?>
{
    private readonly TodoDbContext _db;

    public GetTodoByIdHandler(TodoDbContext db) => _db = db;

    public async Task<TodoDetailResponse?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _db.Todos.FindAsync(new object[] { request.Id }, cancellationToken);

        if (todo == null) return null;

        return new TodoDetailResponse(todo.Id, todo.Title, todo.Description, todo.IsCompleted, todo.CreatedAt, todo.CompletedAt);
    }
}

public static class GetTodoByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/todos/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTodoByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetTodoById")
        .WithTags("Todos")
        .Produces<TodoDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
