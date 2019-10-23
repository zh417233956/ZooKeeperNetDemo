using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFRAMEWORK
using ServiceStack.Redis;
#else
            
#endif



namespace Mango.Nodis
{
    public static class RedisHelper
    {
 #if NETFRAMEWORK
        public static readonly IRedisClientsManager Manager;

        static RedisHelper()
        {
            // 读取Redis主机IP配置信息
            // 有密码的格式：redis:password@127.0.0.1:6379
            // 无密码的格式：127.0.0.1:6379
            //string[] redisMasterHosts = ConfigurationManager.AppSettings["RedisServerIP"].Split(',');

            string[] redisMasterHosts = "127.0.0.1:20001".Split(',');

            // 如果Redis服务器是主从配置，则还需要读取Redis Slave机的IP配置信息
            string[] redisSlaveHosts = null;
            //var slaveConnection = ConfigurationManager.AppSettings["RedisSlaveServerIP"];
            string slaveConnection = null;
            if (!string.IsNullOrWhiteSpace(slaveConnection))
            {
                redisSlaveHosts = slaveConnection.Split(',');
            }

            // 读取RedisDefaultDb配置
            int defaultDb = 0;
            //string defaultDbSetting = ConfigurationManager.AppSettings["RedisDefaultDb"];
            string defaultDbSetting = "0";
            if (!string.IsNullOrWhiteSpace(defaultDbSetting))
            {
                int.TryParse(defaultDbSetting, out defaultDb);
            }

            var redisClientManagerConfig = new RedisClientManagerConfig
            {
                MaxReadPoolSize = 50,
                MaxWritePoolSize = 50,
                DefaultDb = defaultDb
            };

            // 创建Redis连接池
            Manager = new PooledRedisClientManager(redisMasterHosts, redisSlaveHosts, redisClientManagerConfig)
            {
                PoolTimeout = 2000,
                ConnectTimeout = 500
            };

        }

        /// <summary>
        /// 创建Redis连接
        /// </summary>
        /// <returns></returns>
        public static IRedisClient GetRedisClient()
        {
            try
            {
                var client = Manager.GetClient();
                return client;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
#else
            
#endif
    }
}
