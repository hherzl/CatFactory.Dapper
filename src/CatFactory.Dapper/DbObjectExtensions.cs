using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DbObjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static DbObjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetEntityName(this IDbObject dbObject)
            => string.Format("{0}", namingConvention.GetClassName(dbObject.Name));

        public static string GetSingularName(this IDbObject dbObject)
            => NamingService.GetSingularName(dbObject.GetEntityName());

        public static string GetFullName(this IDbObject dbObject)
            => string.Format("[{0}].[{1}]", dbObject.Schema, dbObject.Name);

        public static string GetPluralName(this IDbObject dbObject)
            => NamingService.GetPluralName(dbObject.GetEntityName());

        public static string GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetPluralName());

        public static string GetGetRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetSingularName());

        public static string GetGetByUniqueRepositoryMethodName(this ITable dbObject, Unique unique)
            => string.Format("Get{0}By{1}Async", dbObject.GetSingularName(), string.Join("And", unique.Key.Select(item => namingConvention.GetPropertyName(item))));

        public static string GetAddRepositoryMethodName(this ITable dbObject)
            => string.Format("Add{0}Async", dbObject.GetSingularName());

        public static string GetUpdateRepositoryMethodName(this ITable dbObject)
            => string.Format("Update{0}Async", dbObject.GetSingularName());

        public static string GetDeleteRepositoryMethodName(this ITable dbObject)
            => string.Format("Remove{0}Async", dbObject.GetSingularName());

        public static bool IsPrimaryKeyGuid(this ITable table)
            => table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1 && table.Columns[0].Type == "uniqueidentifier" ? true : false;

        public static bool HasDefaultSchema(this IDbObject table)
            => string.IsNullOrEmpty(table.Schema) || string.Compare(table.Schema, "dbo", true) == 0;
    }
}
