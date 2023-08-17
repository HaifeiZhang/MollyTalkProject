namespace MollyTalkProject.Models.Entities
{
    public class ChatMessage
    {
        public long Id { get; set; }
        public string QuestionMsg { get; set; }
        public string AnswerMsg { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public User User { get; set; }
        public long UserId { get; set; }
    }
}
