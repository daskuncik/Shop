using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.Events
{
    public class RequestEvent : Event
    {
        public string Host { get; set; }
        public string Origin { get; set; }
        public string Route { get; set; }
        public RequestType RequestType { get; set; }

        public override string ToString()
        {
            return $"Type: {RequestType}, Origin: {Origin}, Host: {Host}, Route: {Route}";
        }
    }

    public enum RequestType
    {
        Shop_new,
        Service
    }

}
