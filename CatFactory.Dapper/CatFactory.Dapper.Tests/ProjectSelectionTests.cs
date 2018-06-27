using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class ProjectSelectionTests
    {
        [Fact]
        public void TestProjectSelectionScope()
        {
            // Arrange

            // Import database
            var database = SqlServerDatabaseFactory
                .Import(LoggerHelper.GetLogger<SqlServerDatabaseFactory>(), "server=(local);database=Store;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core project
            var project = new DapperProject
            {
                Name = "Store",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.Dapper\\Store"
            };

            // Act

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
            });

            project.Select("Sales.Order", settings => settings.UseStringBuilderForQueries = false);

            var order = database.FindTable("Sales.Order");

            var selectionForOrder = project.GetSelection(order);

            // Assert
            Assert.True(project.Selections.Count == 2);
            Assert.True(project.GlobalSelection().Settings.UseStringBuilderForQueries == true);
            Assert.True(selectionForOrder.Settings.UseStringBuilderForQueries == false);
        }
    }
}
