using System.Collections.Generic;
using CatFactory.Dapper.Tests.Models;
using CatFactory.ObjectRelationalMapping.Actions;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class ScaffoldingTests
    {
        [Fact]
        public void ProjectScaffoldingFromOnlineStoreDatabaseTest()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnlineStore;integrated security=yes;",
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
                Name = "OnlineStore.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\OnlineStore.Core"
            };

            /* Apply settings for project */

            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
                settings.UpdateExclusions = new List<string> { "CreationUser", "CreationDateTime", "Timestamp" };
                settings.InsertExclusions = new List<string> { "LastUpdateUser", "LastUpdateDateTime", "Timestamp" };
            });

            project.Selection("Warehouse.*", settings => settings.UseStringBuilderForQueries = false);

            project.Selection("Sales.*", settings => settings.AddPagingForGetAllOperation = true);

            project.Selection("Sales.OrderDetail", settings =>
                settings.RemoveAction<ReadAllAction>()
            );

            // Build features for project, group all entities by schema into a feature
            project.BuildFeatures();

            /* Add event handlers to before and after of scaffold */

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
            var databaseFactory = new SqlServerDatabaseFactory(SqlServerDatabaseFactory.GetLogger())
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=Northwind;integrated security=yes;",
                    ImportScalarFunctions = true,
                    ImportTableFunctions = true,
                    ImportStoredProcedures = true,
                    Exclusions =
                    {
                        "dbo.sp_alterdiagram",
                        "dbo.sp_creatediagram",
                        "dbo.sp_dropdiagram",
                        "dbo.sp_helpdiagramdefinition",
                        "dbo.sp_helpdiagrams",
                        "dbo.sp_renamediagram",
                        "dbo.sp_upgraddiagrams",
                        "dbo.sysdiagrams",
                        "dbo.fn_diagramobjects"
                    }
                }
            };

            var database = databaseFactory.Import();

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "Northwind.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\Northwind.Core"
            };

            // Apply settings for project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            project.Selection("dbo.Ten Most Expensive Products", settings => settings.UseStringBuilderForQueries = false);

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

        [Fact]
        public void ProjectScaffoldingFromLegacyErpDatabaseTest()
        {
            // Create database factory
            var databaseFactory = new SqlServerDatabaseFactory(SqlServerDatabaseFactory.GetLogger())
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=OnlineStore;integrated security=yes;",
                    ImportTableFunctions = true,
                    Exclusions =
                    {
                        "dbo.sysdiagrams",
                        "dbo.fn_diagramobjects"
                    }
                }
            };

            // Import database
            var database = Databases.LegacyErpDatabase;

            database.NamingConvention = new CatFactory.SqlServer.SqlServerDatabaseNamingConvention();

            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "OnlineStore.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\LegacyErp.Core"
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
        public void SqlDomScaffoldingFromWideWorldImportersDatabaseTest()
        {
            // Import database
            var factory = new SqlServerDatabaseFactory
            {
                DatabaseImportSettings = new DatabaseImportSettings
                {
                    ConnectionString = "server=(local);database=WideWorldImporters;integrated security=yes;",
                    ImportTables = false,
                    ImportViews = true,
                    ImportCommandText = "select 'sys' as 'schema_name', 'columns' as 'object_name', 'VIEW' as 'object_type'"
                }
            };

            var database = factory.Import();
            
            // Create instance of Dapper Project
            var project = new DapperProject
            {
                Name = "SqlDom.Core",
                Database = database,
                OutputDirectory = @"C:\Temp\CatFactory.Dapper\SqlDom.Core"
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
    }
}
