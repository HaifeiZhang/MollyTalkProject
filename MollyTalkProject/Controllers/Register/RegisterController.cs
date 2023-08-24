using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MollyTalkProject.Models.Entities;
using MollyTalkProject.Models;
using MollyTalkProject.Controllers.RequestModels;
using Azure.Core;
using System.Data;

namespace MollyTalkProject.Controllers.Register
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        public readonly MollyDBContext dBContext;
        public readonly ILogger<RegisterController> logger;

        public RegisterController(UserManager<User> userManager, RoleManager<Role> roleManager, MollyDBContext dBContext, ILogger<RegisterController> logger)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.dBContext = dBContext;
            this.logger = logger;
        }

        //[HttpPost]
        //public async Task<ActionResult> RegisterByUserNameAndPassword([FromBody] UserNameAndPwdRequest request)
        //{
        //    string username = request.UserName;
        //    string pwd = request.Pwd;
        //    bool roleExists = await roleManager.RoleExistsAsync("Common");
        //    if (!roleExists)
        //    {
        //        Role role = new Role { Name = "Common" };
        //        var r = await roleManager.CreateAsync(role);
        //        if (!r.Succeeded)
        //        {
        //            return BadRequest(r.Errors);
        //        }
        //    }
        //    User user = await this.userManager.FindByNameAsync(username);
        //    if (user == null)
        //    {
        //        user = new User { UserName = username, NickName = username };
        //        var r = await userManager.CreateAsync(user, pwd);
        //        if (!r.Succeeded)
        //        {
        //            return BadRequest(r.Errors);
        //        }
        //        r = await userManager.AddToRoleAsync(user, "admin");
        //        if (!r.Succeeded)
        //        {
        //            return BadRequest(r.Errors);
        //        }
        //    }
        //    return Ok();
        //}

        [HttpPost]
        public async Task<ActionResult> RegisterByEmail([FromBody] EmailRequest request)
        {
            try
            {
                string email = request.EmailInfo;
                string pwd = request.Pwd;
                string username = request.UserName;
                bool roleExists = await roleManager.RoleExistsAsync("Common");
                if (!roleExists)
                {
                    Role role = new Role { Name = "Common" };
                    var r = await roleManager.CreateAsync(role);
                    if (!r.Succeeded)
                    {
                        logger.LogError($"{role.Name}创建角色失败！");
                        return BadRequest(r.Errors);
                    }
                }

                User user = await this.userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var users = this.userManager.Users.ToList();
                    if (users.Select(u => u.UserName).ToList().Contains(user.UserName))
                    {
                        logger.LogError($"{email}用户已经存在！");
                        return Forbid("用户名已经存在，请更换用户名！");
                    }
                }
                if (user == null)
                {
                    user = new User { UserName = username, NickName = "邮箱用户", Email = email, EmailConfirmed = true };
                    var r = await userManager.CreateAsync(user, pwd);
                    if (!r.Succeeded)
                    {
                        logger.LogError($"{email}创建用户失败！");
                        return BadRequest(r.Errors);
                    }
                    r = await userManager.AddToRoleAsync(user, "Common");
                    if (!r.Succeeded)
                    {
                        logger.LogError($"{email}添加角色失败！");
                        return BadRequest(r.Errors);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"{request.EmailInfo}创建用户失败！");
                logger.LogError($"{ex.Message}");
                return BadRequest($"{request.EmailInfo}创建用户失败！");

            }


        }

        [HttpPost]
        public async Task<ActionResult> RegisterByPhone([FromBody] PhoneRequest request)
        {
            try
            {
                string phone = request.PhoneNum;
                string username = request.UserName;
                string pwd = request.Pwd;
                bool roleExists = await roleManager.RoleExistsAsync("Common");
                if (!roleExists)
                {
                    Role role = new Role { Name = "Common" };
                    var r = await roleManager.CreateAsync(role);
                    if (!r.Succeeded)
                    {
                        logger.LogError($"{role.Name}创建角色失败！");
                        return BadRequest(r.Errors);
                    }
                }

                User user = await this.userManager.FindByNameAsync(username);
                if (user != null)
                {
                    var users = this.userManager.Users.ToList();
                    if (users.Select(u => u.UserName).ToList().Contains(user.UserName))
                    {
                        logger.LogError($"{phone}用户已经存在！");
                        return Forbid("用户名已经存在，请更换用户名！");
                    }
                }
                if (user == null)
                {
                    user = new User { UserName = username, NickName = "邮箱用户", PhoneNumber = phone, PhoneNumberConfirmed = true };
                    var r = await userManager.CreateAsync(user, pwd);
                    if (!r.Succeeded)
                    {
                        logger.LogError($"{phone}创建用户失败！");
                        return BadRequest(r.Errors);
                    }
                    r = await userManager.AddToRoleAsync(user, "Common");
                    if (!r.Succeeded)
                    {
                        logger.LogError($"{phone}添加角色失败！");
                        return BadRequest(r.Errors);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError($"{request.PhoneNum}创建用户失败！");
                logger.LogError($"{ex.Message}");
                return BadRequest($"{request.PhoneNum}创建用户失败！");
            }
            

        }


    }
}
