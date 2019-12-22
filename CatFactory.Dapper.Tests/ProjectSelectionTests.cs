using System.Threading.Tasks;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class ProjectSelectionTests
    {
        [Fact]
        public async Task ProjectSelectionScopeAsync()
        {
            // Arrange

            // Get database
            var database = await SqlServerDatabaseFactory
                .ImportAsync(SqlServerDatabaseFactory.GetLogger(), "server=(local);database=OnlineStore;integrated security=yes;", "dbo.sysdiagrams");

            // Create instance of Entity Framework Core project
            var project = DapperProject
                .Create("OnlineStore", database, @"C:\Temp\CatFactory.Dapper\OnlineStore.Core");

            // Act

            // Apply settings for Entity Framework Core project
            project.GlobalSelection(settings => settings.ForceOverwrite = true);

            project.Selection("Sales.OrderHeader", settings => settings.UseStringBuilderForQueries = false);

            var orderHeader = database.FindTable("Sales.OrderHeader");

            var selectionForOrder = project.GetSelection(orderHeader);

            // Assert

            Assert.True(project.Selections.Count == 2);

            Assert.True(project.GlobalSelection().Settings.UseStringBuilderForQueries == true);

            Assert.True(selectionForOrder.Settings.UseStringBuilderForQueries == false);
        }
    }
}
