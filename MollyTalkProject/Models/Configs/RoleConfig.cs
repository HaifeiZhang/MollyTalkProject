using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MollyTalkProject.Models.Entities;

namespace MollyTalkProject.Models.Configs
{
    public class RoleConfig: IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("M_Roles");
        }
    }
}
