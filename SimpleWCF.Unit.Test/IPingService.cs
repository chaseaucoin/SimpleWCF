using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWCF.Unit.Test
{
    /// <summary>
    /// Ping service contract
    /// </summary>
    [ServiceContract]
    public interface IPingService
    {
        /// <summary>
        /// Pings this instance.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string Ping();

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string ThrowAnException();
    }
}
