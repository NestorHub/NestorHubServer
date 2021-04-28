using System.Collections.Generic;
using NestorHub.Common.Api;
using Newtonsoft.Json.Linq;

namespace NestorHub.Sentinels.Domain.Class
{
    public class PackageInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public List<StateValueDefinition> StateValues { get; set; }
        public object Parameter { get; set; }
        public object ParameterDescription { get; set; }

        public List<string> ParametersDescriptionInList
        {
            get
            {
                var descriptions = new List<string>();
                if (ParameterDescription != null)
                {
                    descriptions.AddRange(GetPropertiesForObject(descriptions));
                }
                return descriptions;
            }
        }

        private List<string> GetPropertiesForObject(List<string> descriptions)
        {
            var properties = new List<string>();
            var propertiesOfObject = ((JObject) ParameterDescription).PropertyValues();
            foreach (var property in propertiesOfObject)
            {
                properties.Add($"{property.Path} : {property.Value<string>()}");
            }
            return properties;
        }
    }
}