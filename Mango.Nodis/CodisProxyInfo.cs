using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Nodis
{
    /// <summary>
    /// ZK中Codis Proxy的节点信息
    /// </summary>
    public class CodisProxyInfo
    {
        /// <summary>
        /// Proxy地址，ip+port
        /// </summary>
        public string Addr { get; set; }
        /// <summary>
        /// 状态，online/offline
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 编辑状态，0初始状态，1新增，2编辑,3无变化
        /// </summary>
        public int Flag { get; set; }
        /// <summary>
        /// Node节点的名称
        /// </summary>
        public string Node { get; set; }
    }
}
