using GrpcDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) 
        {

        }

        public virtual DbSet<TodoItem> TodoItems { get; set; }

    }
}
