using Microsoft.EntityFrameworkCore;
using CommandAPI.Models;
namespace CommandAPI.Data
{
    // the DbContext's job is to persist local changes and save them to the database when called to.
    public class CommandContext : DbContext
    {
        public CommandContext(DbContextOptions<CommandContext> options) : base(options)
        {
            
        }
        public DbSet<Command> CommandItems {get; set;}
    }
}