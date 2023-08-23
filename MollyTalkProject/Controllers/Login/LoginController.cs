using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MollyTalkProject.Controllers.RequestModels;
using MollyTalkProject.Controllers.SetModels;
using MollyTalkProject.Models;
using MollyTalkProject.Models.Entities;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MollyTalkProject.Controllers.Login
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        public readonly MollyDBContext dBContext;
        private readonly ILogger<LoginController> logger;

        public LoginController(UserManager<User> userManager, RoleManager<Role> roleManager, MollyDBContext dBContext, ILogger<LoginController> logger)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.dBContext = dBContext;
            this.logger = logger;
        }


        //通过微信Code登录，生成OpenID登录
        [HttpPost]
        public async Task<ActionResult> LoginByWXCode([FromBody] WXCodeRequest wXCode)
        {
            if (wXCode==null || string.IsNullOrEmpty(wXCode.Code))
            {
                return BadRequest("Failed");
            }
            var code = wXCode.Code;
            var response = await GetOpenIDByWXCodeAsync(code);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"通过{code}获取OpendID 失败");
                return Problem("网络错误！");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject <WXHttpClientResponse>(responseBody);
            string openID = responseObj.openid;
            if (string.IsNullOrEmpty(openID))
            {
                return BadRequest(responseObj);
            }
            //用openID 用作UserName查询用户是否已经存在
            var user = await userManager.FindByNameAsync(openID);
            if (user==null)
            {
                //创建微信用户
                user = await CreateWXUserByOpendIDAysnc(openID);
            }
            if (user==null)
            {
                return BadRequest("Failed");
            }
            //返回历史聊天记录
            var returnObj =  dBContext.ChatMessages.Where(c => c.UserId == user.Id).OrderByDescending(c=>c.Id).Take(30).ToList();

            //生成Token
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            var roles = await userManager.GetRolesAsync(user);
            //可能有多个角色
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            string jwtToken = BuildToken(claims);
            if (string.IsNullOrEmpty(jwtToken))
            {
                return BadRequest("Failed");
            }
            ChatMsgRecord msgRecord = new ChatMsgRecord() 
            { 
                Token = jwtToken,
                ChatMessages = returnObj
            };
            return Ok(msgRecord);
        }

        private string BuildToken(IEnumerable<Claim> claims)
        {
            JWTOptions options = new JWTOptions() {
                SigningKey = Environment.GetEnvironmentVariable("JWTSecKey"),
                ExpireSeconds = int.Parse(Environment.GetEnvironmentVariable("JWTSecKeyExpireSeconds"))
            };
            DateTime expires = DateTime.Now.AddSeconds(options.ExpireSeconds);
            byte[] keyBytes = Encoding.UTF8.GetBytes(options.SigningKey);
            var secKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(secKey,
                SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(expires: expires,
                signingCredentials: credentials, claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private async Task<User> CreateWXUserByOpendIDAysnc(string openID)
        {
            bool roleExists = await roleManager.RoleExistsAsync("Common");
            if (!roleExists)
            {
                Role role = new Role { Name = "Common" };
                var r = await roleManager.CreateAsync(role);
                if (!r.Succeeded)
                {
                    return null;
                }
            }

            var user = new User { NickName = "微信用户", UserName = openID};
            var u = await userManager.CreateAsync(user, "Aa123456@");
            if (!u.Succeeded)
            {
                return null;
            }
            u = await userManager.AddToRoleAsync(user, "Common");
            //var c=await userManager.
            if (!u.Succeeded)
            {
                return null;
            }
            return user;
        }

        private async Task<HttpResponseMessage> GetOpenIDByWXCodeAsync(string code)
        {
            string appId = Environment.GetEnvironmentVariable("WXAppID");
            string appSecret = Environment.GetEnvironmentVariable("WXAppSecretKey");
            //string code = "USER_LOGIN_CODE"; // Replace with the actual user login code

            using (var httpClient = new HttpClient())
            {
                string requestUrl = $"https://api.weixin.qq.com/sns/jscode2session" +
                                    $"?appid={appId}&secret={appSecret}&js_code={code}&grant_type=authorization_code";

                return await httpClient.GetAsync(requestUrl);

                //if (response.IsSuccessStatusCode)
                //{
                //    string responseBody = await response.Content.ReadAsStringAsync();
                //    // Parse the JSON response to get OpenID and other data
                //    Console.WriteLine(responseBody);
                //    return responseBody;
                //}
                //else
                //{
                //    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                //    //要写日志
                //    return "";
                //}

               
            }
        }


        //通过帐号密码登录
        [HttpPost]
        public async Task<ActionResult> LoginByUserNameAndPassword([FromBody] UserNameAndPwdRequest request)
        {
            string userName = request.UserName;
            string password = request.Pwd;
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound($"用户名不存在{userName}");
            }
            string jwtToken = await CheckUserAccountAsync(user, request.Pwd);
            if (jwtToken == null)
            {
                return BadRequest("Failed");
            }
            return Ok(jwtToken);
        }

        //通过邮箱密码登录
        [HttpPost]
        public async Task<ActionResult> LoginByEmail([FromBody] EmailRequest request)
        {
            if (request == null)
            {
                return BadRequest("Failed");
            }
            User user = await userManager.FindByEmailAsync(request.EmailInfo);
            if (user == null)
            {
                return NotFound($"用户名不存在");
            }
            string jwtToken = await CheckUserAccountAsync(user,request.Pwd);
            if (jwtToken == null)
            {
                return BadRequest("Failed");
            }
            
            return Ok(jwtToken);
        }

        private async Task<string> CheckUserAccountAsync(User user, string pwd)
        {
            var success = await userManager.CheckPasswordAsync(user, pwd);
            if (!success)
            {
                if (user.AccessFailedCount==3)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
                }
                user.AccessFailedCount++;   
                return null ;
            }
            //解除锁定
            user.AccessFailedCount = 0;
            user.LockoutEnabled = false;
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            var roles = await userManager.GetRolesAsync(user);
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return BuildToken(claims);
        }

        
    }
}
