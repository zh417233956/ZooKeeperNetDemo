using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

#if NETFRAMEWORK
using ServiceStack.Redis;
#else
using StackExchange.Redis;
#endif

namespace Mango.Nodis
{
    /// <summary>
    /// Redis连接池帮助类
    /// </summary>
    public class RedisPool
    {
#if NETFRAMEWORK
        #region NETFRAMEWORK
        private ILog log = LogManager.GetLogger(typeof(RedisPool));

        private IRedisClientsManager Manager;

        int maxWritePoolSize = 1;
        int defaultDb = 0;
        /// <summary>
        /// 连接池设置
        /// </summary>
        /// <param name="poolSize">连接池大小</param>
        /// <param name="defaultdb"></param>
        /// <returns></returns>
        public RedisPool PoolConfig(int poolSize, int defaultdb = 0)
        {
            this.maxWritePoolSize = poolSize;
            this.defaultDb = defaultdb;
            return this;
        }
        /// <summary>
        /// 构建一个监听zk变化自动更新的连接池
        /// </summary>
        /// <returns></returns>
        public RedisPool Build()
        {
        #region zk配置获取及建立监听
            validate();
            if (zkhelper != null)
            {
                zkhelper.Dispose();
            }
            zkhelper = new ZooKeeperHelper(log, zkAddr, zkProxyDir, zkSessionTimeoutMs,
                (nodes) =>
                {
                    foreach (var item in nodes)
                    {
                        log.InfoFormat("新增节点：{0}", item.Addr);
                    }
                    CreateManager();
                },
                (nodes) =>
                {
                    foreach (var item in nodes)
                    {
                        log.InfoFormat("删除节点：{0}", item.Addr);
                    }
                    CreateManager();
                });

        #region 打日志
            //TODO:日志代码
            var pools = zkhelper.pools;
            foreach (var itemCodisProxy in pools)
            {
                log.InfoFormat($"加载节点:{itemCodisProxy.Node}={itemCodisProxy.Addr}-{itemCodisProxy.State}");
            }
        #endregion 打日志

        #endregion zk配置获取及建立监听

            CreateManager();

            return this;
        }

        /// <summary>
        /// 获取Redis连接
        /// </summary>
        /// <returns></returns>
        public IRedisClient GetClient()
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

            var redisClientManagerConfig = new RedisClientManagerConfig
            {
                MaxWritePoolSize = maxWritePoolSize,
                DefaultDb = defaultDb,
                AutoStart = true
            };
            // 创建Redis连接池
            if (Manager != null)
            {
                Manager.Dispose();
                log.InfoFormat("销毁Redis连接池完成");
            }

        #region 只为打日志
            string redisMasterHostsStr = "";
            foreach (var itemHost in redisMasterHosts)
            {
                redisMasterHostsStr += itemHost + ",";
            }
            log.InfoFormat("创建Redis连接池，RedisMasterHosts：{0}", redisMasterHostsStr.TrimEnd(','));
        #endregion 只为打日志

            Manager = new PooledRedisClientManager(redisMasterHosts, redisSlaveHosts, redisClientManagerConfig)
            {
                PoolTimeout = 2000,
                ConnectTimeout = 1000
            };
        }
        #endregion NETFRAMEWORK
#else
        #region NETSTANDARD


