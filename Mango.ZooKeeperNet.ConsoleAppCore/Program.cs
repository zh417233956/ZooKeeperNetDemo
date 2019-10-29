using log4net;
using log4net.Config;
using System;
using System.IO;

namespace Mango.ZooKeeperNet.ConsoleAppCore
{
    class Program
    {
        private static bool logState;

        private static ILog log;

        static void Main(string[] args)
        {
            //加载log4net日志配置文件
            XmlConfigurator.Configure(LogManager.CreateRepository("log4net-default-repository"), new System.IO.FileInfo("Configs/log4net.config"));
            //初始化redis连接信息
            Nodis.RedisPoolBuilder.Init("192.168.4.77:2181,192.168.4.78:2181,192.168.4.79:2181", "codis-mango", 5);
            try
            {
                log = LogManager.GetLogger(typeof(Program));
                var redisClient = Nodis.RedisPoolBuilder.GetDatabase(5);
                var value = redisClient.StringGet("codis-proxy");
                log.DebugFormat("查询redis:{0}={1}", "codis-proxy", value);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("异常信息:{0}", ex.ToString());
            }
            Console.ReadKey();

            Console.ReadLine();
        }
    }
}
