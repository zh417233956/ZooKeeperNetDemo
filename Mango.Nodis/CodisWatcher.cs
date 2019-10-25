using log4net;
using Mango.ZooKeeperNet.Util;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Nodis
{
    public class CodisWatcher : DefaultWatcher
    {
        public delegate void DeleteNodeDel(List<CodisProxyInfo> pools);
        public delegate void AddNodeDel(List<CodisProxyInfo> pools);
        /// <summary>
        /// 要监听的节点
        /// </summary>
        private string path;

        //TODO:正式使用
        //public string Path
        //{
        //    get { return $"/jodis/{path}"; }
        //    set { path = value; }
        //}

        public string Path
        {
            get { return $"/jodis.{path}"; }
            set { path = value; }
        }

        private List<CodisProxyInfo> pools;
        public CodisWatcher(ILog log, ZooKeeperClient zk, string path, AddNodeDel addNodeDel, DeleteNodeDel deleteNodeDel) : base(log, zk)
        {
            this.addNodeDel = addNodeDel;
            this.deleteNodeDel = deleteNodeDel;
            this.path = path;
        }
        public List<CodisProxyInfo> GetPools()
        {
            if (pools == null)
            {
                CodisProxyInfo itemCodisProxy = null;
                var allPools = new List<CodisProxyInfo>();
                var childPath = _zk.client.GetChildrenAsync(Path).Result;
                foreach (var itemPath in childPath)
                {
                    var iteData = _zk.client.GetDataAsync($"{Path}/{itemPath}").Result;
                    itemCodisProxy = JsonToObject<CodisProxyInfo>(Encoding.UTF8.GetString(iteData.ToArray()));
                    itemCodisProxy.Node = itemPath;
                    allPools.Add(itemCodisProxy);
                }
                pools = allPools;
            }
            return pools;
        }
        private DeleteNodeDel deleteNodeDel;
        private AddNodeDel addNodeDel;
        public override void ProcessWatched()
        {
            base.ProcessWatched();
            _zk.SubscribeStatusChange(async (ct, args) =>
            {
                if (args.State == Watcher.Event.KeeperState.SyncConnected)
                {
                    //检查是否存在根节点，不存在则写入
                    if (await _zk.client.ExistsAsync("/jodis") == false)
                    {
                        try
                        {
                            _ = _zk.client.CreateAsync("/jodis", null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                            _log.DebugFormat($"创建根节点:/jodis");
                        }
                        catch (Exception ex)
                        {
                            _log.ErrorFormat($"创建根节点-异常:{0}", ex.Message.ToString());
                        }
                    }
                    _ = this.GetPools();
                    _log.DebugFormat($"初始化节点开始");
                    _log.DebugFormat($"online上线节点:");
                    pools.FindAll(m => m.State == "online").Select(m => m.Addr).Distinct().ToList().ForEach(n =>
                    {
                        _log.DebugFormat($"{n}");
                    });
                    _log.DebugFormat($"初始化节点完成");

                    //_log.DebugFormat($"offline下线节点:");
                    //pools.FindAll(m => m.State == "offline").Select(m => m.Addr).Distinct().ToList().ForEach(n =>
                    //{
                    //    _log.DebugFormat($"{n}");
                    //});
                }
                //return CompletedTask;
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
                    var deletePools = pools.FindAll(m => m.Flag == 0);
                    if (deletePools.Count > 0)
                    {
                        var deletedPools= new List<CodisProxyInfo>();
                        deletePools.ForEach(n =>
                        {
                            //_log.DebugFormat($"删除节点:{n.Node}={n.Addr}-{n.State}");
                            deletedPools.Add(n);
                            pools.Remove(n);
                        });
                        if (deleteNodeDel != null)
                        {
                            deleteNodeDel(deletedPools);
                        }
                    }
                    var addPools = pools.FindAll(m => m.Flag == 1);
                    if (addPools.Count > 0)
                    {
                        if (addNodeDel != null)
                        {
                            addNodeDel(addPools);
                        }
                        //addPools.ForEach(n =>
                        //{
                        //    _log.DebugFormat($"新增节点:{n.Node}={n.Addr}-{n.State}");
                        //});
                    }
                    //pools.FindAll(m => m.Flag == 2).ForEach(n =>
                    //{
                    //    _log.DebugFormat($"编辑节点:{n.Node}={n.Addr}-{n.State}");
                    //});
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
