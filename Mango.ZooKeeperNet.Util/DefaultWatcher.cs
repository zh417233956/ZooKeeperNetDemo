using log4net;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.ZooKeeperNet.Util
{
    public class DefaultWatcher
    {
        public static ILog _log;
        public ZooKeeperClient _zk;
        public DefaultWatcher(ILog log, ZooKeeperClient zk)
        {
            if (log == null)
            {
                log= LogManager.GetLogger(typeof(DefaultWatcher));
            }
            _log = log;
            _zk = zk;
        }

        public static readonly Task CompletedTask = Task.FromResult(1);
        public virtual void ProcessWatched()
        {
            _zk.SubscribeStatusChange((ct, args) =>
            {
                _log.DebugFormat("SubscribeStatusChange接收到ZooKeeper服务端的通知，State是：{0}", args.State);
                if (args.State == org.apache.zookeeper.Watcher.Event.KeeperState.Disconnected)
                {
                    var waitState = false;
                    while (!waitState)
                    {
                        _log.DebugFormat("RetryUntilConnected，State是：{0}", waitState);
                        try
                        {
                            //重连
                            _zk = _zk.ReRetryConnect();
                        }
                        catch (Exception ex)
                        {
                            _log.DebugFormat("RetryUntilConnected，等待连接:{0}", ex.ToString());
                        }
                        waitState = _zk.client.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected, TimeSpan.FromSeconds(30));
                    }
                    _log.DebugFormat("RetryUntilConnected，上线成功");
                    //启用监听
                    ProcessWatched();
                }
                return CompletedTask;
            });
            //_zk.SubscribeChildrenChange("/jodis.mango", (ct, args) =>
            //{
            //    IEnumerable<string> currentChildrens = args.CurrentChildrens;
            //    string path = args.Path;
            //    Watcher.Event.EventType eventType = args.Type;

            //    if (args.Type == Watcher.Event.EventType.NodeChildrenChanged)
            //    {
            //    }
            //    _log.DebugFormat("SubscribeChildrenChange接收到ZooKeeper服务端的通知，EventType是：{0}，Path是：{1}", eventType, path ?? string.Empty);
            //    return CompletedTask;
            //});
        }
    }
}
