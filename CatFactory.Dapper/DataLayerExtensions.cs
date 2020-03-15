using System.IO;
using CatFactory.Dapper.Definitions.Extensions;
using CatFactory.Markdown;
using CatFactory.NetCore.ObjectOrientedProgramming;

namespace CatFactory.Dapper
{
    public static class DataLayerExtensions
    {
        public static DapperProject ScaffoldDataLayer(this DapperProject project)
        {
            ScaffoldRepositories(project);
            ScaffoldMdReadMe(project);

            return project;
        }

        internal static void ScaffoldRepositories(DapperProject project)
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

        internal static void ScaffoldRepositoryInterface(DapperProject project)
        {
            project.Scaffold(project.GetRepositoryInterfaceDefinition(), project.GetDataLayerContractsDirectory());
        }

        internal static void ScaffoldBaseRepositoryClassDefinition(DapperProject project)
        {
            project.Scaffold(project.GetRepositoryBaseClassDefinition(), project.GetDataLayerRepositoriesDirectory());
        }

        internal static void ScaffoldMdReadMe(this DapperProject project)
        {
            var readMe = new MdDocument();

            readMe.H1("CatFactory ==^^==: Scaffolding Made Easy");

            readMe.WriteLine("How to use this code on your ASP.NET Core Application:");

            readMe.OrderedList(
                "Install SqlClient and Dapper packages",
                "Register the Repositories in ConfigureServices method (Startup class)"
                );

            readMe.H2("Install packages");

            readMe.WriteLine("You can install the NuGet packages in Visual Studio or Windows Command Line, for more info:");

            readMe.WriteLine(
                Md.Link("Install and manage packages with the Package Manager Console in Visual Studio (PowerShell)", "https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell")
                );

            readMe.WriteLine(
                Md.Link(".NET Core CLI", "https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package")
                );

            readMe.H2("Register Repositories");

            readMe.WriteLine("Add the following code lines in {0} method (Startup class):", Md.Bold("ConfigureServices"));
            readMe.WriteLine("  services.AddScope<{0}, {1}>()", "IDboRepository", "DboRepository");

            readMe.WriteLine("Happy scaffolding!");

            var codeProjectLink = Md.Link("Scaffolding Dapper with CatFactory", "https://www.codeproject.com/Articles/1213355/Scaffolding-Dapper-with-CatFactory");

            readMe.WriteLine("You can check the guide for this package in: {0}", codeProjectLink);

            var gitHubRepositoryLink = Md.Link("GitHub repository", "https://github.com/hherzl/CatFactory.Dapper");

            readMe.WriteLine("Also you can check the source code on {0}", gitHubRepositoryLink);

            readMe.WriteLine("Special Thanks for {0} to let me help to Dapper community", Md.Link("Edson Ferreira", "https://github.com/EdsonF"));

            readMe.WriteLine("CatFactory Development Team ==^^==");

            File.WriteAllText(Path.Combine(project.OutputDirectory, "CatFactory.Dapper.ReadMe.MD"), readMe.ToString());
        }
    }
}
