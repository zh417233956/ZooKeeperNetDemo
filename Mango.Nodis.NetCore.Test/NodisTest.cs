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
            //����log4net��־�����ļ�
            if (log == null)
            {
                XmlConfigurator.Configure(LogManager.CreateRepository("log4net-default-repository"), new System.IO.FileInfo("Configs/log4net.config"));
                log = LogManager.GetLogger(typeof(NodisTest));
            }
        }
        /// <summary>
        /// ����log����
        /// </summary>
        [Fact]
        public void TestLog4Net()
        {
            var appenders = log.Logger.Repository.GetAppenders();
            Assert.Equal(4, appenders.Length);
        }
        /// <summary>
        /// ����ZK����ȡjodis�µ�proxy�ڵ�
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
        /// ��ȡredis���ӣ�ʵ��redis��/д/ɾ
        /// </summary>
        [Fact]
        public void GetRedisValue()
        {
            //��ʼ��redis������Ϣ
            Nodis.RedisPoolBuilder.Init("192.168.4.79:2181", "codis-mango");
            //��ȡredis�������ݿ�
            var redisClient = Nodis.RedisPoolBuilder.GetDatabase(5);
            redisClient.StringSet("codisproxytest","zhh");
            var value = redisClient.StringGet("codisproxytest");
            redisClient.KeyDelete("codisproxytest");
            Assert.Equal("zhh", value);
        }
    }
}
