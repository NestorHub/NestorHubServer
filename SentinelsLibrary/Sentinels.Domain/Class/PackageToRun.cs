using System;
using Newtonsoft.Json;

namespace NestorHub.Sentinels.Domain.Class
{
    public class PackageToRun
    {
        public Guid InstanceId { get; set; }
        public string PackageId { get; set; }
        public string InstanceName { get; set; }
        public bool RunOnServerStart { get; set; }
        public int RunOrder { get; set; }
        public object Parameter { get; set; }

        [JsonConstructor]
        public PackageToRun(string packageId, string instance, bool runOnServerStart, Guid instanceId, int runOrder, object parameter)
        {
            InstanceId = instanceId == Guid.Empty ? Guid.NewGuid() : instanceId;

            PackageId = packageId;
            InstanceName = instance;
            RunOnServerStart = runOnServerStart;
            RunOrder = runOrder;
            Parameter = parameter;
        }
    }
}