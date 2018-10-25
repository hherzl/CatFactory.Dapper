using CatFactory.Dapper.Definitions.Extensions;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

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

                var classDefinition = project.CreateEntity(table);

                if (project.Database.HasDefaultSchema(table))
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, classDefinition);
                else
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(table.Schema), selection.Settings.ForceOverwrite, classDefinition);
            }

            foreach (var view in project.Database.Views)
            {
                var selection = project.GetSelection(view);

                var classDefinition = project.CreateEntity(view);

                if (project.Database.HasDefaultSchema(view))
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, classDefinition);
                else
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(view.Schema), selection.Settings.ForceOverwrite, classDefinition);
            }

            foreach (var tableFunction in project.Database.TableFunctions)
            {
                var selection = project.GetSelection(tableFunction);

                var classDefinition = project.CreateEntity(tableFunction);

                if (project.Database.HasDefaultSchema(tableFunction))
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(), selection.Settings.ForceOverwrite, project.CreateEntity(tableFunction));
                else
                    CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetEntityLayerDirectory(tableFunction.Schema), selection.Settings.ForceOverwrite, classDefinition);
            }

            return project;
        }
    }
}
