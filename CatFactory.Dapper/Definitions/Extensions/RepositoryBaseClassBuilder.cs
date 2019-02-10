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
                AccessModifier = AccessModifier.Public,
                Name = "Repository",
                Constructors =
                {
                    new ClassConstructorDefinition(AccessModifier.Public, new ParameterDefinition("IDbConnection", "connection"))
                    {
                        Lines =
                        {
                            new CodeLine("Connection = connection;")
                        }
                    }
                },
                Properties =
                {
                    new PropertyDefinition(AccessModifier.Protected, "IDbConnection", "Connection")
                    {
                        IsReadOnly = true
                    }
                }
            };
    }
}
