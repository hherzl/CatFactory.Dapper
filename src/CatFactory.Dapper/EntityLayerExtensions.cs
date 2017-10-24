using CatFactory.Dapper.Definitions;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class EntityLayerExtensions
    {
        private static void GenerateEntityInterface(this DapperProject project)
        {
            var interfaceDefinition = new CSharpInterfaceDefinition
            {
                Namespace = project.GetEntityLayerNamespace(),
                Name = "IEntity"
            };

            CSharpInterfaceBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, interfaceDefinition);
        }

        public static DapperProject GenerateEntityLayer(this DapperProject project)
        {
            project.GenerateEntityInterface();

            foreach (var table in project.Database.Tables)
            {
                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.CreateEntity(table));
            }

            foreach (var view in project.Database.Views)
            {
                CSharpClassBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.Settings.ForceOverwrite, project.CreateView(view));
            }

            return project;
        }
    }
}
