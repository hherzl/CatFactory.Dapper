using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class NamingExtensions
    {
        public static ICodeNamingConvention namingConvention;
        public static INamingService namingService;

        static NamingExtensions()
        {
            namingConvention = new DotNetNamingConvention();
            namingService = new NamingService();
        }

        public static string GetInterfaceRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => namingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => namingConvention.GetClassName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetEntityName(this IDbObject dbObject)
            => string.Format("{0}", namingConvention.GetClassName(dbObject.Name));

        public static string GetSingularName(this IDbObject dbObject)
            => namingService.Singularize(dbObject.GetEntityName());

        public static string GetFullName(this IDbObject dbObject)
            => string.Format("[{0}].[{1}]", dbObject.Schema, dbObject.Name);

        public static string GetPluralName(this IDbObject dbObject)
            => namingService.Pluralize(dbObject.GetEntityName());

        public static string GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetPluralName());

        public static string GetGetRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetEntityName());

        public static string GetGetByUniqueRepositoryMethodName(this ITable dbObject, Unique unique)
            => string.Format("Get{0}By{1}Async", dbObject.GetEntityName(), string.Join("And", unique.Key.Select(item => namingConvention.GetPropertyName(item))));

        public static string GetAddRepositoryMethodName(this ITable dbObject)
            => string.Format("Add{0}Async", dbObject.GetEntityName());

        public static string GetUpdateRepositoryMethodName(this ITable dbObject)
            => string.Format("Update{0}Async", dbObject.GetEntityName());

        public static string GetDeleteRepositoryMethodName(this ITable dbObject)
            => string.Format("Remove{0}Async", dbObject.GetEntityName());

        public static string GetEntityLayerNamespace(this DapperProject project)
            => string.Format("{0}.{1}", namingConvention.GetClassName(project.Name), project.Namespaces.EntityLayer);

        public static string GetEntityLayerNamespace(this DapperProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.Name, project.Namespaces.EntityLayer, ns);

        public static string GetDataLayerNamespace(this DapperProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer);

        public static string GetDataLayerContractsNamespace(this DapperProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this DapperProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this DapperProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Repositories);
    }
}
