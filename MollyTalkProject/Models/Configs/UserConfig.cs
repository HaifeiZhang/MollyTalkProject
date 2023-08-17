using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MollyTalkProject.Models.Entities;

namespace MollyTalkProject.Models.Configs
{
    public class UserConfig: IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("M_Users");
            builder.Property(e => e.UserName).IsRequired().IsUnicode();
        }
    }
}
