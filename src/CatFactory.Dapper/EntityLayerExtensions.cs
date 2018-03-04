using CatFactory.Dapper.Definitions.Extensions;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class EntityLayerExtensions
    {
        private static void ScaffoldEntityInterface(this DapperProject project)
        {
            var globalSelection = project.GlobalSelection();

            var interfaceDefinition = new CSharpInterfaceDefinition
            {
                Namespace = project.GetEntityLayerNamespace(),
                Name = "IEntity"
            };

            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), globalSelection.Settings.ForceOverwrite, interfaceDefinition);
        }

        public static DapperProject ScaffoldEntityLayer(this DapperProject project)
        {
            var globalSelection = project.GlobalSelection();

            project.ScaffoldEntityInterface();

            foreach (var table in project.Database.Tables)
            {
                var selection = project.GetSelection(table);

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), globalSelection.Settings.ForceOverwrite, project.CreateEntity(table));
            }

            foreach (var view in project.Database.Views)
            {
                var selection = project.GetSelection(view);

                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), globalSelection.Settings.ForceOverwrite, project.CreateView(view));
            }

            return project;
        }
    }
}
