namespace NestorHub.Sentinels.Domain.Class
{
    public class PackageToRunWithStatus
    {
        public PackageToRun Instance { get; set; }
        public bool Running { get; set; }

        public PackageToRunWithStatus(PackageToRun instance, bool running)
        {
            Instance = instance;
            Running = running;
        }
    }
}