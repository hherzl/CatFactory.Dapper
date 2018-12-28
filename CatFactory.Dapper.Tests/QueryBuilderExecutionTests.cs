using System.Collections.Generic;
using System.Data.SqlClient;
using CatFactory.Dapper.Sql;
using CatFactory.Dapper.Sql.Dml;
using CatFactory.Dapper.Tests.Models;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class QueryBuilderExecutionTests
    {
        [Fact]
        public void TestSelect()
        {
            // Arrange
            var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;");

            var query = QueryBuilder
                .Select<Shipper>("dbo.Shippers");

            var list = new List<Shipper>();

            // Act
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = query.ToString();

            using (var dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    list.Add(new Shipper
                    {
                        ShipperID = dataReader.GetInt32(0),
                        CompanyName = dataReader.GetString(1),
                        Phone = dataReader.GetString(2)
                    });
                }
            }

            connection.Dispose();

            // Assert
            Assert.True(list.Count > 0);
        }

        [Fact]
        public void TestSelectByID()
        {
            // Arrange
            var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;");

            var query = QueryBuilder
                .Select<Shipper>("dbo.Shippers", new SqlServerDatabaseNamingConvention())
                .Where("ShipperID", ComparisonOperator.Equals, 1);

            var entity = default(Shipper);

            // Act
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = query.ToString();

            foreach (var condition in query.Where)
            {
                command.Parameters.Add(new SqlParameter(query.NamingConvention.GetParameterName(condition.Column), condition.Value));
            }

            using (var dataReader = command.ExecuteReader())
            {
                dataReader.Read();

                entity = new Shipper
                {
                    ShipperID = dataReader.GetInt32(0),
                    CompanyName = dataReader.GetString(1),
                    Phone = dataReader.GetString(2)
                };
            }

            connection.Dispose();

            // Assert
            Assert.False(entity == null);
        }

        [Fact]
        public void TestSelectSearchByIDAndName()
        {
            // Arrange
            var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;");

            var query = QueryBuilder
                .Select<Shipper>("dbo.Shippers")
                .Where("ShipperID", ComparisonOperator.Equals, 1)
                .And("CompanyName", ComparisonOperator.Equals, "Speedy Express");

            var list = new List<Shipper>();

            // Act
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = query.ToString();

            foreach (var condition in query.Where)
            {
                command.Parameters.Add(new SqlParameter(query.NamingConvention.GetParameterName(condition.Column), condition.Value));
            }

            using (var dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    list.Add(new Shipper
                    {
                        ShipperID = dataReader.GetInt32(0),
                        CompanyName = dataReader.GetString(1),
                        Phone = dataReader.GetString(2)
                    });
                }
            }

            connection.Dispose();

            // Assert
            Assert.True(list.Count > 0);
        }
    }
}
