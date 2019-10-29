using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace Mango.ZooKeeperNet.WebAppCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static ILog log = LogManager.GetLogger(typeof(ValuesController));
        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {
            string result = "null";
            try
            {
                var redisClient = Nodis.RedisPoolBuilder.GetDatabase(5);
                var value = redisClient.StringGet("codis-proxy");
                result = value;
                log.DebugFormat("查询redis:{0}={1}", "codis-proxy", value);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("异常信息:{0}", ex.Message.ToString());
            }
            return result;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
