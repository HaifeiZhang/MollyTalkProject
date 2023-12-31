﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MollyTalkProject.Controllers.RequestModels;
using MollyTalkProject.FileHelper;
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
        private readonly MollyDBContext dBContext;
        private readonly ILogger<ChatRobotController> logger;

        public ChatRobotController(UserManager<User> userManager, MollyDBContext dBContext, ILogger<ChatRobotController> logger)
        {
            this.userManager = userManager;
            this.dBContext = dBContext;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<string> ChatRobotReponse(PromptMsg promptMsg)
        {
            try
            {
                if (promptMsg == null || string.IsNullOrEmpty(promptMsg.Prompt))
                {
                    return "Hello, do you have a question? Let's Chat!";
                }

                string apiKey = Environment.GetEnvironmentVariable("ChatRobotApiKey");
                if (string.IsNullOrEmpty(apiKey))
                {
                    return "Sorry, I'm not ready.";
                }
                string prompt = promptMsg.Prompt;
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
                    SaveApiResponseToDB(prompt, responseContent);
                    SaveApiResponseToFile(prompt, responseContent);
                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}");
                return "something wrong";
            }
            



        }

        private async void SaveApiResponseToFile(string prompt, string responseContent)
        {
            try
            {
                long userId = long.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                ChatMessage chatMessage = new ChatMessage()
                {
                    QuestionMsg = prompt,
                    AnswerMsg = responseContent,
                    CreateTime = DateTime.Now,
                    UserId = userId
                };
                WriteToTxtHelper.DataToTxt(chatMessage);
            }
            catch (Exception ex)
            {
                logger.LogError($"save msg to text failed {ex.Message}");
            }

        }

        private async void SaveApiResponseToDB(string prompt, string responseContent)
        {
            try
            {
                long userId = long.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                ChatMessage chatMessage = new ChatMessage()
                {
                    QuestionMsg = prompt,
                    AnswerMsg = responseContent,
                    CreateTime = DateTime.Now,
                    UserId = userId
                };

                dBContext.ChatMessages.Add(chatMessage);
                await dBContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }


        }
    }
}
