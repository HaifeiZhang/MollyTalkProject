using MollyTalkProject.Models.Entities;

namespace MollyTalkProject.Controllers.RequestModels
{
    public class ChatMsgRecord
    {
        public string Token { get; set; }
        public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
