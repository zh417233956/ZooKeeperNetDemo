using log4net;
using Mango.Nodis;
using Mango.ZooKeeperNet.Util;
using ServiceStack.Redis;
using System;

namespace Mango.ZooKeeperNet.ConsoleApp
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(typeof(Program));

        static void Main0(string[] args)
        {
            var codiswatcher = new MyCodisWatcher(log, new ZooKeeperClient("192.168.4.144:20000"));
            codiswatcher.ProcessWatched();
            Console.ReadKey();
        }
        static void Main1(string[] args)
        {
            var zkhelper = new Nodis.ZooKeeperHelper(log, "192.168.4.144:20000", "mango");
            Console.ReadKey();
        }
        static void Main2(string[] args)
        {
            var zk = new ZooKeeperClient("192.168.4.77:2181");
            var data = zk.client.ExistsAsync("/jodis");
            Console.WriteLine(data.Result);

            Console.ReadKey();
        }
        static void Main3(string[] args)
        {
            var zkhelper = new Nodis.ZooKeeperHelper(log, "192.168.4.77:2181", "codis-zxf");
            var pools = zkhelper.pools;
            foreach (var itemCodisProxy in pools)
            {
                log.DebugFormat($"加载节点:{itemCodisProxy.Node}={itemCodisProxy.Addr}-{itemCodisProxy.State}");
            }
            zkhelper.addNodeDel = (nodes) =>
            {
                foreach (var item in nodes)
                {
                    log.InfoFormat("新增节点：{0}", item.Addr);
                }
            };
            zkhelper.deleteNodeDel = (nodes) =>
            {
                foreach (var item in nodes)
                {
                    log.InfoFormat("删除节点：{0}", item.Addr);
                }
            };
            Console.ReadKey();
        }
        static void Main(string[] args)
        {
            var redisPool = RoundRobinSSRedisPool.Create().CuratorClient("192.168.4.77:2181", 5000).ZkProxyDir("codis-zxf")
               .Build();
            //使用连接池查询
            using (var redisClient = redisPool.GetClient())
            {
                redisClient.Db = 5;
                var value = redisClient.Get<string>("k1");
                log.InfoFormat("查询redis:k1={0}", value);
            }

            //直接查询
            using (RedisClient redisClient = new RedisClient("192.168.4.79", 6379))
            {
                redisClient.Db = 5;
                var value = redisClient.Get<string>("k1");
                log.InfoFormat("查询redis:k1={0}", value);
            }


            Console.ReadKey();
        }
    }
    public class MyCodisWatcher : DefaultWatcher
    {
        public MyCodisWatcher(ILog log, ZooKeeperClient zk) : base(log, zk)
        {
        }
        public override void ProcessWatched()
        {
            base.ProcessWatched();
            _zk.SubscribeChildrenChange("/jodis.mango", (ct, args) =>
            {
                _log.DebugFormat("MyCodisWatcher接收到ZooKeeper服务端的通知，EventType是：{0}，Path是：{1}", args.Type, args.Path ?? string.Empty);
                return CompletedTask;
            });
        }
    }
}
