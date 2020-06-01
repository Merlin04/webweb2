using BlazorEditor.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorEditor.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Component> Components { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Layout> Layouts { get; set; }
        public DbSet<CompiledPage> CompiledPages { get; set; }
        public DbSet<RegisterToken> RegisterTokens { get; set; }
        public DbSet<ConfigItem> Configuration { get; set; }
    }
}