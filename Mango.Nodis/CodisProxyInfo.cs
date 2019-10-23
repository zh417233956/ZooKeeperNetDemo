using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Nodis
{
    public class CodisProxyInfo
    {
        public string Addr { get; set; }
        public string State { get; set; }
        /// <summary>
        /// 编辑状态，0初始状态，1新增，2编辑,3无变化
        /// </summary>
        public int Flag { get; set; }

        public string Node { get; set; }
    }
}
