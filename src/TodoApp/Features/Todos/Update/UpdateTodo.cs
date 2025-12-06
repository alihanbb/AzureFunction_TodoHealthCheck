using MediatR;
using TodoApp.Repository;

namespace TodoApp.Features.Todos.Update;

public record UpdateTodoCommand(Guid Id, string Title, string? Description) : IRequest<bool>;

public class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, bool>
{
    private readonly TodoDbContext _db;

    public UpdateTodoHandler(TodoDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _db.Todos.FindAsync(new object[] { request.Id }, cancellationToken);

        if (todo == null) return false;

        todo.Title = request.Title;
        todo.Description = request.Description;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public static class UpdateTodoEndpoint
{
    public record UpdateTodoRequest(string Title, string? Description);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/todos/{id:guid}", async (Guid id, UpdateTodoRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateTodoCommand(id, request.Title, request.Description));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateTodo")
        .WithTags("Todos")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
