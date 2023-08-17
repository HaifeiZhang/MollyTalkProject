using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MollyTalkProject.Models.Entities;

namespace MollyTalkProject.Models.Configs
{
    public class ChatMessageConfig: IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("M_ChatMessages");
            builder.HasOne<User>(c=>c.User).WithMany(u => u.ChatMessages).IsRequired();
        }
    }

}
