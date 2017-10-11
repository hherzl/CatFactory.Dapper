using System;
using System.Collections.Generic;
using System.IO;
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

        private static void GenerateReadMe(this DapperProject project)
        {
            var lines = new List<String>();

            lines.Add("CatFactory: Code Generation Made Easy");
            lines.Add(String.Empty);

            lines.Add("How to use this code on your ASP.NET Core Application");
            lines.Add(String.Empty);

            lines.Add("Register objects in Startup class, register your repositories in ConfigureServices method:");
            lines.Add(" services.AddScoped<IDboRepository, DboRepository>();");
            lines.Add(String.Empty);

            lines.Add("Happy coding!");
            lines.Add(String.Empty);

            lines.Add("You can check source code on GitHub:");
            lines.Add("https://github.com/hherzl/CatFactory.Dapper");
            lines.Add(String.Empty);
            lines.Add("*** Special Thanks for Edson Ferreira to let me help to Dapper community ***");
            lines.Add(String.Empty);
            lines.Add("CatFactory Development Team ==^^==");

            TextFileHelper.CreateFile(Path.Combine(project.OutputDirectory, "ReadMe.txt"), lines.ToStringBuilder().ToString());
        }
    }
}
