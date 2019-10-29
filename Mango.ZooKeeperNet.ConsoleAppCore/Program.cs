using log4net;
using log4net.Config;
using System;
using System.IO;

namespace Mango.ZooKeeperNet.ConsoleAppCore
{
    class Program
    {
        private static bool logState;

        private static ILog _log;

        public static ILog log 
        {
            get {
                if (_log==null)
                {
                    var logRepository = log4net.LogManager.CreateRepository("NETCoreRepository");
                    XmlConfigurator.Configure(logRepository, new System.IO.FileInfo("Configs/log4net.config"));
                    _log = LogManager.GetLogger(logRepository.Name, typeof(Program));
                }
                return _log;
            }
        }

        static void Main(string[] args)
        {
            // 加载log4net日志配置文件

            log.DebugFormat("日志打印测试");
            log.ErrorFormat("日志打印测试");

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
