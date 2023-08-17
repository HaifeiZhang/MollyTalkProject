namespace MollyTalkProject.Controllers.RequestModels
{
    public class WXHttpClientResponse
    {
        public string session_key { get; set; }
        public string openid { get; set;}

        public string errcode { get; set; }
        public string errmsg { get; set; }

    }
}
