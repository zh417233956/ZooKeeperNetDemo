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
#endif
    }
}
