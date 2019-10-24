using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
#if NETFRAMEWORK
using ServiceStack.Redis;
#else
            
#endif

namespace Mango.Nodis
{
    public class RedisPool
    {
#if NETFRAMEWORK
        private ILog log = LogManager.GetLogger(typeof(RedisPool));

        private IRedisClientsManager Manager;

        #region ZK配置
        private string zkAddr;
        private int zkSessionTimeoutMs;
        private string zkProxyDir;
        private ZooKeeperHelper zkhelper;
        public RedisPool CuratorClient(string zkAddr, int zkSessionTimeoutMs = 20)
        {
            this.zkAddr = zkAddr;
            this.zkSessionTimeoutMs = zkSessionTimeoutMs;
            return this;
        }

        public RedisPool ZkProxyDir(string zkProxyDir)
        {
            this.zkProxyDir = zkProxyDir;
            return this;
        }
        #endregion ZK配置

        public IRedisClientsManager Build()
        {
            #region zk配置获取及建立监听
            validate();
            if (zkhelper != null)
            {
                zkhelper.Dispose();
            }
            zkhelper = new ZooKeeperHelper(log, zkAddr, zkProxyDir, zkSessionTimeoutMs);                      
            zkhelper.addNodeDel = (nodes) =>
            {
                CreateManager();
            };
            zkhelper.deleteNodeDel = (nodes) =>
            {
                CreateManager();
            };
            #endregion zk配置获取及建立监听

            CreateManager();

            return Manager;
        }
        /// <summary>
        /// 建立连接池
        /// </summary>
        private void CreateManager()
        {
            // 读取Redis主机IP配置信息
            // 有密码的格式：redis:password@127.0.0.1:6379
            // 无密码的格式：127.0.0.1:6379
            var pools = zkhelper.pools;
            string[] redisMasterHosts = pools.Select(m => m.Addr).ToArray();

            // 如果Redis服务器是主从配置，则还需要读取Redis Slave机的IP配置信息
            string[] redisSlaveHosts = null;
            //string slaveConnection = null;
            //if (!string.IsNullOrWhiteSpace(slaveConnection))
            //{
            //    redisSlaveHosts = slaveConnection.Split(',');
            //}
            // RedisDefaultDb配置
            int defaultDb = 0;
            var redisClientManagerConfig = new RedisClientManagerConfig
            {
                MaxReadPoolSize = 1,
                MaxWritePoolSize = 1,
                DefaultDb = defaultDb,
                AutoStart = true
            };
            // 创建Redis连接池
            if (Manager != null)
            {
                Manager.Dispose();
            }
            Manager = new PooledRedisClientManager(redisMasterHosts, redisSlaveHosts, redisClientManagerConfig)
            {
                PoolTimeout = 2000,
                ConnectTimeout = 1000
            };
        }
        private void validate()
        {
            if (string.IsNullOrEmpty(zkProxyDir))
            {
                throw new Exception("zkProxyDir can not be null");
            }
            if (string.IsNullOrEmpty(zkAddr))
            {
                throw new Exception("zk client can not be null");
            }
        }
#else
            
#endif
    }
}
