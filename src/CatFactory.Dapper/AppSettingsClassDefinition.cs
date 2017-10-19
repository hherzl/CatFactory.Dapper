using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.Dapper
{
    public static class AppSettingsClassDefinition
    {
        public static CSharpClassDefinition GetAppSettingsClassDefinition(this DapperProject project)
        {
            return new CSharpClassDefinition
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
