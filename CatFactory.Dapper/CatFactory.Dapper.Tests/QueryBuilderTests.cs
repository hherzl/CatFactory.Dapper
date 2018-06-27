﻿using CatFactory.Dapper.Sql;
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
            // Act
            var query = QueryBuilder
                .Select<Shipper>();

            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 3);
            Assert.True(query.From == "Shipper");
        }

        [Fact]
        public void TestSelectByID()
        {
            // Arrange
            // Act
            var query = QueryBuilder
                .Select<Shipper>()
                .Where("ShipperID", ComparisonOperator.Equals, 1);

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
            // Act
            var query = QueryBuilder
                .Select<Shipper>()
                .Where("ShipperID", ComparisonOperator.Equals, 1)
                .And("CompanyName", ComparisonOperator.Equals, "Speedy Express");

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
            // Act
            var query = QueryBuilder
                .InsertInto<Shipper>(identity: "ShipperID");

            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 2);
            Assert.True(query.Identity == "ShipperID");
        }

        [Fact]
        public void TestUpdate()
        {
            // Arrange
            // Act
            var query = QueryBuilder
                .Update<Shipper>(key: "ShipperID");

            var sql = query.ToString();

            // Assert
            Assert.True(query.Columns.Count == 2);
            Assert.True(query.Key == "ShipperID");
        }

        [Fact]
        public void TestDelete()
        {
            // Arrange
            // Act
            var query = QueryBuilder
                .DeleteFrom<Shipper>(key: "ShipperID");

            var sql = query.ToString();

            // Assert
            Assert.True(query.Table == "Shipper");
            Assert.True(query.Key == "ShipperID");
        }
    }
}
