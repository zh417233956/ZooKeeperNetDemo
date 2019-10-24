using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Nodis
{
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
}
