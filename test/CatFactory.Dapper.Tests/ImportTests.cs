using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class ImportTests
    {
        [Fact]
        public void ProjectGenerationFromExistingDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Ef Core Project
            var project = new DapperProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.Dapper\\Store"
            };

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =^^=
            //project
            //    .GenerateEntityLayer()
            //    .GenerateDataLayer();
        }
    }
}
