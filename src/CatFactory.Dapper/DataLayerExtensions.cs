using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class DataLayerExtensions
    {
        public static DapperProject GenerateDataLayer(this DapperProject project)
        {
            GenerateAppSettings(project);
            GenerateDataRepositories(project);

            return project;
        }

        private static void GenerateAppSettings(DapperProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = project.GetAppSettingsClassDefinition(),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerDirectory());
        }

        private static void GenerateDataLayerContract(DapperProject project, CSharpInterfaceDefinition interfaceDefinition)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = interfaceDefinition,
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
        }

        private static void GenerateDataRepositories(DapperProject project)
        {
            GenerateRepositoryInterface(project);
            GenerateBaseRepositoryClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = new RepositoryClassDefinition(projectFeature);

                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = repositoryClassDefinition,
                    OutputDirectory = project.OutputDirectory
                };

                var interfaceDef = repositoryClassDefinition.RefactInterface();

                interfaceDef.Implements.Add("IRepository");

                interfaceDef.Namespace = project.GetDataLayerContractsNamespace();

                GenerateDataLayerContract(project, interfaceDef);

                codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
            }
        }

        private static void GenerateRepositoryInterface(DapperProject project)
        {
            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = new RepositoryInterfaceDefinition(project),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerContractsDirectory());
        }

        private static void GenerateBaseRepositoryClassDefinition(DapperProject project)
        {
            var codeBuilder = new CSharpClassBuilder
            {
                ObjectDefinition = new RepositoryBaseClassDefinition(project),
                OutputDirectory = project.OutputDirectory
            };

            codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
        }
    }
}
