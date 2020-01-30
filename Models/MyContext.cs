using Microsoft.EntityFrameworkCore;
 
namespace Books.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) { }

        public DbSet<Author> Authors {get;set;}
        public DbSet<Book> Books {get;set;}
    }
}
