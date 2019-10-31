using log4net;
using Mango.ZooKeeperNet.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Nodis
{
    /// <summary>
    /// zk配置及建立监听
    /// </summary>
    public class ZooKeeperHelper : IDisposable
    {
        private static ILog _log;
        public ZooKeeperClient _zk;
        private CodisWatcher.DeleteNodeDel deleteNodeDel;
        private CodisWatcher.AddNodeDel addNodeDel;
        CodisWatcher codiswatcher;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="log"></param>
        /// <param name="connectionString"></param>
        /// <param name="proxy"></param>
        /// <param name="SessionTimeout"></param>
        /// <param name="addNodeDel"></param>
        /// <param name="deleteNodeDel"></param>
        public ZooKeeperHelper(ILog log, string connectionString, string proxy, double SessionTimeout = 20, CodisWatcher.AddNodeDel addNodeDel = null, CodisWatcher.DeleteNodeDel deleteNodeDel = null)
        {
            _log = log;
            _zk = new ZooKeeperClient(connectionString, SessionTimeout);
            this.addNodeDel = addNodeDel;
            this.deleteNodeDel = deleteNodeDel;
            codiswatcher = new CodisWatcher(log, _zk, proxy, addNodeDel, deleteNodeDel);
            codiswatcher.ProcessWatched();
        }
        /// <summary>
        /// 当前的节点列表
        /// </summary>
        public List<CodisProxyInfo> pools => codiswatcher.GetPools();

        /// <summary>
        /// 执行与释放或重置非托管资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 获取或设置一个值。该值指示资源已经被释放。
        /// </summary>
        private bool _disposed;
        /// <summary>
        /// 由终结器调用以释放资源。
        /// </summary>
        ~ZooKeeperHelper()
        {
            Dispose(false);
        }
        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
            }
            // 标记已经被释放。
            _disposed = true;
        }
    }
}
