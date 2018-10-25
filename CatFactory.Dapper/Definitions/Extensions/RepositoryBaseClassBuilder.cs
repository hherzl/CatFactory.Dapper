using CatFactory.CodeFactory;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class RepositoryBaseClassBuilder
    {
        public static RepositoryBaseClassDefinition GetRepositoryBaseClassDefinition(this DapperProject project)
            => new RepositoryBaseClassDefinition
            {
                Namespaces =
                {
                    "System.Data",
                    project.GetEntityLayerNamespace()
                },
                Namespace = project.GetDataLayerContractsNamespace(),
                Name = "Repository",
                Constructors =
                {
                    new ClassConstructorDefinition(new ParameterDefinition("IDbConnection", "connection"))
                    {
                        Lines =
                        {
                            new CodeLine("Connection = connection;")
                        }
                    }
                },
                Properties =
                {
                    new PropertyDefinition("IDbConnection", "Connection")
                    {
                        AccessModifier = AccessModifier.Protected,
                        IsReadOnly = true
                    }
                }
            };
    }
}
