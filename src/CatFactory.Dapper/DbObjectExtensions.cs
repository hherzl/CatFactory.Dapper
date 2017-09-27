using System;
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
            namingConvention = new DotNetNamingConvention() as ICodeNamingConvention;
        }

        public static String GetEntityName(this IDbObject dbObject)
            => String.Format("{0}", namingConvention.GetClassName(dbObject.Name));

        public static String GetSingularName(this IDbObject dbObject)
            => NamingService.GetSingularName(dbObject.GetEntityName());
    }
}
