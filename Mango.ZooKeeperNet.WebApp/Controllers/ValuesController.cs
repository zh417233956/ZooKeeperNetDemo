using log4net;
using Mango.Nodis;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Mango.ZooKeeperNet.WebApp.Controllers
{
    public class ValuesController : ApiController
    {
        private static ILog log = LogManager.GetLogger(typeof(ValuesController));
        // GET api/values
        public string Get()
        {
            string result = "null";
            try
            {
                using (var redisClient = RedisPoolManager.GetClient())
                {
                    redisClient.Db = 5;
                    var value = redisClient.Get<string>("codis-proxy");
                    result = value;
                    log.DebugFormat("get:{0}:{1},{2}={3}", redisClient.Host, redisClient.Port, "codis-proxy", value);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("异常信息:{0}", ex.Message.ToString());
            }
            return result;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
    public static class RedisPoolManager
    {
        private static RedisPool redisPool;
        static RedisPoolManager()
        {

        }
        public static void Init(string zkhosts, string db_proxy)
        {
            redisPool = RoundRobinSSRedisPool.Create().CuratorClient(zkhosts, 5000).ZkProxyDir(db_proxy).PoolConfig(2).Build();
        }
        public static IRedisClient GetClient()
        {
            return redisPool.GetClient();
        }
    }
}
