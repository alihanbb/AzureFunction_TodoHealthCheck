using MediatR;
using TodoApp.Repository;

namespace TodoApp.Features.Todos.Create;
public record CreateTodoCommand(string Title, string? Description) : IRequest<CreateTodoResponse>;
public record CreateTodoResponse(Guid Id, string Title, string? Description, DateTime CreatedAt);

public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, CreateTodoResponse>
{
    private readonly TodoDbContext _db;

    public CreateTodoHandler(TodoDbContext db) => _db = db;

    public async Task<CreateTodoResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Todos.Add(todo);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateTodoResponse(todo.Id, todo.Title, todo.Description, todo.CreatedAt);
    }
}

public static class CreateTodoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/todos", async (CreateTodoCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/todos/{result.Id}", result);
        })
        .WithName("CreateTodo")
        .WithTags("Todos")
        .Produces<CreateTodoResponse>(StatusCodes.Status201Created);
    }
}
