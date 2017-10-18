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

            var codeBuilder = new CSharpInterfaceBuilder
            {
                ObjectDefinition = interfaceDefinition,
                OutputDirectory = project.OutputDirectory,
                ForceOverwrite = project.Settings.ForceOverwrite
            };

            codeBuilder.CreateFile(project.GetEntityLayerDirectory());
        }

        public static DapperProject GenerateEntityLayer(this DapperProject project)
        {
            project.GenerateEntityInterface();

            foreach (var table in project.Database.Tables)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = project.CreateEntity(table),
                    OutputDirectory = project.OutputDirectory,
                    ForceOverwrite = project.Settings.ForceOverwrite
                };

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
            }

            foreach (var view in project.Database.Views)
            {
                var codeBuilder = new CSharpClassBuilder
                {
                    ObjectDefinition = project.CreateView(view),
                    OutputDirectory = project.OutputDirectory,
                    ForceOverwrite = project.Settings.ForceOverwrite
                };

                codeBuilder.CreateFile(project.GetEntityLayerDirectory());
            }

            return project;
        }
    }
}
