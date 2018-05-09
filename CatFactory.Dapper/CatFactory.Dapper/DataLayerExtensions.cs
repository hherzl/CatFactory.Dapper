using System.Collections.Generic;
using System.IO;
using CatFactory.Collections;
using CatFactory.Dapper.Definitions.Extensions;
using CatFactory.NetCore;

namespace CatFactory.Dapper
{
    public static class DataLayerExtensions
    {
        public static DapperProject ScaffoldDataLayer(this DapperProject project)
        {
            ScaffoldAppSettings(project);
            ScaffoldDataRepositories(project);
            ScaffoldReadMe(project);

            return project;
        }

        private static void ScaffoldAppSettings(DapperProject project)
        {
            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.GetAppSettingsClassDefinition());
        }

        private static void ScaffoldDataLayerContract(DapperProject project, CSharpInterfaceDefinition interfaceDefinition)
        {
            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), project.GlobalSelection().Settings.ForceOverwrite, interfaceDefinition);
        }

        private static void ScaffoldDataRepositories(DapperProject project)
        {
            var projectSelection = project.GlobalSelection();

            ScaffoldRepositoryInterface(project);
            ScaffoldBaseRepositoryClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = projectFeature.GetRepositoryClassDefinition();

                var interfaceDefinition = repositoryClassDefinition.RefactInterface();

                interfaceDefinition.Implements.Add("IRepository");

                interfaceDefinition.Namespace = project.GetDataLayerContractsNamespace();

                ScaffoldDataLayerContract(project, interfaceDefinition);

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), projectSelection.Settings.ForceOverwrite, repositoryClassDefinition);
            }
        }

        private static void ScaffoldRepositoryInterface(DapperProject project)
        {
            var projectSelection = project.GlobalSelection();

            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerContractsDirectory(), projectSelection.Settings.ForceOverwrite, project.GetRepositoryInterfaceDefinition());
        }

        private static void ScaffoldBaseRepositoryClassDefinition(DapperProject project)
        {
            var projectSelection = project.GlobalSelection();

            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerRepositoriesDirectory(), projectSelection.Settings.ForceOverwrite, project.GetRepositoryBaseClassDefinition());
        }

        private static void ScaffoldReadMe(this DapperProject project)
        {
            var lines = new List<string>
            {
                "CatFactory: Scaffolding Made Easy",
                string.Empty,

                "How to use this code on your ASP.NET Core Application",
                string.Empty,

                "Register objects in Startup class, register your repositories in ConfigureServices method:",
                " services.AddScoped<IDboRepository, DboRepository>();",
                string.Empty,

                "Happy coding!",
                string.Empty,

                "You can check the guide for this package in:",
                "https://www.codeproject.com/Articles/1213355/Scaffolding-Dapper-with-CatFactory",
                string.Empty,
                "You can check source code on GitHub:",
                "https://github.com/hherzl/CatFactory.Dapper",
                string.Empty,
                "*** Special Thanks for Edson Ferreira to let me help for Dapper community ***",
                string.Empty,
                "CatFactory Development Team ==^^=="
            };

            TextFileHelper.CreateFile(Path.Combine(project.OutputDirectory, "CatFactory.Dapper.ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
