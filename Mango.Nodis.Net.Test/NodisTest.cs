using System;
using System.Linq;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mango.Nodis.Net.Test
{
    [TestClass]
    public class NodisTest
    {
        private static ILog log;
        public NodisTest()
        {
            if (log == null)
            {
                //初始化日志
                XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("Configs/log4net.config"));
                log = LogManager.GetLogger(typeof(NodisTest));
            }
            //Nodis.RedisPoolBuilder.Init("192.168.4.77:2181,192.168.4.78:2181,192.168.4.79:2181", "codis-mango", 2, 0);
        }
        /// <summary>
        /// 测试log配置
        /// </summary>
        [TestMethod]
        public void TestLog4Net()
        {
            var appenders = log.Logger.Repository.GetAppenders();
            Assert.AreEqual(4, appenders.Length);
        }

        /// <summary>
        /// 连接ZK，获取jodis下的proxy节点
        /// </summary>
        [TestMethod]
        public void GetZKJodis()
        {
            var zkhelper = new ZooKeeperHelper(log, "192.168.4.79:2181", "codis-mango");

            var result = zkhelper._zk.client.GetChildrenAsync("/jodis");
            result.Wait();
            var resData = result.Result.ToList();
            Assert.IsNotNull(resData.Exists(m=>m.Contains("codis-mango")));
        }
        /// <summary>
        /// 获取redis连接，实现redis读/写/删
        /// </summary>
        [TestMethod]
        public void GetRedisValue()
        {
            //初始化redis连接信息
            Nodis.RedisPoolBuilder.Init("192.168.4.79:2181", "codis-mango");
            //获取redis连接数据库
            string value = "null";
            using (var redisClient = RedisPoolBuilder.GetClient())
            {
                redisClient.Db = 5;
                redisClient.Set<string>("codisproxytest","zhh");
                value = redisClient.Get<string>("codisproxytest");
                redisClient.Remove("codisproxytest");
            }
            Assert.AreEqual("zhh", value);
        }
    }
}