        private ILog log = LogManager.GetLogger(typeof(RedisPool));
        int defaultDb = 0;
        /// <summary>
        /// 默认redis连接DB设置
        /// </summary>
        /// <param name="defaultdb"></param>
        /// <returns></returns>
        public RedisPool DefaultDB(int defaultdb = 0)
        {
            this.defaultDb = defaultdb;
            return this;
        }
        /// <summary>
        /// 构建一个监听zk变化自动更新的连接实例
        /// </summary>
        /// <returns></returns>
        public RedisPool Build()
        {
            #region zk配置获取及建立监听
            validate();
            if (zkhelper != null)
            {
                zkhelper.Dispose();
            }
            zkhelper = new ZooKeeperHelper(log, zkAddr, zkProxyDir, zkSessionTimeoutMs,
                (nodes) =>
                {
                    foreach (var item in nodes)
                    {
                        log.InfoFormat("新增节点：{0}", item.Addr);
                    }
                    CreateInstance();
                },
                (nodes) =>
                {
                    foreach (var item in nodes)
                    {
                        log.InfoFormat("删除节点：{0}", item.Addr);
                    }
                    CreateInstance();
                });

            #region 打日志
            //TODO:日志代码
            var pools = zkhelper.pools;
            foreach (var itemCodisProxy in pools)
            {
                log.InfoFormat($"加载节点:{itemCodisProxy.Node}={itemCodisProxy.Addr}-{itemCodisProxy.State}");
            }
            #endregion 打日志

            #endregion zk配置获取及建立监听

            CreateInstance();

            return this;
        }

        /// <summary>
        /// 获取Redis连接实例
        /// </summary>
        /// <returns></returns>
        public ConnectionMultiplexer GetInstance()
        {
            try
            {
                if (Instance.IsConnected)
                {
                    return Instance;
                }
                else
                {
                    throw new Exception("连接实例已断开");
                }
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat("获取Redis连接实例异常:{0}", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 连接实例
        /// </summary>
        private ConnectionMultiplexer Instance = null;

        /// <summary>
        /// 使用一个静态属性来返回已连接的实例，如下列中所示。这样，一旦 ConnectionMultiplexer 断开连接，便可以初始化新的连接实例。
        /// </summary>
        public void CreateInstance()
        {
            string redisMasterHostsStr = "";
            foreach (var itemHost in zkhelper.pools)
            {
                redisMasterHostsStr += itemHost.Addr + ",";
            }
            var constr = "{0}DefaultDatabase={1}";

            if (Instance != null)
            {
                ConnectionMultiplexer oldInstance = null;
                lock (Instance)
                {
                    oldInstance = Instance;                   
                    log.InfoFormat("重新创建Redis实例，RedisHosts：{0}", redisMasterHostsStr.TrimEnd(','));
                    Instance = ConnectionMultiplexer.Connect(string.Format(constr, redisMasterHostsStr, defaultDb));
                }
                System.Threading.Thread.Sleep(1000);
                oldInstance.CloseAsync();
                oldInstance.Dispose();
                log.InfoFormat("销毁Redis实例完成");
            }
            else
            {
                #region 只为打日志         
                log.InfoFormat("创建Redis实例，RedisHosts：{0}", redisMasterHostsStr.TrimEnd(','));
                #endregion 只为打日志            
                Instance = ConnectionMultiplexer.Connect(string.Format(constr, redisMasterHostsStr, defaultDb));
            }
        }
        #endregion
#endif

        #region ZK配置
        private string zkAddr;
        private int zkSessionTimeoutMs;
        private string zkProxyDir;
        private ZooKeeperHelper zkhelper;
        /// <summary>
        /// ZK信息配置
        /// </summary>
        /// <param name="zkAddr"></param>
        /// <param name="zkSessionTimeoutMs"></param>
        /// <returns></returns>
        public RedisPool CuratorClient(string zkAddr, int zkSessionTimeoutMs = 20)
        {
            this.zkAddr = zkAddr;
            this.zkSessionTimeoutMs = zkSessionTimeoutMs;
            return this;
        }
        /// <summary>
        /// CodisProxy实例节点设置
        /// </summary>
        /// <param name="zkProxyDir"></param>
        /// <returns></returns>
        public RedisPool ZkProxyDir(string zkProxyDir)
        {
            this.zkProxyDir = zkProxyDir;
            return this;
        }
        #endregion ZK配置
        /// <summary>
        /// 参数校验检查
        /// </summary>
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
    }
}
