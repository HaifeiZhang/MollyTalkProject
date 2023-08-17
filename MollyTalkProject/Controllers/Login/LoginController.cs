using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MollyTalkProject.Controllers.RequestModels;
using MollyTalkProject.Controllers.SetModels;
using MollyTalkProject.Models.Entities;
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

        public LoginController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
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
            var openID = GetOpenIDByWXCode(code);
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
            return Ok(jwtToken);
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

        private string GetOpenIDByWXCode(string code)
        {
            throw new NotImplementedException();
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
            User user = await userManager.FindByEmailAsync(request.LoginInfo);
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
                return null ;
            }
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
