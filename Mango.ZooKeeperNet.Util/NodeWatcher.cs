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
    /// 节点监听
    /// </summary>
    public class NodeWatcher : Watcher
    {
        private readonly ILog _log;
        private string _state;
        private Event.EventType _type;
        private string _path;
        internal static readonly Task CompletedTask = Task.FromResult(1);


        public NodeWatcher(ILog log)
        {
            _log = log;
        }

        public override Task process(WatchedEvent @event)
        {
            _state = @event.getState().ToString();
            _type = @event.get_Type();
            _path = @event.getPath();
            switch (_type)
            {
                case Event.EventType.NodeCreated:
                    HandleCreate();
                    break;
                case Event.EventType.NodeDeleted:
                    HandleDelete();
                    break;
                case Event.EventType.NodeDataChanged:
                    HandleDataChange();
                    break;
                case Event.EventType.NodeChildrenChanged:
                    HandleChildrenChange();
                    break;
                default:
                    HandleNone();
                    break;
            }
            return CompletedTask;
        }


        /// <summary>
        /// 创建节点事件
        /// </summary>
        private void HandleCreate()
        {
            _log.InfoFormat("NodeCreated");
        }


        private void HandleDelete()
        {
            _log.InfoFormat("NodeDeleted");
        }


        private void HandleDataChange()
        {
            _log.InfoFormat("NodeDataChanged");
        }


        private void HandleChildrenChange()
        {
            _log.InfoFormat("NodeChildrenChanged");
        }


        private void HandleNone()
        {
            _log.InfoFormat(_state);
        }
    }
}
