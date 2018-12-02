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
                .Import(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=OnLineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core project
            var project = new DapperProject
            {
                Name = "OnLineStore",
                Database = database,
                OutputDirectory = "C:\\Temp\\CatFactory.Dapper\\OnLineStore.Core"
            };

            // Act

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings =>
            {
                settings.ForceOverwrite = true;
            });

            project.Select("Sales.OrderHeader", settings => settings.UseStringBuilderForQueries = false);

            var orderHeader = database.FindTable("Sales.OrderHeader");

            var selectionForOrder = project.GetSelection(orderHeader);

            // Assert
            Assert.True(project.Selections.Count == 2);
            Assert.True(project.GlobalSelection().Settings.UseStringBuilderForQueries == true);
            Assert.True(selectionForOrder.Settings.UseStringBuilderForQueries == false);
        }
    }
}
