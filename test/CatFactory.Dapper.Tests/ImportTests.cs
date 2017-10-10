using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class ImportTests
    {
        [Fact]
        public void ProjectGenerationFromExistingDatabaseTest()
        {
            var logger = LoggerMocker.GetLogger<SqlServerDatabaseFactory>();

            // Import database
            var database = SqlServerDatabaseFactory
                .Import("server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Ef Core Project
            var project = new DapperProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Store.Dapper.API\src\Store.Dapper.API"
            };

            project.Settings.Exclusions.Add("Timestamp");

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =^^=
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }

        [Fact]
        public void ProjectGenerationFromNorthwindDatabaseTest()
        {
            var logger = LoggerMocker.GetLogger<SqlServerDatabaseFactory>();

            // Import database
            var database = SqlServerDatabaseFactory
                .Import("server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Ef Core Project
            var project = new DapperProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Northwind.Dapper.API\src\Northwind.Dapper.API"
            };

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Generate code =^^=
            project
                .GenerateEntityLayer()
                .GenerateDataLayer();
        }
    }
}
