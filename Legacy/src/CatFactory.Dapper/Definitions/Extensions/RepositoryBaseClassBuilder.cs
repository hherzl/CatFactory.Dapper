using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class RepositoryBaseClassBuilder
    {
        public static RepositoryBaseClassDefinition GetRepositoryBaseClassDefinition(this DapperProject project)
        {
            var classDefinition = new RepositoryBaseClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Linq");
            classDefinition.Namespaces.Add("System.Threading.Tasks");
            classDefinition.Namespaces.Add("Microsoft.Extensions.Options");

            classDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

            classDefinition.Namespace = project.GetDataLayerContractsNamespace();

            classDefinition.Name = "Repository";

            classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
            {
                Lines = new List<ILine>
                {
                    new CodeLine("ConnectionString = appSettings.Value.ConnectionString;")
                }
            });

            classDefinition.Properties.Add(new PropertyDefinition("String", "ConnectionString ")
            {
                AccessModifier = AccessModifier.Protected,
                IsReadOnly = true
            });

            return classDefinition;
        }
    }
}
