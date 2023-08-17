using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MollyTalkProject.Controllers.UserAdmin
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserAdminController : ControllerBase
    {
        //后端管理帐号登陆
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(UserAdminRequest userAdminRequest)
        {
            if (userAdminRequest == null) {  return BadRequest(); }
            return Ok();
        }

        //后端处理方法
    }
}
