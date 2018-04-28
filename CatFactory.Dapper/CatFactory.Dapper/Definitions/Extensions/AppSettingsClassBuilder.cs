using System.Collections.Generic;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class AppSettingsClassBuilder
    {
        public static AppSettingsClassDefinition GetAppSettingsClassDefinition(this DapperProject project)
        {
            return new AppSettingsClassDefinition
            {
                Namespace = project.GetDataLayerNamespace(),
                Namespaces = new List<string>
                {
                    "System"
                },
                Name = "AppSettings",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition("String", "ConnectionString")
                }
            };
        }
    }
}
