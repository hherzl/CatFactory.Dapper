using System.Linq;
using CatFactory.ObjectRelationalMapping;
using CatFactory.SqlServer;

namespace CatFactory.Dapper.Tests.Models
{
    public static class Databases
    {
        public static Database LegacyErpDatabase
            => new Database
            {
                Tables =
                {
                    new Table
                    {
                        Schema = "DBO",
                        Name = "COMPANY",
                        Columns =
                        {
                            new Column
                            {
                                Name = "COMPANY_ID",
                                Type = "int"
                            },
                            new Column
                            {
                                Name = "COMPANY_NAME",
                                Type = "nvarchar",
                                Length = 50
                            },
                            new Column
                            {
                                Name = "DESCRIPTION",
                                Type = "nvarchar"
                            }
                        },
                        PrimaryKey = new PrimaryKey("COMPANY_ID"),
                        Identity = new Identity("COMPANY_ID")
                    },
                    new Table
                    {
                        Schema = "DBO",
                        Name = "BRANCH",
                        Columns =
                        {
                            new Column
                            {
                                Name = "BRANCH_ID",
                                Type = "int"
                            },
                            new Column
                            {
                                Name = "COMPANY_ID",
                                Type = "int"
                            },
                            new Column
                            {
                                Name = "BRANCH_NAME",
                                Type = "nvarchar",
                                Length = 50
                            },
                            new Column
                            {
                                Name = "DESCRIPTION",
                                Type = "nvarchar"
                            }
                        },
                        PrimaryKey = new PrimaryKey("BRANCH_ID"),
                        Identity = new Identity("BRANCH_ID")
                    }
                },
                DefaultSchema = "DBO",
                NamingConvention = new SqlServer.SqlServerDatabaseNamingConvention(),
                DatabaseTypeMaps = new SqlServerDatabaseFactory().DatabaseTypeMaps.ToList()
            }
            .AddDbObjectsFromTables();
    }
}
