using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class ImportTests
    {
        [Fact]
        public void ProjectScaffoldingFromExistingDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerHelper.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Store\Store.Dapper.API"
            };

            // Apply settings for project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.UpdateExclusions.AddRange(new string[] { "CreationUser", "CreationDateTime", "Timestamp" });
                settings.InsertExclusions.AddRange(new string[] { "LastUpdateUser", "LastUpdateDateTime", "Timestamp" });
            });

            project.Select("Production.*", settings =>
            {
                settings.UseStringBuilderForQueries = false;
                settings.AddPagingForGetAllOperation = true;
            });

            project.Select("Sales.Order", settings => settings.AddPagingForGetAllOperation = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Add event handlers to before and after of scaffold

            project.ScaffoldingDefinition += (source, args) =>
            {
                // Add code to perform operations with code builder instance before to create code file
            };

            project.ScaffoldedDefinition += (source, args) =>
            {
                // Add code to perform operations after of create code file
            };

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingFromNorthwindDatabaseTest()
        {
            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerHelper.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Northwind\Northwind.Dapper.API"
            };

            // Apply settings for project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingFromAdventureWorksDatabaseTest()
        {
            // Import database
            var databaseFactory = new SqlServerDatabaseFactory(LoggerHelper.GetLogger<SqlServerDatabaseFactory>())
            {
                ConnectionString = "server=(local);database=AdventureWorks2017;integrated security=yes;",
                ImportSettings = new DatabaseImportSettings
                {
                    Exclusions = { "dbo.sysdiagrams" },
                    ExclusionTypes = { "geography" },
                    ImportTableFunctions = true,
                    ImportScalarFunctions = true
                }
            };

            var database = databaseFactory.Import();

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "AdventureWorks",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\AdventureWorks\AdventureWorks.Dapper.API"
            };

            // Apply settings for project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            project.Select("Sales.SalesOrderHeader", settings => settings.AddPagingForGetAllOperation = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
