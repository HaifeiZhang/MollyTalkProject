using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MollyTalkProject.Controllers.RequestModels;
using MollyTalkProject.Models;
using MollyTalkProject.Models.Entities;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace MollyTalkProject.Controllers.ChatRobot
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatRobotController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly MollyDesignTimeDbContextFactory dbContextFactory;

        public ChatRobotController(UserManager<User> userManager, MollyDesignTimeDbContextFactory dbContextFactory)
        {
            this.userManager = userManager;
            this.dbContextFactory = dbContextFactory;
        }

        [HttpGet]
        public async Task<string> ChatRobotReponse(UserMsg userMsg)
        {
            if (userMsg == null|| string.IsNullOrEmpty(userMsg.Msg))
            {
                return "Hello, do you have a question? Let's Chat!";
            }

            string apiKey = Environment.GetEnvironmentVariable("ChatRobotApiKey");
            if (string.IsNullOrEmpty(apiKey))
            {
                return "Sorry, I'm not ready.";
            }
            string prompt = userMsg.Msg;
            string apiUrl = "https://api.openai.com/v1/engines/davinci-codex/completions";

            var requestBody = new
            {
                prompt = prompt,
                max_tokens = 50  // 最大生成词数
            };
            var requestBodyJson = JsonConvert.SerializeObject(requestBody);
            using (var httpClient = new HttpClient())
            {
                // 添加请求头
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // 发送 POST 请求
                var response = await httpClient.PostAsync(apiUrl, new StringContent(requestBodyJson, Encoding.UTF8, "application/json"));

                // 处理响应
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Generated text:");
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("API request failed:");
                    Console.WriteLine(responseContent);
                    responseContent = $"API request failed:{responseContent}";
                }
                SaveApiResponseToDB(prompt,responseContent);
                SaveApiResponseToFile(prompt, responseContent);
                return responseContent;
            }



        }

        private async void SaveApiResponseToFile(string prompt, string responseContent)
        {
            long userId = long.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            ChatMessage chatMessage = new ChatMessage() 
            {
                QuestionMsg = prompt,
                AnswerMsg = responseContent,
                CreateTime = DateTime.Now,
                UserId =userId
            };

            //dbContextFactory.ChatMessages.Add(chatMessage);
            //dbContextFactory.SaveChanges();
        }

        private void SaveApiResponseToDB(string propmt, string responseContent)
        {
            throw new NotImplementedException();
        }
    }
}
