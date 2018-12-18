using System.Collections.Generic;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class ImportTests
    {
        [Fact]
        public void ProjectScaffoldingFromOnLineStoreDatabaseTest()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory(SqlServerDatabaseFactory.GetLogger())
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnLineStore;integrated security=yes;",
                    ImportTableFunctions = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams",
                        "dbo.fn_diagramobjects"
                    }
                }
            };

            // Import database
            var database = databaseFactory.Import();

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "OnLineStore.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\OnLineStore.Core"
            };

            // Apply settings for project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.UpdateExclusions = new List<string> { "CreationUser", "CreationDateTime", "Timestamp" };
                settings.InsertExclusions = new List<string> { "LastUpdateUser", "LastUpdateDateTime", "Timestamp" };
            });

            project.Selection("Warehouse.*", settings =>
            {
                settings.UseStringBuilderForQueries = false;
                settings.AddPagingForGetAllOperation = true;
            });

            project.Selection("Sales.OrderHeader", settings => settings.AddPagingForGetAllOperation = true);

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
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=Northwind;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Northwind.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Northwind.Core"
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
            var databaseFactory = new SqlServerDatabaseFactory(SqlServerDatabaseFactory.GetLogger())
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=AdventureWorks2017;integrated security=yes;",
                    ImportTableFunctions = true,
                    ImportScalarFunctions = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams"
                    },
                    ExclusionTypes =
                    {
                        "geography"
                    }
                }
            };

            var database = databaseFactory.Import();

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "AdventureWorks.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\AdventureWorks.Core"
            };

            // Apply settings for project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            project.Selection("Sales.SalesOrderHeader", settings => settings.AddPagingForGetAllOperation = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }

        [Fact]
        public void ProjectScaffoldingFromWideWorldImportersDatabaseTest()
        {
            // Import database
            var databaseFactory = new SqlServerDatabaseFactory(SqlServerDatabaseFactory.GetLogger())
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=WideWorldImporters;integrated security=yes;",
                    Exclusions =
                    {
                        "dbo.sysdiagrams"
                    }
                }
            };

            var database = databaseFactory.Import();

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "WideWorldImporters.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\WideWorldImporters.Core"
            };

            // Apply settings for project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            project.Selection("Warehouse.StockItems", settings => settings.AddPagingForGetAllOperation = true);

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            // Scaffolding =^^=
            project
                .ScaffoldEntityLayer()
                .ScaffoldDataLayer();
        }
    }
}
