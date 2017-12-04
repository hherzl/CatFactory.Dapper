using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DbObjectExtensions
    {
        static DbObjectExtensions()
        {
        }

        public static bool IsPrimaryKeyGuid(this ITable table)
            => table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1 && table.Columns[0].Type == "uniqueidentifier" ? true : false;

        public static bool HasDefaultSchema(this IDbObject table)
            => string.IsNullOrEmpty(table.Schema) || string.Compare(table.Schema, "dbo", true) == 0;
    }
}
