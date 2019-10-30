using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.ZooKeeperNet.Util
{
    public enum AuthEnum
    {
        /// <summary>
        /// 无需授权
        /// </summary>
        world = 0,
        auth = 1,
        digest = 2,
        ip = 3,
        super = 4
    }
}
