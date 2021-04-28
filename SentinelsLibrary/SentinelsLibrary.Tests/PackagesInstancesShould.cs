using System;
using System.Collections.Generic;
using System.Linq;
using NestorHub.Sentinels.Domain.Class;
using NestorHub.Sentinels.Domain.Interfaces;
using NFluent;
using NSubstitute;
using Xunit;

namespace NestorHub.SentinelsLibrary.Tests
{
    public class PackagesInstancesShould
    {
        private readonly IPackageToRunStorage _packageToRunStorage;

        public PackagesInstancesShould()
        {
            _packageToRunStorage = Substitute.For<IPackageToRunStorage>();
            _packageToRunStorage.LoadFromPersistence().Returns(new List<PackageToRun>());
        }

        [Fact]
        public void return_empty_list_when_no_package_added()
        {
            var packageInstances = new PackagesInstances(_packageToRunStorage);
            Check.That(packageInstances.GetAllInstancesToRun().Count()).IsEqualTo(0);
        }

        [Fact]
        public void return_one_when_add_one_package_instance()
        {
            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(new PackageToRun("PackageId", "InstanceOne", false, Guid.NewGuid(), 0, null));
            Check.That(packageInstances.GetAllInstancesToRun().Count()).IsEqualTo(1);
            Check.That(packageInstances.GetAllInstancesToRun().First().RunOrder).IsEqualTo(1);
        }

        [Fact]
        public void return_one_when_add_one_package_instance_and_then_updated()
        {
            var instanceId = Guid.NewGuid();
            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(new PackageToRun("PackageId", "InstanceOne", false, instanceId, 0, null));
            packageInstances.AddInstanceToRun(new PackageToRun("PackageId", "InstanceOne", true, instanceId, 0, null));
            Check.That(packageInstances.GetAllInstancesToRun().Count()).IsEqualTo(1);
        }

