using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFRAMEWORK
using ServiceStack.Redis;
#else
using StackExchange.Redis;
#endif

namespace Mango.Nodis
{
    public static class RedisPoolBuilder
    {
#if NETFRAMEWORK
        private static RedisPool redisPool;
        public static void Init(string zkhosts, string db_proxy, int poolSize = 1, int defaultdb = 0)
        {
            redisPool = RoundRobinSSRedisPool.Create().CuratorClient(zkhosts, 5000).ZkProxyDir(db_proxy).PoolConfig(poolSize, defaultdb).Build();
        }
        public static IRedisClient GetClient()
        {
            return redisPool.GetClient();
        }
#else
        private static RedisPool instance;
        public static void Init(string zkhosts, string db_proxy, int defaultdb = 0)
        {
            instance = SERedisClient.Create().CuratorClient(zkhosts, 5000).ZkProxyDir(db_proxy).DefaultDB(defaultdb).Build();
        }
        /// <summary>
        /// 获取Redis连接实例
        /// </summary>
        /// <returns></returns>
        public static ConnectionMultiplexer GetInstance()
        {
            return instance.GetInstance();
        }
        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="db">-1为默认连接的数据库</param>
        /// <returns></returns>
        public static IDatabase GetDatabase(int db = -1)
        {
            return GetInstance().GetDatabase(db);
        }
#endif
    }
}
