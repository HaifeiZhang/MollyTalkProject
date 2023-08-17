using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MollyTalkProject.Models.Entities;

namespace MollyTalkProject.Models
{
    public class IdDbContext: IdentityDbContext<User, Role, long>
    {
        public IdDbContext(DbContextOptions<IdDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);        }

    }
}
