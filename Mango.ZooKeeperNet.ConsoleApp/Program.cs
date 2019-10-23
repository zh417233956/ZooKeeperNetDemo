using log4net;
using Mango.Nodis;
using Mango.ZooKeeperNet.Util;
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
        static void Main(string[] args)
        {
            var zkhelper = new ZooKeeperHelper(log, "192.168.4.144:20000");
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
