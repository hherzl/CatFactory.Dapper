using CatFactory.Dapper.Sql;
using CatFactory.Dapper.Sql.Dml;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class QueryBuilderTests
    {
        [Fact]
        public void TestSelect()
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
        public void TestSelectByID()
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
        public void TestSelectSearch()
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
        public void TestInsertInto()
        {
            // Arrange
            var query = QueryBuilder
                .InsertInto<Shipper>(identity: "ShipperID");

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 2);
            Assert.True(query.Identity == "ShipperID");
        }

        [Fact]
        public void TestUpdate()
        {
            // Arrange
            var query = QueryBuilder
                .Update<Shipper>(key: "ShipperID");

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 2);
            Assert.True(query.Key == "ShipperID");
        }

        [Fact]
        public void TestDelete()
        {
            // Arrange
            var query = QueryBuilder
                .DeleteFrom<Shipper>(key: "ShipperID");

            // Act
            var sql = query.ToString();

            // Assert
            Assert.True(query.Table == "Shipper");
            Assert.True(query.Key == "ShipperID");
        }
    }
}
