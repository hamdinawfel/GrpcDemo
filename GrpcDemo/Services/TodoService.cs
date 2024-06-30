using Grpc.Core;
using GrpcDemo.Data;
using GrpcDemo.Models;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using static GrpcDemo.Todo;

namespace GrpcDemo.Services
{
    public class TodoService : TodoBase
    {
        private readonly AppDbContext _context;
        public TodoService(AppDbContext context)
        {
            _context = context;
        }
        public override async Task<CreateTodoResponse> Create(CreateTodoRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You shoud compelte the required fields"));
            }

            var todo = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
            };

            await _context.TodoItems.AddAsync(todo);
            await _context.SaveChangesAsync();

            return new CreateTodoResponse { Id = todo.Id };
        }

        public override async Task<GetAllResponse> GetAll(GetAllRequest request, ServerCallContext context)
        {
            var todoItems = await _context.TodoItems.ToListAsync();

            var response = new GetAllResponse();
            response.TodoItems.AddRange(todoItems.Select(todo => new TodoItem
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                Status = todo.Status,
            }));

            return response;
        }

        public override async Task<GetByIdResponse> GetById(GetByIdRequest request, ServerCallContext context)
        {
            if(request.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Entity Id must be greater than 0"));
            }

            var todoItem = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

            return new GetByIdResponse { TodoItem = todoItem };
        }

        public override async Task<UpdateResponse> Update(UpdateRequest request, ServerCallContext context)
        {
            if (request.TodoItem.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Entity Id must be greater than 0"));
            }

            var todoItem = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == request.TodoItem.Id);

            if (todoItem == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"This id : {request.TodoItem.Id} is not found"));
            }

            todoItem.Title = request.TodoItem.Title;
            todoItem.Description = request.TodoItem.Description;
            todoItem.Status = request.TodoItem.Status;

            _context.TodoItems.Update(todoItem);
            await _context.SaveChangesAsync();
            
            return new UpdateResponse { TodoItem = todoItem };
        }

        public override async Task<DeleteResponse> Delete(DeleteRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Entity Id must be greater than 0"));
            }

            var todoItem = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (todoItem == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"This id : {request.Id} is not found"));
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return new DeleteResponse { };
        }
    }
}
