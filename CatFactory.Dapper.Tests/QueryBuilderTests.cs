using CatFactory.Dapper.Sql;
using CatFactory.Dapper.Sql.Dml;
using CatFactory.Dapper.Tests.Models;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class QueryBuilderTests
    {
        [Fact]
        public void SelectAll()
        {
            // Arrange
            var query = QueryBuilder
                .Select<Shipper>();

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 3);
            Assert.True(query.From == "Shipper");
        }

        [Fact]
        public void SelectByID()
        {
            // Arrange
            var query = QueryBuilder
                .Select<Shipper>()
                .Where("ShipperID", ComparisonOperator.Equals, 1);

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 3);
            Assert.True(query.From == "Shipper");
            Assert.True(query.Where.Count == 1);
        }

        [Fact]
        public void SelectSearch()
        {
            // Arrange
            var query = QueryBuilder
                .Select<Shipper>()
                .Where("ShipperID", ComparisonOperator.Equals, 1)
                .And("CompanyName", ComparisonOperator.Equals, "Speedy Express");

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 3);
            Assert.True(query.From == "Shipper");
            Assert.True(query.Where.Count == 2);
        }

        [Fact]
        public void InsertInto()
        {
            // Arrange
            var query = QueryBuilder
                .InsertInto(new Shipper(), identity: "ShipperID");

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 2);
            Assert.True(query.Identity == "ShipperID");
        }

        [Fact]
        public void UpdateByKey()
        {
            // Arrange
            var query = QueryBuilder
                .Update(new Shipper(), key: "ShipperID");

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 2);
            Assert.True(query.Key == "ShipperID");
        }

        [Fact]
        public void DeleteByKey()
        {
            // Arrange
            var query = QueryBuilder
                .DeleteFrom(new Shipper(), key: "ShipperID");

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Table == "Shipper");
            Assert.True(query.Key == "ShipperID");
        }
    }
}
