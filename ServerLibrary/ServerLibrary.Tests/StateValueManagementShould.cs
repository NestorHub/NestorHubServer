using NestorHub.Common.Api;
using NestorHub.Common.Api.Enum;
using NestorHub.Server.Domain.Class;
using NestorHub.Server.Domain.Exceptions;
using NFluent;
using Xunit;

namespace NestorHub.ServerLibrary.Tests
{
    public class StateValueManagerShould
    {
        [Fact]
        public void return_0_state_value()
        {
            var stateValueManager = new StateValueManagement();
            Check.That(stateValueManager.GetAllStateValues().Count).IsEqualTo(0);
        }

        [Fact]
        public void return_true_when_state_value_exist()
        {
            var stateValueManager = new StateValueManagement();
            var stateValueToAdd = new StateValue("sentinel1", "package1", "statename", 2, TypeOfValue.Int);
            stateValueManager.AddStateValue(stateValueToAdd);
            Check.That(stateValueManager.Exist(stateValueManager.GetStateValueKey("sentinel1", "package1", "statename"))).IsEqualTo(true);
        }

        [Fact]
        public void return_false_when_state_value_doesnt_exist()
        {
            var stateValueManager = new StateValueManagement();
            var stateValueToAdd = new StateValue("sentinel1", "package1", "statename", 2, TypeOfValue.Int);
            stateValueManager.AddStateValue(stateValueToAdd);
            Check.That(stateValueManager.Exist(stateValueManager.GetStateValueKey("sentinel1", "package2", "statename"))).IsEqualTo(false);
        }

        [Fact]
        public void return_at_least_1_when_adding_a_new_state_value()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 2, TypeOfValue.Int));
            Check.That(stateValueManager.GetAllStateValues().Count).IsStrictlyGreaterThan(0);
        }

        [Fact]
        public void return_3_when_get_state_value()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            Check.That(stateValueManager.GetStateValueByKey("sentinel1", "package1", "statename").Value).IsEqualTo(3);
        }

        [Fact]
        public void throw_no_state_value_found_exception_when_get_state_value_doesnt_exist()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            Check.ThatCode(() => stateValueManager.GetStateValueByKey("sentinel2", "package1", "statename"))
                .Throws<NoStateValueFoundException>();
        }

        [Fact]
        public void return_0_when_adding_and_then_remove_state_value()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            stateValueManager.RemoveStateValueByKey("sentinel1", "package1", "statename");
            Check.That(stateValueManager.GetAllStateValues().Count).IsEqualTo(0);
        }

        [Fact]
        public void throw_no_state_value_found_exception_when_adding_and_then_remove_state_value_doesnt_exist()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            Check.ThatCode(() => stateValueManager.RemoveStateValueByKey("sentinel2", "package1", "statename"))
                .Throws<NoStateValueFoundException>();
        }

        [Fact]
        public void throw_exception_when_2_state_value_are_same_state_value_key()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            Check.ThatCode(() =>
                    stateValueManager.AddStateValue(
                        new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int)))
                .Throws<StateValueNotUniqueException>();
        }


        [Fact]
        public void throw_exception_when_trying_to_update_a_state_value_which_doesnt_exist()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            Check.ThatCode(() =>
                    stateValueManager.UpdateStateValue(
                        new StateValue("sentinel1", "package2", "statename", 3, TypeOfValue.Int)))
                .Throws<StateValueNotExistException>();
        }

        [Fact]
        public void return_5_when_a_state_value_is_update()
        {
            var stateValueManager = new StateValueManagement();
            stateValueManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            stateValueManager.UpdateStateValue(new StateValue("sentinel1", "package1", "statename", 5, TypeOfValue.Int));
            Check.That(stateValueManager.GetStateValueByKey("sentinel1", "package1", "statename").Value).IsEqualTo(5);
        }

        [Fact]
        public void show_sentinel1_dot_package1_dot_statename_when_get_state_value_key_to_string_override_method()
        {
            var stateValueManager = new StateValueManagement();
            var stateValueKey = stateValueManager.GetStateValueKey("sentinel1", "package1", "statename");
            Check.That(stateValueKey.ToString()).IsEqualTo("sentinel1.package1.statename");
        }
    }
}
