using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NestorHub.Sentinels.Domain.Class;
using NestorHub.Sentinels.Domain.Interfaces;

namespace NestorHub.Server.Controllers.PackagesHub
{
    [Route("packageshub/[controller]")]
    [ApiController]
    public class PackagesStoreController : ControllerBase
    {
        private readonly IPackagesStore _packagesStore;
        private readonly IPackagesInstances _packagesInstances;
        private readonly IPackageRunner _packageRunner;
        private readonly ILogger _logger;

        public PackagesStoreController(IPackagesStore packagesStore, IPackagesInstances packagesInstances, IPackageRunner packageRunner, ILogger<HomeControllerLogCategory> logger)
        {
            _packagesStore = packagesStore;
            _packagesInstances = packagesInstances;
            _packageRunner = packageRunner;
            _logger = logger;
        }

        [HttpGet]
        public List<PackageInfo> Get()
        {
            return _packagesStore.GetInstallPackages();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] FileUpload packageFile)
        {
            var filePath = await CreateTemporaryFileUploaded(packageFile);

            var name = FileNameWithoutExtension(packageFile);
            var packageInfo = _packagesStore.InstallPackage(name, filePath);
            if (packageInfo != null)
            {
                DeleteTemporaryFileUploaded(filePath);
                return Ok();
            }
            return NotFound();
        }

        [HttpDelete("{package}")]
        public async Task<IActionResult> Delete(string package)
        {
            if (await _packageRunner.StopPackage(package))
            {
                _packagesStore.MarkPackageToUninstall(package);
                var instances = _packagesInstances.GetInstancesToRunByPackageId(package);
                _packagesInstances.DeleteInstances(instances.ToArray());
                _logger.LogInformation($"PackageId {package} mark to uninstall on next restart of home controller server");
                return Ok(true);
            }
            return Ok(false);
        }

        private static async Task<string> CreateTemporaryFileUploaded(FileUpload packageFile)
        {
            var filePath = Path.GetTempFileName();

            if (packageFile.File.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await packageFile.File.CopyToAsync(stream);
                }
            }

            return filePath;
        }

        private static void DeleteTemporaryFileUploaded(string filePath)
        {
            System.IO.File.Delete(filePath);
        }

        private static string FileNameWithoutExtension(FileUpload packageFile)
        {
            var name = packageFile.Name;
            if (name.Contains('.'))
            {
                name = name.Substring(0, name.LastIndexOf('.'));
            }

            return name;
        }
    }

    public class FileUpload
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; }

        [FromForm(Name = "name")]
        public string Name { get; set; }
    }
}
