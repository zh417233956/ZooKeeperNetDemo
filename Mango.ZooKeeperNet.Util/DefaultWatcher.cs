using log4net;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.ZooKeeperNet.Util
{
    /// <summary>
    /// ZooKeeperClient的默认监听类
    /// 可继承重写其监听实现
    /// </summary>
    public class DefaultWatcher
    {
        public static ILog _log;
        /// <summary>
        /// ZooKeeper客户端
        /// </summary>
        public ZooKeeperClient _zk;
        /// <summary>
        /// zk重连状态标记 0连接状态，1断开连接
        /// </summary>
        public int _zkReconnPoolState = 0;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="log"></param>
        /// <param name="zk"></param>
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
        /// <summary>
        /// 监听实现
        /// 可重写
        /// </summary>
        public virtual void ProcessWatched()
        {
            _zk.SubscribeStatusChange((ct, args) =>
            {
                _log.InfoFormat("SubscribeStatusChange接收到ZooKeeper服务端的通知，State是：{0}", args.State);
                if (args.State == org.apache.zookeeper.Watcher.Event.KeeperState.Disconnected)
                {
                    _log.ErrorFormat("SubscribeStatusChange接收到ZooKeeper服务端的通知，State是：{0}", args.State);
                    _zkReconnPoolState = 1;
                    var waitState = false;
                    while (!waitState)
                    {
                        _log.InfoFormat("RetryUntilConnected，State是：{0}", waitState);
                        try
                        {
                            //重连
                            _zk = _zk.ReRetryConnect();
                        }
                        catch (Exception ex)
                        {
                            _log.ErrorFormat("RetryUntilConnected，异常:{0}", ex.Message.ToString());
                        }
                        //网络断开重试 间隔30秒
                        waitState = _zk.client.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected, TimeSpan.FromSeconds(30));
                    }
                    _log.InfoFormat("RetryUntilConnected，上线成功");                   
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
            //    _log.InfoFormat("SubscribeChildrenChange接收到ZooKeeper服务端的通知，EventType是：{0}，Path是：{1}", eventType, path ?? string.Empty);
            //    return CompletedTask;
            //});
        }
    }
}
