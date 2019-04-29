using CatFactory.Dapper.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.Dapper
{
    public static class EntityLayerExtensions
    {
        private static void ScaffoldEntityInterface(this DapperProject project)
        {
            project.Scaffold(project.GetEntityInterfaceDefinition(), project.GetEntityLayerDirectory());
        }

        public static DapperProject ScaffoldEntityLayer(this DapperProject project)
        {
            project.ScaffoldEntityInterface();

            foreach (var table in project.Database.Tables)
            {
                var definition = project.GetEntityClassDefinition(table);

                project.Scaffold(definition, project.GetEntityLayerDirectory(project.Database.HasDefaultSchema(table) ? "" : table.Schema));
            }

            foreach (var view in project.Database.Views)
            {
                var definition = project.GetEntityClassDefinition(view);

                project.Scaffold(definition, project.GetEntityLayerDirectory(project.Database.HasDefaultSchema(view) ? "" : view.Schema));
            }

            foreach (var tableFunction in project.Database.TableFunctions)
            {
                var selection = project.GetSelection(tableFunction);

                var definition = project.GetEntityClassDefinition(tableFunction);

                project.Scaffold(definition, project.GetEntityLayerDirectory(project.Database.HasDefaultSchema(tableFunction) ? "" : tableFunction.Schema));
            }

            foreach (var storedProcedure in project.Database.StoredProcedures)
            {
                var selection = project.GetSelection(storedProcedure);

                var definition = project.GetEntityClassDefinition(storedProcedure);

                project.Scaffold(definition, project.GetEntityLayerDirectory(project.Database.HasDefaultSchema(storedProcedure) ? "" : storedProcedure.Schema));
            }

            return project;
        }
    }
}