        [Fact]
        public void return_one_when_add_two_package_instance_and_then_delete_one()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 0, null));
            packageInstances.AddInstanceToRun(new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 0, null));

            packageInstances.DeleteInstance(instanceTwoId);

            Check.That(packageInstances.GetAllInstancesToRun().Count()).IsEqualTo(1);
        }

        [Fact]
        public void return_InstanceTwo_when_search_on_instance_id_two()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 0, null));
            packageInstances.AddInstanceToRun(new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 0, null));

            var instancePackageTwo = packageInstances.GetInstanceToRunById(instanceTwoId);

            Check.That(instancePackageTwo.InstanceName).IsEqualTo("InstanceTwo");
        }

        [Fact]
        public void return_empty_list_when_add_two_package_instance_and_then_delete_all()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();

            var instanceOne = new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 0, null);
            var instanceTwo = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 0, null);

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(instanceOne);
            packageInstances.AddInstanceToRun(instanceTwo);

            packageInstances.DeleteInstances(new List<PackageToRun>() {instanceOne, instanceTwo});

            Check.That(packageInstances.GetAllInstancesToRun().Count()).IsEqualTo(0);
        }

        [Fact]
        public void return_two_when_search_all_instances_of_one_package()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();
            var instanceThreeId = Guid.NewGuid();

            var instanceOne = new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 0, null);
            var instanceTwo = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 0, null);
            var instanceThree = new PackageToRun("PackageTwoId", "InstanceThree", true, instanceThreeId, 0, null);

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(instanceOne);
            packageInstances.AddInstanceToRun(instanceTwo);
            packageInstances.AddInstanceToRun(instanceThree);

            Check.That(packageInstances.GetInstancesToRunByPackageId("PackageId").Count()).IsEqualTo(2);
        }

        [Fact]
        public void return_3_when_reorder_fourth_package()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();
            var instanceThreeId = Guid.NewGuid();
            var instanceFourId = Guid.NewGuid();
            var instanceFiveId = Guid.NewGuid();

            var instanceOne = new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 1, null);
            var instanceTwo = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 2, null);
            var instanceThree = new PackageToRun("PackageTwoId", "InstanceThree", true, instanceThreeId, 3, null);
            var instanceFour = new PackageToRun("PackageTwoId", "InstanceFour", true, instanceFourId, 4, null);
            var instanceFive= new PackageToRun("PackageTwoId", "InstanceFive", true, instanceFiveId, 5, null);

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(instanceOne);
            packageInstances.AddInstanceToRun(instanceTwo);
            packageInstances.AddInstanceToRun(instanceThree);
            packageInstances.AddInstanceToRun(instanceFour);
            packageInstances.AddInstanceToRun(instanceFive);

            var instanceFourNew = new PackageToRun("PackageTwoId", "InstanceFour", true, instanceFourId, 2, null);

            packageInstances.AddInstanceToRun(instanceFourNew);

            var packages = packageInstances.GetAllInstancesToRun().ToList().OrderBy(p => p.RunOrder).ToList();
            Check.That(packages[2].RunOrder).IsEqualTo(3);
            Check.That(packages[2].InstanceName).IsEqualTo("InstanceTwo");
            Check.That(packages[3].InstanceName).IsEqualTo("InstanceThree");
            Check.That(packages[3].RunOrder).IsEqualTo(4);
            Check.That(packages[4].RunOrder).IsEqualTo(5);
        }

        [Fact]
        public void return_3_when_reorder_two_package()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();
            var instanceThreeId = Guid.NewGuid();
            var instanceFourId = Guid.NewGuid();
            var instanceFiveId = Guid.NewGuid();

            var instanceOne = new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 1, null);
            var instanceTwo = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 2, null);
            var instanceThree = new PackageToRun("PackageTwoId", "InstanceThree", true, instanceThreeId, 3, null);
            var instanceFour = new PackageToRun("PackageTwoId", "InstanceFour", true, instanceFourId, 4, null);
            var instanceFive = new PackageToRun("PackageTwoId", "InstanceFive", true, instanceFiveId, 5, null);

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(instanceOne);
            packageInstances.AddInstanceToRun(instanceTwo);
            packageInstances.AddInstanceToRun(instanceThree);
            packageInstances.AddInstanceToRun(instanceFour);
            packageInstances.AddInstanceToRun(instanceFive);

            var instanceTwoNew = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 4, null);

            packageInstances.AddInstanceToRun(instanceTwoNew);

            var packages = packageInstances.GetAllInstancesToRun().ToList().OrderBy(p => p.RunOrder).ToList();
            Check.That(packages[2].RunOrder).IsEqualTo(3);
            Check.That(packages[2].InstanceName).IsEqualTo("InstanceFour");
            Check.That(packages[3].InstanceName).IsEqualTo("InstanceTwo");
            Check.That(packages[3].RunOrder).IsEqualTo(4);
            Check.That(packages[4].RunOrder).IsEqualTo(5);
        }

        [Fact]
        public void return_1_when_reorder_last_package()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();
            var instanceThreeId = Guid.NewGuid();
            var instanceFourId = Guid.NewGuid();
            var instanceFiveId = Guid.NewGuid();

            var instanceOne = new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 1, null);
            var instanceTwo = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 2, null);
            var instanceThree = new PackageToRun("PackageTwoId", "InstanceThree", true, instanceThreeId, 3, null);
            var instanceFour = new PackageToRun("PackageTwoId", "InstanceFour", true, instanceFourId, 4, null);
            var instanceFive = new PackageToRun("PackageTwoId", "InstanceFive", true, instanceFiveId, 5, null);

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(instanceOne);
            packageInstances.AddInstanceToRun(instanceTwo);
            packageInstances.AddInstanceToRun(instanceThree);
            packageInstances.AddInstanceToRun(instanceFour);
            packageInstances.AddInstanceToRun(instanceFive);

            var instanceFiveNew = new PackageToRun("PackageTwoId", "InstanceFive", true, instanceFiveId, 1, null);

            packageInstances.AddInstanceToRun(instanceFiveNew);

            var packages = packageInstances.GetAllInstancesToRun().ToList().OrderBy(p => p.RunOrder).ToList();
            Check.That(packages[2].RunOrder).IsEqualTo(3);
            Check.That(packages[2].InstanceName).IsEqualTo("InstanceTwo");
            Check.That(packages[3].InstanceName).IsEqualTo("InstanceThree");
            Check.That(packages[3].RunOrder).IsEqualTo(4);
            Check.That(packages[4].RunOrder).IsEqualTo(5);
        }

        [Fact]
        public void return_1_when_reorder_packages_and_some_packages_have_0_in_run_order_property()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();
            var instanceThreeId = Guid.NewGuid();
            var instanceFourId = Guid.NewGuid();
            var instanceFiveId = Guid.NewGuid();

            var instanceOne = new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 1, null);
            var instanceTwo = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 0, null);
            var instanceThree = new PackageToRun("PackageTwoId", "InstanceThree", true, instanceThreeId, 0, null);
            var instanceFour = new PackageToRun("PackageTwoId", "InstanceFour", true, instanceFourId, 0, null);

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(instanceOne);
            packageInstances.AddInstanceToRun(instanceTwo);
            packageInstances.AddInstanceToRun(instanceThree);
            packageInstances.AddInstanceToRun(instanceFour);

            var packages = packageInstances.GetAllInstancesToRun().ToList().OrderBy(p => p.RunOrder).ToList();
            Check.That(packages[2].RunOrder).IsEqualTo(3);
            Check.That(packages[2].InstanceName).IsEqualTo("InstanceThree");
            Check.That(packages[3].InstanceName).IsEqualTo("InstanceFour");
            Check.That(packages[3].RunOrder).IsEqualTo(4);
        }

        [Fact]
        public void return_1_on_next_run_order_when_store_is_empty()
        {

            var packageInstances = new PackagesInstances(_packageToRunStorage);

            Check.That(packageInstances.GetNextRunOrderIndex()).IsEqualTo(1);
        }

        [Fact]
        public void return_3_on_next_run_order_when_store_contains_2_instances()
        {
            var instanceOneId = Guid.NewGuid();
            var instanceTwoId = Guid.NewGuid();

            var instanceOne = new PackageToRun("PackageId", "InstanceOne", false, instanceOneId, 1, null);
            var instanceTwo = new PackageToRun("PackageId", "InstanceTwo", true, instanceTwoId, 2, null);

            var packageInstances = new PackagesInstances(_packageToRunStorage);
            packageInstances.AddInstanceToRun(instanceOne);
            packageInstances.AddInstanceToRun(instanceTwo);

            Check.That(packageInstances.GetNextRunOrderIndex()).IsEqualTo(3);
        }
    }
}
