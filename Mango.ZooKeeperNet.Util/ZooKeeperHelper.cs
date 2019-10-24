using log4net;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Mango.ZooKeeperNet.Util
{
    public class ZooKeeperHelper
    {
        private readonly ILog _log;
        private List<string> _address;
        private int _sessiontTimeout = 10 * 1000;//10秒
        private ZooKeeper _zooKeeper;
        private int _connectTimeout = 3 * 30 * 1000;//每个zookeeper实例尝试连接最长30秒
        private string _success = "success";
        private string _fail = "fail";


        public ZooKeeperHelper(ILog log, List<string> address, int sessionTimeOut = 10 * 1000)
        {
            _log = log;
            _address = address;
            _sessiontTimeout = sessionTimeOut;
        }


        /// <summary>
        /// 返回null表示连接不成功
        /// </summary>
        /// <param name="authEnum"></param>
        /// <param name="authInfo"></param>
        /// <returns></returns>
        public ZooKeeper Connect(AuthEnum authEnum, string authInfo)
        {
            try
            {
                foreach (string address in _address)
                {
                    _zooKeeper = new ZooKeeper(address, _sessiontTimeout, new NodeWatcher(_log));
                    if (authEnum != AuthEnum.world)
                    {
                        _zooKeeper.addAuthInfo(authEnum.ToString(), System.Text.Encoding.UTF8.GetBytes(authInfo));
                    }
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (stopwatch.ElapsedMilliseconds < _connectTimeout / _address.Count)
                    {
                        ZooKeeper.States states = _zooKeeper.getState();
                        if (states == ZooKeeper.States.CONNECTED || states == ZooKeeper.States.CONNECTEDREADONLY)
                        {
                            break;
                        }
                    }
                    stopwatch.Stop();
                    if (_zooKeeper.getState().ToString().ToUpper().Contains("CONNECTED"))
                    {
                        break;
                    }
                }

                return _zooKeeper;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("连接zookeeper发生异常：{0}", ex.Message + ex.StackTrace);
            }
            return null;
        }


        /// <summary>
        /// 创建节点,不能在临时节点下创建子节点
        /// </summary>
        /// <param name="path">不要使用path等关键字作为路径</param>
        /// <param name="data"></param>
        /// <param name="persistent"></param>
        /// <returns></returns>
        public string CreateNode(string path, string data, bool persistent = false)
        {
            try
            {
                Task<string> task = _zooKeeper.createAsync(path, System.Text.Encoding.UTF8.GetBytes(data), ZooDefs.Ids.OPEN_ACL_UNSAFE, persistent ? CreateMode.PERSISTENT : CreateMode.EPHEMERAL);
                task.Wait();
                if (!string.IsNullOrEmpty(task.Result) && task.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return task.Result;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("创建节点发生异常：{0}({1}),{2}", path, data, ex.Message + ex.StackTrace);
            }
            return _fail;
        }


        /// <summary>
        /// 删除节点,删除节点的子节点个数必须为0，否则请先删除子节点
        /// </summary>
        /// <param name="path">不要使用path等关键字作为路径</param>
        /// <returns></returns>
        public string DeleteNode(string path)
        {
            try
            {
                Task task = _zooKeeper.deleteAsync(path);
                task.Wait();
                if (task.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return _success;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("删除节点发生异常：{0},{1}", path, ex.Message + ex.StackTrace);
            }
            return _fail;
        }


        /// <summary>
        /// 给节点设置数据
        /// </summary>
        /// <param name="path">不要使用path等关键字作为路径</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SetData(string path, string data)
        {
            try
            {
                Task<org.apache.zookeeper.data.Stat> stat = _zooKeeper.setDataAsync(path, System.Text.Encoding.UTF8.GetBytes(data));
                stat.Wait();
                if (stat.Result != null && stat.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return _success;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("设置节点数据发生异常：{0}({1}),{2}", path, data, ex.Message + ex.StackTrace);
            }
            return _fail;
        }


        /// <summary>
        /// 判断节点是否存在
        /// </summary>
        /// <param name="path">不要使用path等关键字作为路径</param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public string ExistsNode(string path, Watcher watcher = null)
        {
            try
            {
                Task<org.apache.zookeeper.data.Stat> stat = _zooKeeper.existsAsync(path, watcher);
                stat.Wait();
                if (stat.Result != null && stat.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return _success;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("判定节点存在与否发生异常：{0},{1}", path, ex.Message + ex.StackTrace);
            }
            return _fail;
        }


        /// <summary>
        /// 得到节点相关信息
        /// </summary>
        /// <param name="path">不要使用path等关键字作为路径</param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public Stat GetNode(string path, Watcher watcher = null)
        {
            try
            {
                Task<org.apache.zookeeper.data.Stat> stat = _zooKeeper.existsAsync(path, watcher);
                stat.Wait();
                if (stat.Result != null && stat.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return stat.Result;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("得到节点信息发生异常：{0},{1}", path, ex.Message + ex.StackTrace);
            }
            return null;
        }


        /// <summary>
        /// 得到节点数据
        /// </summary>
        /// <param name="path">不要使用path等关键字作为路径</param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public string GetData(string path, Watcher watcher = null)
        {
            try
            {
                Task<DataResult> dataResult = _zooKeeper.getDataAsync(path, watcher);
                dataResult.Wait();
                if (dataResult.Result != null && dataResult.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return Encoding.UTF8.GetString(dataResult.Result.Data);
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("得到节点数据发生异常：{0},{1}", path, ex.Message + ex.StackTrace);
            }
            return _fail;
        }


        /// <summary>
        /// 得到后代节点路径
        /// </summary>
        /// <param name="path">不要使用path等关键字作为路径</param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public List<string> GetChildren(string path, Watcher watcher = null)
        {
            try
            {
                Task<ChildrenResult> childrenResult = _zooKeeper.getChildrenAsync(path, watcher);
                childrenResult.Wait();
                if (childrenResult.Result != null && childrenResult.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return childrenResult.Result.Children;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("得到后代节点发生异常：{0},{1}", path, ex.Message + ex.StackTrace);
            }
            return null;
        }


        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        public string Close()
        {
            try
            {
                Task task = _zooKeeper.closeAsync();
                task.Wait();
                if (task.Status.ToString().ToLower() == "RanToCompletion".ToLower())
                {
                    return _success;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("关闭zookeeper发生异常：{0}", ex.Message + ex.StackTrace);
            }
            return _fail;
        }


        /// <summary>
        /// 得到连接状态
        /// </summary>
        /// <returns></returns>
        public string GetState()
        {
            try
            {
                if (_zooKeeper != null)
                {
                    ZooKeeper.States states = _zooKeeper.getState();
                    return states.ToString();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("获取zookeeper连接状态发生异常：{0}", ex.Message + ex.StackTrace);
            }
            return _fail;
        }


        /// <summary>
        /// 是否已经连接
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
            try
            {
                if (_zooKeeper != null)
                {
                    ZooKeeper.States states = _zooKeeper.getState();
                    if (states == ZooKeeper.States.CONNECTED || states == ZooKeeper.States.CONNECTEDREADONLY)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("获取zookeeper连接状态发生异常：{0}", ex.Message + ex.StackTrace);
            }
            return false;
        }
    }
}
