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
    public class ZooKeeperClient
    {
        public IZookeeperClient client;
        public ZooKeeperClient(string connectionString, double SessionTimeout=20)
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
        public ZooKeeperClient ReRetryConnect()
        {
            client = new ZookeeperClient(client.Options);
            return this;
        }
        public void SubscribeStatusChange(ConnectionStateChangeHandler listener)
        {
            client.SubscribeStatusChange(listener);
        }
        public void SubscribeDataChange(string path, NodeDataChangeHandler listener)
        {
            client.SubscribeDataChange(path, listener);
        }
        public Task<IEnumerable<string>> SubscribeChildrenChange(string path, NodeChildrenChangeHandler listener)
        {
            return client.SubscribeChildrenChange(path, listener);
        }
    }
}
