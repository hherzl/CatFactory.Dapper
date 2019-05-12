using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using CatFactory.Dapper.Sql;
using CatFactory.Dapper.Sql.Dml;
using CatFactory.Dapper.Tests.Models;
using CatFactory.SqlServer;
using Xunit;

namespace CatFactory.Dapper.Tests
{
    public class QueryBuilderExecutionTests
    {
        public QueryBuilderExecutionTests()
        {
            QueryBuilder.DatabaseNamingConvention = new SqlServerDatabaseNamingConvention();
        }

        [Fact]
        public void TestSelect()
        {
            // Arrange
            using (var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;"))
            {
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

                // Assert
                Assert.True(list.Count > 0);
            }
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
            Assert.True(list.Count == 1);
        }

        [Fact]
        public void TestBuildSelectCommandExtensionMethod()
        {
            // Arrange
            // Act

            var list = new List<Product>();

            using (var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;"))
            {
                connection.Open();

                var command = QueryBuilder
                    .Select<Product>("dbo.Products")
                    .And("ProductName", ComparisonOperator.Like, "%ha%")
                    .CreateCommand(connection);

                File.WriteAllText(@"C:\Temp\CatFactory.Dapper\Queries\SelectProductByProductName.txt", command.CommandText);

                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        list.Add(new Product
                        {
                            ProductID = dataReader.GetInt32(0),
                            ProductName = dataReader.GetString(1)
                        });
                    }
                }
            }

            // Assert

            Assert.True(list.Count > 0);
        }

        [Fact]
        public void TestBuildInsertIntoCommandExtensionMethod()
        {
            using (var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;"))
            {
                connection.Open();

                var entity = new Shipper
                {
                    CompanyName = "Company name from unit tests",
                    Phone = "778899665544"
                };

                var command = QueryBuilder
                    .InsertInto(entity, "dbo.Shippers", "ShipperID")
                    .CreateCommand(connection);

                File.WriteAllText(@"C:\Temp\CatFactory.Dapper\Queries\InsertIntoShipper.txt", command.CommandText);

                var affectedRows = command.ExecuteNonQuery();

                var outputParameter = (SqlParameter)command.Parameters["@shipperID"];

                Assert.True(affectedRows == 1);
                Assert.True((int)outputParameter.Value > 0);
            }
        }

        [Fact]
        public void TestBuildUpdateCommandExtensionMethod()
        {
            using (var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;"))
            {
                connection.Open();

                var entity = new Shipper
                {
                    ShipperID = 5,
                    CompanyName = "Company name update from unit tests",
                    Phone = "2244668800"
                };

                var command = QueryBuilder
                    .Update(entity, "dbo.Shippers", "ShipperID")
                    .CreateCommand(connection);

                File.WriteAllText(@"C:\Temp\CatFactory.Dapper\Queries\UpdateShipper.txt", command.CommandText);

                var affectedRows = command.ExecuteNonQuery();

                var outputParameter = (SqlParameter)command.Parameters["@shipperID"];

                Assert.True(affectedRows >= 0);
                Assert.True((int)outputParameter.Value > 0);
            }
        }

        [Fact]
        public void TestBuildDeleteCommandExtensionMethod()
        {
            using (var connection = new SqlConnection("server=(local);database=Northwind;integrated security=yes;"))
            {
                connection.Open();

                var entity = new Shipper
                {
                    ShipperID = 6
                };

                var command = QueryBuilder
                    .DeleteFrom(entity, "dbo", "Shippers", "ShipperID")
                    .CreateCommand(connection);

                File.WriteAllText(@"C:\Temp\CatFactory.Dapper\Queries\DeleteShipper.txt", command.CommandText);

                var affectedRows = command.ExecuteNonQuery();

                Assert.True(affectedRows >= 0);
            }
        }
    }
}
