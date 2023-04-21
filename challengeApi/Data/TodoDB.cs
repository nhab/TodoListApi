using challengeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace challengeApi.Data
{
    public class TodoDb : DbContext
    {
        public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

        public DbSet<Todo> Todos => Set<Todo>();
       
    }
}
