using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.Dapper
{
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
