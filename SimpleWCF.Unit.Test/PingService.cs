using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWCF.Unit.Test
{
    /// <summary>
    /// A simple implementation of the ping service contract
    /// </summary>
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

        public string ThrowAnException()
        {
            throw new ArgumentException("I didn't work");
        }
    }
}
