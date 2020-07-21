using System.Net;
using GenericBot.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GenericBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        // GET: api/userinfo
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                WebClient client = new WebClient();

                client.Headers.Add("Authorization", $"Bearer {Request.Headers["Authorization"]}");
                string userData = client.DownloadString("https://discordapp.com/api/v6/users/@me");

                PartialUser pUser = JsonConvert.DeserializeObject<PartialUser>(userData);

                return new OkObjectResult(pUser);
            }
            catch
            {
                return new BadRequestResult();
            }
        }
    }
}
