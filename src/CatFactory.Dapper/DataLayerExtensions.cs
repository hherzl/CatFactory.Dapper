using System.Collections.Generic;
using System.IO;
using CatFactory.Collections;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class DataLayerExtensions
    {
        public static DapperProject GenerateDataLayer(this DapperProject project)
        {
            GenerateAppSettings(project);
            GenerateDataRepositories(project);
            GenerateReadMe(project);

            return project;
        }

        private static void GenerateAppSettings(DapperProject project)
        {
            CSharpClassBuilder.Create(project.OutputDirectory, project.GetDataLayerDirectory(), project.Settings.ForceOverwrite, project.GetAppSettingsClassDefinition());
        }

        private static void GenerateDataLayerContract(DapperProject project, CSharpInterfaceDefinition interfaceDefinition)
        {
            CSharpInterfaceBuilder.Create(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.Settings.ForceOverwrite, interfaceDefinition);
        }

        private static void GenerateDataRepositories(DapperProject project)
        {
            GenerateRepositoryInterface(project);
            GenerateBaseRepositoryClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = projectFeature.GetRepositoryClassDefinition();

                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = repositoryClassDefinition,
                    OutputDirectory = project.OutputDirectory,
                    ForceOverwrite = project.Settings.ForceOverwrite
                };

                var interfaceDefinition = repositoryClassDefinition.RefactInterface();

                interfaceDefinition.Implements.Add("IRepository");

                interfaceDefinition.Namespace = project.GetDataLayerContractsNamespace();

                GenerateDataLayerContract(project, interfaceDefinition);

                codeBuilder.CreateFile(project.GetDataLayerRepositoriesDirectory());
            }
        }

        private static void GenerateRepositoryInterface(DapperProject project)
        {
            CSharpInterfaceBuilder.Create(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.Settings.ForceOverwrite, project.GetRepositoryInterfaceDefinition());
        }

        private static void GenerateBaseRepositoryClassDefinition(DapperProject project)
        {
            CSharpClassBuilder.Create(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), project.Settings.ForceOverwrite, project.GetRepositoryBaseClassDefinition());
        }

        private static void GenerateReadMe(this DapperProject project)
        {
            var lines = new List<string>
            {
                "CatFactory: Code Generation Made Easy",
                string.Empty,

                "How to use this code on your ASP.NET Core Application",
                string.Empty,

                "Register objects in Startup class, register your repositories in ConfigureServices method:",
                " services.AddScoped<IDboRepository, DboRepository>();",
                string.Empty,

                "Happy coding!",
                string.Empty,

                "You can check source code on GitHub:",
                "https://github.com/hherzl/CatFactory.Dapper",
                string.Empty,
                "*** Special Thanks for Edson Ferreira to let me help for Dapper community ***",
                string.Empty,
                "CatFactory Development Team ==^^=="
            };

            TextFileHelper.CreateFile(Path.Combine(project.OutputDirectory, "ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
