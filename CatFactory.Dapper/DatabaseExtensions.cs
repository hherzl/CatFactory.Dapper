using System.Linq;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DatabaseExtensions
    {
        public static bool HasDefaultSchema(this Database database, IDbObject dbObj)
            => string.IsNullOrEmpty(dbObj.Schema) || string.Compare(dbObj.Schema, database.DefaultSchema, true) == 0;

        public static string ResolveType(this Database database, Column column)
        {
            var map = database.Mappings.FirstOrDefault(item => item.DatabaseType == column.Type);

            if (map == null || map.GetClrType() == null)
                return "object";

            return map.AllowClrNullable ? string.Format("{0}?", map.GetClrType().Name) : map.GetClrType().Name;
        }

        public static string ResolveDbType(this Database database, Column column)
        {
            var map = database.Mappings.FirstOrDefault(item => item.DatabaseType == column.Type);

            if (map == null || map.GetClrType() == null)
                return "object";

            return string.Format("DbType.{0}", map.DbTypeEnum);
        }
    }
}
