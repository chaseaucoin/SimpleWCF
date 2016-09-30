using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWCF._45.Test
{
    /// <summary>
    /// A simple implementation of the ping service contract
    /// </summary>
    /// <seealso cref="SimpleWCF._45.Test.IPingService" />
    public class PingService : IPingService
    {
        /// <summary>
        /// Pings this instance.
        /// </summary>
        /// <returns></returns>
        public string Ping()
        {
            return "pong";
        }
    }
}
