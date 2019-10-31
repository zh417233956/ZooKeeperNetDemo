﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.ZooKeeperNet.Util
{
    /// <summary>
    /// ZK连接授权方式
    /// </summary>
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
