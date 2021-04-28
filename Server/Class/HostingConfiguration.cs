using System.IO;
using Microsoft.Extensions.Configuration;

namespace NestorHub.Server.Class
{
    public class HostingConfiguration : IHostingConfiguration
    {
        private string _url;
        private int _port;
        private bool _useSsl;

        public HostingConfiguration()
        {
            LoadConfiguration();
        }

        public string GetUrlToUse()
        {
            var protocol = _useSsl ? "https" : "http";
            return $"{protocol}://{_url}:{_port}";
        }

        public string GetAddressServer()
        {
            return _url;
        }

        public int GetPortServer()
        {
            return _port;
        }

        public bool GetUseSsl()
        {
            return _useSsl;
        }

        private void LoadConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            _url = config.GetValue<string>("Url");
            _port = config.GetValue<int>("Port");
            _useSsl = config.GetValue<bool>("UseSsl");
        }
    }
}
