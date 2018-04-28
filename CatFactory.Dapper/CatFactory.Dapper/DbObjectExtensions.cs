using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DbObjectExtensions
    {
        static DbObjectExtensions()
        {
        }

        public static bool HasDefaultSchema(this IDbObject table)
            => string.IsNullOrEmpty(table.Schema) || string.Compare(table.Schema, "dbo", true) == 0;
    }
}
