using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class RepositoryInterfaceBuilder
    {
        public static RepositoryInterfaceDefinition GetRepositoryInterfaceDefinition(this DapperProject project)
            => new RepositoryInterfaceDefinition
            {
                Namespace = project.GetDataLayerContractsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = "IRepository"
            };
    }
}
