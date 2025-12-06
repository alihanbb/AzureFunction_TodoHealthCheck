using MediatR;
using TodoApp.Repository;

namespace TodoApp.Features.Todos.Delete;

public record DeleteTodoCommand(Guid Id) : IRequest<bool>;

public class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly TodoDbContext _db;

    public DeleteTodoHandler(TodoDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _db.Todos.FindAsync(new object[] { request.Id }, cancellationToken);

        if (todo == null) return false;

        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public static class DeleteTodoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/todos/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteTodoCommand(id));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteTodo")
        .WithTags("Todos")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
