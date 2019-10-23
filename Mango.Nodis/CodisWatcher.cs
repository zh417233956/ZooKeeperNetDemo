using log4net;
using Mango.ZooKeeperNet.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mango.Nodis
{
    public class CodisWatcher : DefaultWatcher
    {
        /// <summary>
        /// 要监听的节点
        /// </summary>
        private string path;

        public string Path
        {
            get { return $"/jodis.{path}"; }
            set { path = value; }
        }

        private List<CodisProxyInfo> pools = new List<CodisProxyInfo>();
        public CodisWatcher(ILog log, ZooKeeperClient zk, string path) : base(log, zk)
        {
            this.path = path;
        }
        public override void ProcessWatched()
        {
            base.ProcessWatched();
            _zk.SubscribeStatusChange(async (ct, args) =>
            {
                CodisProxyInfo itemCodisProxy = null;
                if (args.State == org.apache.zookeeper.Watcher.Event.KeeperState.SyncConnected)
                {
                    IEnumerable<string> childPath = await _zk.client.GetChildrenAsync(Path);
                    _log.DebugFormat($"初始化节点开始");
                    foreach (var itemPath in childPath)
                    {
                        var iteData = await _zk.client.GetDataAsync($"{Path}/{itemPath}");
                        itemCodisProxy = JsonToObject<CodisProxyInfo>(Encoding.UTF8.GetString(iteData.ToArray()));
                        itemCodisProxy.Node = itemPath;
                        pools.Add(itemCodisProxy);
                        _log.DebugFormat($"加载节点:{itemCodisProxy.Node}={itemCodisProxy.Addr}-{itemCodisProxy.State}");
                    }
                    _log.DebugFormat($"初始化节点完成");
                    _log.DebugFormat($"online上线节点:");
                    pools.FindAll(m => m.State == "online").Select(m => m.Addr).Distinct().ToList().ForEach(n =>
                    {
                        _log.DebugFormat($"{n}");
                    });
                    _log.DebugFormat($"offline下线节点:");
                    pools.FindAll(m => m.State == "offline").Select(m => m.Addr).Distinct().ToList().ForEach(n =>
                    {
                        _log.DebugFormat($"{n}");
                    });
                }
            });
            _zk.SubscribeChildrenChange(Path, (ct, args) =>
            {
                CodisProxyInfo poolCodisProxy = null;
                CodisProxyInfo itemCodisProxy = null;
                IEnumerable<string> childrenList = _zk.client.GetChildrenAsync(Path).Result;
                lock (pools)
                {
                    //设置为初始状态
                    pools.ForEach(m => m.Flag = 0);
                    foreach (var itemPath in childrenList)
                    {
                        var iteData = _zk.client.GetDataAsync($"{Path}/{itemPath}").Result;
                        itemCodisProxy = JsonToObject<CodisProxyInfo>(Encoding.UTF8.GetString(iteData.ToArray()));
                        itemCodisProxy.Node = itemPath;
                        poolCodisProxy = pools.FirstOrDefault(m => m.Node == itemCodisProxy.Node);
                        if (poolCodisProxy == null)
                        {
                            itemCodisProxy.Flag = 1;
                            pools.Add(itemCodisProxy);
                        }
                        else if (itemCodisProxy.Addr.Equals(poolCodisProxy.Addr) && itemCodisProxy.State.Equals(poolCodisProxy.State))
                        {
                            poolCodisProxy.Flag = 3;
                        }
                        else
                        {
                            poolCodisProxy.Flag = 2;
                            poolCodisProxy.Addr = itemCodisProxy.Addr;
                            poolCodisProxy.State = itemCodisProxy.State;
                        }
                    }

                    pools.FindAll(m => m.Flag == 0).ForEach(n =>
                    {
                        _log.DebugFormat($"删除节点:{n.Node}={n.Addr}-{n.State}");
                        pools.Remove(n);
                    });
                    pools.FindAll(m => m.Flag == 1).ForEach(n =>
                    {
                        _log.DebugFormat($"新增节点:{n.Node}={n.Addr}-{n.State}");
                    });
                    pools.FindAll(m => m.Flag == 2).ForEach(n =>
                    {
                        _log.DebugFormat($"编辑节点:{n.Node}={n.Addr}-{n.State}");
                    });
                }
                return CompletedTask;
            });
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strJson"></param>
        /// <returns></returns>
        T JsonToObject<T>(string strJson)
        {
#if NETFRAMEWORK
            System.Web.Script.Serialization.JavaScriptSerializer jsonSerialize = new System.Web.Script.Serialization.JavaScriptSerializer();
            return jsonSerialize.Deserialize<T>(strJson);
#else
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(strJson);
#endif

        }
    }
}
