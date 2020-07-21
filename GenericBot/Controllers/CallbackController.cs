using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GenericBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        // GET: api/callback
        [HttpGet]
        public IActionResult Get()
        {
            string code = Request.Query["code"];
            if (code == null)
                return new BadRequestResult();

            WebClient client = new WebClient();
            client.Headers.Clear();
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            var reqparm = new System.Collections.Specialized.NameValueCollection();
            reqparm.Add("grant_type", "authorization_code");
            reqparm.Add("code", code);
            reqparm.Add("redirect_uri", Core.GlobalConfig.RedirectUri);
            reqparm.Add("client_id", Core.GlobalConfig.OAuthClientId);
            reqparm.Add("client_secret", Core.GlobalConfig.OAuthClientSecret);
            reqparm.Add("scope", "guilds identify");

            try
            {
                byte[] responsebytes = client.UploadValues("https://discordapp.com/api/v6/oauth2/token", "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                var responseType = new
                {
                    access_token = "",
                };
                return new OkObjectResult(JsonConvert.DeserializeAnonymousType(responsebody, responseType).access_token);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            return new BadRequestResult();
        }
    }
}
