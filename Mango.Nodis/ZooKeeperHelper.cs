using log4net;
using Mango.ZooKeeperNet.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Nodis
{
    public class ZooKeeperHelper
    {
        public static ILog _log;
        public ZooKeeperClient _zk;
        public ZooKeeperHelper(ILog log, string connectionString, double SessionTimeout = 20)
        {
            _log = log;
            _zk = new ZooKeeperClient(connectionString, SessionTimeout);
            var codiswatcher = new CodisWatcher(log, _zk,"mango");
            codiswatcher.ProcessWatched();
        }
    }    
}
