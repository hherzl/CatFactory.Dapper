using System.Collections.Generic;
using System.IO;
using CatFactory.Collections;
using CatFactory.Dapper.Definitions.Extensions;
using CatFactory.NetCore.ObjectOrientedProgramming;

namespace CatFactory.Dapper
{
    public static class DataLayerExtensions
    {
        public static DapperProject ScaffoldDataLayer(this DapperProject project)
        {
            ScaffoldRepositories(project);
            ScaffoldReadMe(project);

            return project;
        }

        private static void ScaffoldRepositories(DapperProject project)
        {
            ScaffoldRepositoryInterface(project);

            ScaffoldBaseRepositoryClassDefinition(project);

            foreach (var projectFeature in project.Features)
            {
                var repositoryClassDefinition = projectFeature.GetRepositoryClassDefinition();

                project.Scaffold(repositoryClassDefinition, project.GetDataLayerRepositoriesDirectory());

                var interfaceDefinition = repositoryClassDefinition.RefactInterface();

                interfaceDefinition.Namespace = project.GetDataLayerContractsNamespace();

                interfaceDefinition.Implements.Add("IRepository");

                project.Scaffold(interfaceDefinition, project.GetDataLayerContractsDirectory());
            }
        }

        private static void ScaffoldRepositoryInterface(DapperProject project)
        {
            project.Scaffold(project.GetRepositoryInterfaceDefinition(), project.GetDataLayerContractsDirectory());
        }

        private static void ScaffoldBaseRepositoryClassDefinition(DapperProject project)
        {
            project.Scaffold(project.GetRepositoryBaseClassDefinition(), project.GetDataLayerRepositoriesDirectory());
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
                "*** Special Thanks for Edson Ferreira to let me help to Dapper community ***",
                string.Empty,
                "CatFactory Development Team ==^^=="
            };

            File.WriteAllText(Path.Combine(project.OutputDirectory, "CatFactory.Dapper.ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
