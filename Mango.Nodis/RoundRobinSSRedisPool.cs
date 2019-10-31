#if NETFRAMEWORK

#else
using StackExchange.Redis;
#endif

namespace Mango.Nodis
{
#if NETFRAMEWORK

    /// <summary>
    /// Redis连接池实例化类
    /// </summary>
    public static class RoundRobinSSRedisPool
    {
        private static RedisPool redisPool;
        public static RedisPool Create()
        {
            if (redisPool == null)
            {
                redisPool = new RedisPool();
            }
            return redisPool;
        }
    }
#else
    /// <summary>
    /// Redis连接池实例化类
    /// </summary>
    public static class SERedisClient
    {
        private static RedisPool instance;
        public static RedisPool Create()
        {
            if (instance == null)
            {
                instance = new RedisPool();
            }
            return instance;
        }
    }
#endif
}
