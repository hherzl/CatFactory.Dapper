using CatFactory.ObjectRelationalMapping;

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
                        PrimaryKey = new PrimaryKey("COMPANY_ID")
                    }
                },
                NamingConvention = new SqlServer.SqlServerDatabaseNamingConvention()
            }
            .AddDbObjectsFromTables();
    }
}
