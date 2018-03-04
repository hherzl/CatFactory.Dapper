using System.Collections.Generic;
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
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Store.Dapper.API\src\Store.Dapper.API"
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
                settings.AddPagingForGetAllOperations = true;
            });

            project.Select("Sales.Order", settings => settings.AddPagingForGetAllOperations = true);

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
                .Import(LoggerMocker.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Northwind",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Northwind.Dapper.API\src\Northwind.Dapper.API"
            };

            // Apply settings for project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
            });

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
            var factory = new SqlServerDatabaseFactory(LoggerMocker.GetLogger<SqlServerDatabaseFactory>())
            {
                ConnectionString = "server=(local);database=AdventureWorks2012;integrated security=yes;",
                ImportSettings = new DatabaseImportSettings
                {
                    Exclusions = new List<string> { "dbo.sysdiagrams" },
                    ExclusionTypes = new List<string> { "geography" },
                    ImportTableFunctions = true
                }
            };

            var database = factory.Import();

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "AdventureWorks",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\AdventureWorks.Dapper.API\src\AdventureWorks.Dapper.API"
            };

            // Apply settings for project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
            });

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
