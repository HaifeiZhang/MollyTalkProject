using MollyTalkProject.Models.Entities;

namespace MollyTalkProject.FileHelper
{
    public class WriteToTxtHelper
    {
        public static void DataToTxt(ChatMessage chatMessage)
        {
            string content = chatMessage.CreateTime.ToString() + $" 用户ID：{chatMessage.UserId}; " +
                $"Question: {chatMessage.QuestionMsg}; " + $"Answer: {chatMessage.AnswerMsg}; ";
            string filePath = Environment.GetEnvironmentVariable("ChatMsgSavePath");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string fileName = filePath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            File.WriteAllText(fileName, content);
        }
    }
}
