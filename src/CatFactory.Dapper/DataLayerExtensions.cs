using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

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

    public class RepositoryInterfaceDefinition : CSharpInterfaceDefinition
    {
        public RepositoryInterfaceDefinition(DapperProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public DapperProject Project { get; }

        public void Init()
        {
            Namespace = Project.GetDataLayerContractsNamespace();

            Name = "IRepository";
        }
    }

    public class RepositoryBaseClassDefinition : CSharpClassDefinition
    {
        public RepositoryBaseClassDefinition(DapperProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public DapperProject Project { get; }

        public void Init()
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Microsoft.Extensions.Options");

            Namespaces.Add(Project.GetEntityLayerNamespace());

            Namespace = Project.GetDataLayerContractsNamespace();

            Name = "Repository";

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
            {
                Lines = new List<ILine>()
                {
                    new CodeLine("ConnectionString = appSettings.Value.ConnectionString;")
                }
            });

            Properties.Add(new PropertyDefinition("String", "ConnectionString ") { AccessModifier = AccessModifier.Protected, IsReadOnly = true });
        }
    }
}
