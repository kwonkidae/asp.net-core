using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using db;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.AspNetCore.Http.Features;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        readonly ILogger<ValuesController> _log;

        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public ValuesController(ILogger<ValuesController> log) {
            _log = log;
        }
        private bool IsValidUserAndPasswordCombination(string username, string password)
        {
            return !string.IsNullOrEmpty(username) && username == password;
        }

        private string GenerateToken(string username)
        {
            
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
                new Claim("custome", "custome")
            };

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the secret that needs to be at least 16 characeters long for HmacSha256")),
                                             SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        // GET api/values
        [HttpGet]
        public JsonResult Get()
        {
            _log.LogInformation("Hello, world!");
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Console.WriteLine(Agent.ConString);
            using (DataTable dt = Agent.ExecuteDataTable("SELECT TOP (1000) * FROM[uLibrary].[dbo].[SL_USE_LOG_TBL]"))
            {
                Console.WriteLine("RowsCount: " + dt.Rows.Count);
                rows = Agent.convertDataTable(dt);
            }
            return Json(rows);
        }

        [Authorize(Policy = "OfficeNumberUnder200")]
        [HttpGet("auth")]
        public IActionResult GetUserDetails()
        {
            var claim = User.Identity as ClaimsIdentity;
            Console.WriteLine(claim.FindFirst("nbf"));
            Console.WriteLine(claim.FindFirst("custome"));

            Console.WriteLine(claim.Name);
            
            return new ObjectResult(new
            {
                Username = User.Identity.Name
            });
        }


        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            return new ObjectResult(GenerateToken("kkdosk"));
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

        public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        [HttpPost("imageupload")]
        public async Task<IActionResult> Post(List<IFormFile> file, string name)
        {
            Console.WriteLine(name);
            long size = file.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in file)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = file.Count, size, filePath });
        }

    }
}
