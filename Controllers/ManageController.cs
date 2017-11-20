
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using db;
using System.Data;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    public class ManageController : Controller
    {
        readonly ILogger<ManageController> _log;

        public ManageController(ILogger<ManageController> log)
        {
            _log = log;
        }
        // GET api/manage
        [HttpGet]
        public JsonResult Get()
        {
            //_log.LogInformation("api/manage/get");
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Console.WriteLine(Agent.ConString);
            using (DataTable dt = Agent.ExecuteDataTable("EXEC GET_TOY_INFO"))
            {
                rows = Agent.convertDataTable(dt);
            }
            return Json(rows);
        }
    }
}