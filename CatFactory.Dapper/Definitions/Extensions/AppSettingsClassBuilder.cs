using System.Collections.Generic;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class AppSettingsClassBuilder
    {
        public static AppSettingsClassDefinition GetAppSettingsClassDefinition(this DapperProject project)
            => new AppSettingsClassDefinition
            {
                Namespace = project.GetDataLayerNamespace(),
                Name = "AppSettings",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition("string", "ConnectionString")
                }
            };
    }
}
