using System;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DbObjectsExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static DbObjectsExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static String GetFullName(this IDbObject dbObject)
            => String.Format("[{0}].[{1}]", dbObject.Schema, dbObject.Name);

        public static String GetPluralName(this IDbObject dbObject)
            => NamingService.GetPluralName(dbObject.GetEntityName());

        public static String GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => String.Format("Get{0}Async", dbObject.GetPluralName());

        public static String GetGetRepositoryMethodName(this IDbObject dbObject)
            => String.Format("Get{0}Async", dbObject.GetSingularName());

        public static String GetGetByUniqueRepositoryMethodName(this ITable dbObject, Unique unique)
            => String.Format("Get{0}By{1}Async", dbObject.GetSingularName(), String.Join("And", unique.Key.Select(item => namingConvention.GetPropertyName(item))));

        public static String GetAddRepositoryMethodName(this ITable dbObject)
            => String.Format("Add{0}Async", dbObject.GetSingularName());

        public static String GetUpdateRepositoryMethodName(this ITable dbObject)
            => String.Format("Update{0}Async", dbObject.GetSingularName());

        public static String GetRemoveRepositoryMethodName(this ITable dbObject)
            => String.Format("Remove{0}Async", dbObject.GetSingularName());

        public static Boolean IsPrimaryKeyGuid(this ITable table)
            => table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1 && table.Columns[0].Type == "uniqueidentifier" ? true : false;
    }
}
