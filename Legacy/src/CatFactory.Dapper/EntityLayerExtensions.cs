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
                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.CreateEntity(table));
            }

            foreach (var view in project.Database.Views)
            {
                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.CreateView(view));
            }

            foreach (var tableFunction in project.Database.TableFunctions)
            {
                CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), project.GlobalSelection().Settings.ForceOverwrite, project.CreateView(tableFunction));
            }

            return project;
        }
    }
}
