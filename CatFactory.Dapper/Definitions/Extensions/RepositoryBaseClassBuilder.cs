using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class RepositoryBaseClassBuilder
    {
        public static RepositoryBaseClassDefinition GetRepositoryBaseClassDefinition(this DapperProject project)
            => new RepositoryBaseClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Linq",
                    "System.Threading.Tasks",
                    "Microsoft.Extensions.Options",
                    project.GetEntityLayerNamespace()
                },
                Namespace = project.GetDataLayerContractsNamespace(),
                Name = "Repository",
                Constructors =
                {
                    new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
                    {
                        Lines = new List<ILine>
                        {
                            new CodeLine("ConnectionString = appSettings.Value.ConnectionString;")
                        }
                    }
                },
                Properties =
                {
                    new PropertyDefinition("String", "ConnectionString ")
                    {
                        AccessModifier = AccessModifier.Protected,
                        IsReadOnly = true
                    }
                }
            };
    }
}
