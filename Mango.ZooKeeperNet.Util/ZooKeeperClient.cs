using org.apache.zookeeper;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.ZooKeeperNet.Util
{
    /// <summary>
    /// ZK连接客户端类
    /// 支持自动重连及重试，监听了节点变化事件
    /// </summary>
    public class ZooKeeperClient: IDisposable
    {
        /// <summary>
        /// ZooKeeper客户端
        /// </summary>
        public IZookeeperClient client;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">ZK地址，多个以“,”分割</param>
        /// <param name="SessionTimeout">会话超时时间(Second)</param>
        public ZooKeeperClient(string connectionString, double SessionTimeout = 20)
        {
            client = new ZookeeperClient(new ZookeeperClientOptions(connectionString)
            {
                BasePath = "/", //default value
                ConnectionTimeout = TimeSpan.FromSeconds(10), //default value
                SessionTimeout = TimeSpan.FromSeconds(SessionTimeout), //default value
                OperatingTimeout = TimeSpan.FromSeconds(60), //default value
                ReadOnly = false, //default value
                SessionId = 0, //default value
                SessionPasswd = null, //default value
                EnableEphemeralNodeRestore = true //default value
            });
        }
        /// <summary>
        /// 重新建立连接
        /// </summary>
        /// <returns></returns>
        public ZooKeeperClient ReRetryConnect()
        {
            client = new ZookeeperClient(client.Options);
            return this;
        }
        /// <summary>
        /// 订阅连接状态变更
        /// </summary>
        /// <param name="listener">监听者</param>
        public void SubscribeStatusChange(ConnectionStateChangeHandler listener)
        {
            client.SubscribeStatusChange(listener);
        }
        /// <summary>
        /// 订阅节点数据变更
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="listener">监听者</param>
        public void SubscribeDataChange(string path, NodeDataChangeHandler listener)
        {
            client.SubscribeDataChange(path, listener);
        }
        /// <summary>
        /// 订阅节点子节点变更
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="listener">监听者</param>
        /// <returns></returns>
        public Task<IEnumerable<string>> SubscribeChildrenChange(string path, NodeChildrenChangeHandler listener)
        {
            return client.SubscribeChildrenChange(path, listener);
        }

        /// <summary>
        /// 执行与释放或重置非托管资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 由终结器调用以释放资源。
        /// </summary>
        ~ZooKeeperClient()
        {
            Dispose(false);
        }
        /// <summary>
        /// 获取或设置一个值。该值指示资源已经被释放。
        /// </summary>
        private bool _disposed;
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
                // 清理托管资源
                if (client != null)
                {
                    client.Dispose();
                }
            }
            // 标记已经被释放。
            _disposed = true;
        }
    }
}
