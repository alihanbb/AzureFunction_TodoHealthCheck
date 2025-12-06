using MediatR;
using TodoApp.Repository;

namespace TodoApp.Features.Todos.Complete;

public record CompleteTodoCommand(Guid Id, bool IsCompleted = true) : IRequest<bool>;

public class CompleteTodoHandler : IRequestHandler<CompleteTodoCommand, bool>
{
    private readonly TodoDbContext _db;

    public CompleteTodoHandler(TodoDbContext db) => _db = db;

    public async Task<bool> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _db.Todos.FindAsync(new object[] { request.Id }, cancellationToken);

        if (todo == null) return false;

        todo.IsCompleted = request.IsCompleted;
        todo.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public static class CompleteTodoEndpoint
{
    public record CompleteTodoRequest(bool IsCompleted = true);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/todos/{id:guid}/complete", async (Guid id, CompleteTodoRequest? request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CompleteTodoCommand(id, request?.IsCompleted ?? true));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("CompleteTodo")
        .WithTags("Todos")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
