using Microsoft.AspNetCore.Identity;

namespace MollyTalkProject.Models.Entities
{
    public class User : IdentityUser<long>
    {
        public DateTime CreationTime { get; set; }
        public string? NickName { get; set; }
        public string? AvatarUrl { get; set; }
        public int? Gender { get; set; }
        public string? LocCountry { get; set; }
        public string? City { get; set; }
        public string? Language { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>(); 

    }
}
