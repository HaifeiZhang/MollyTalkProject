using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MollyTalkProject.Models.Entities;

namespace MollyTalkProject.Models
{
    public class MollyDesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdDbContext>
    {

        public IdDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<IdDbContext> builder = new();
            //string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:Default");
            string connStr = Environment.GetEnvironmentVariable("MollySqlServerConnStr");
            builder.UseSqlServer(connStr);
            return new IdDbContext(builder.Options);
        }
    }
}
