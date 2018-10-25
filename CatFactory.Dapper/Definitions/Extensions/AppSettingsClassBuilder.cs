using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class AppSettingsClassBuilder
    {
        public static AppSettingsClassDefinition GetAppSettingsClassDefinition(this DapperProject project)
            => new AppSettingsClassDefinition
            {
                Namespace = project.GetDataLayerNamespace(),
                Name = "AppSettings",
                Properties =
                {
                    new PropertyDefinition("string", "ConnectionString")
                }
            };
    }
}
