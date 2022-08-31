using System;
using System.Net;
using System.Net.NetworkInformation;

namespace AsyncThreadsComparison.Dto
{
    public class ExecutionDetailsDto : IComparable
    {
        public int ManagedThreadId { get; set; }

        public IPAddress IPAddress { get; set; }

        public IPStatus IPStatus { get; set; }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            //TODO: why here may be null, we check obj null before
            IPAddress ipAddress = obj as IPAddress;

            return ipAddress != null ? this.IPAddress.Address.CompareTo(ipAddress.Address) : 0;
        }
    }
}
