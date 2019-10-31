using log4net;
using log4net.Config;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace Mango.Nodis.NetCore.Test
{
    public class NodisTest
    {
        private static ILog log;
        public NodisTest()
        {
            //加载log4net日志配置文件
            if (log == null)
            {
                XmlConfigurator.Configure(LogManager.CreateRepository("log4net-default-repository"), new System.IO.FileInfo("Configs/log4net.config"));
                log = LogManager.GetLogger(typeof(NodisTest));
            }
        }
        /// <summary>
        /// 测试log配置
        /// </summary>
        [Fact]
        public void TestLog4Net()
        {
            var appenders = log.Logger.Repository.GetAppenders();
            Assert.Equal(4, appenders.Length);
        }
        /// <summary>
        /// 连接ZK，获取jodis下的proxy节点
        /// </summary>
        [Fact]
        public void GetZKJodis()
        {
            var zkhelper = new ZooKeeperHelper(log, "192.168.4.79:2181", "codis-mango");

            var result = zkhelper._zk.client.GetChildrenAsync("/jodis");
            result.Wait();
            var resData = result.Result.ToList();
            Assert.Contains("codis-mango", resData);
        }
        /// <summary>
        /// 获取redis连接，实现redis读/写/删
        /// </summary>
        [Fact]
        public void GetRedisValue()
        {
            //初始化redis连接信息
            Nodis.RedisPoolBuilder.Init("192.168.4.79:2181", "codis-mango");
            //获取redis连接数据库
            var redisClient = Nodis.RedisPoolBuilder.GetDatabase(5);
            redisClient.StringSet("codisproxytest","zhh");
            var value = redisClient.StringGet("codisproxytest");
            redisClient.KeyDelete("codisproxytest");
            Assert.Equal("zhh", value);
        }
    }
}
