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
                .Import(logger, "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Store.Dapper.API\src\Store.Dapper.API",
            };

            // Apply settings for project
            project.Settings.ForceOverwrite = true;
            project.Settings.UpdateExclusions.AddRange(new string[] { "CreationUser", "CreationDateTime", "Timestamp" });
            project.Settings.InsertExclusions.AddRange(new string[] { "LastUpdateUser", "LastUpdateDateTime", "Timestamp" });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectGenerationFromNorthwindDatabaseTest()
        {
            var logger = LoggerMocker.GetLogger<SqlServerDatabaseFactory>();

            // Import database
            var database = SqlServerDatabaseFactory
                .Import(logger, "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Northwind.Dapper.API\src\Northwind.Dapper.API"
            };

            // Apply settings for project
            project.Settings.ForceOverwrite = true;

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
