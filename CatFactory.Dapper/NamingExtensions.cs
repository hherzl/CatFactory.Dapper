using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.Dapper
{
    public static class NamingExtensions
    {
        public static ICodeNamingConvention codeNamingConvention;
        public static INamingService namingService;

        static NamingExtensions()
        {
            codeNamingConvention = new DotNetNamingConvention();
            namingService = new NamingService();
        }
        
        public static string GetFullName(this Database database, IDbObject dbObject)
            => database.NamingConvention.GetObjectName(dbObject.Schema, dbObject.Name);

        public static string GetColumnName(this Database database, ITable table, Column column)
            => database.NamingConvention.GetObjectName(table.Schema, table.Name, column.Name);

        public static string GetColumnName(this Database database, IView view, Column column)
            => database.NamingConvention.GetObjectName(view.Schema, view.Name, column.Name);

        public static string GetColumnName(this Database database, TableFunction tableFunction, Column column)
            => database.NamingConvention.GetObjectName(tableFunction.Schema, tableFunction.Name, column.Name);

        public static string GetColumnName(this Database database, Column column)
            => database.NamingConvention.GetObjectName(column.Name);

        public static string GetParameterName(this Column column)
            => codeNamingConvention.GetParameterName(column.Name);

        public static string GetParameterName(this Database database, Column column)
            => database.NamingConvention.GetParameterName(column.Name);

        public static string GetParameterName(this Database database, Parameter param)
            => database.NamingConvention.GetParameterName(param.Name);

        public static string GetInterfaceRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => codeNamingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => codeNamingConvention.GetClassName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetEntityName(this IDbObject dbObject)
            => string.Format("{0}", codeNamingConvention.GetClassName(dbObject.Name));

        public static string GetSingularName(this IDbObject dbObject)
            => namingService.Singularize(dbObject.GetEntityName());

        public static string GetPluralName(this IDbObject dbObject)
            => namingService.Pluralize(dbObject.GetEntityName());

        public static string GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetPluralName());

        public static string GetGetRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetEntityName());

        public static string GetGetByUniqueRepositoryMethodName(this ITable dbObject, Unique unique)
            => string.Format("Get{0}By{1}Async", dbObject.GetEntityName(), string.Join("And", unique.Key.Select(item => codeNamingConvention.GetPropertyName(item))));

        public static string GetAddRepositoryMethodName(this ITable dbObject)
            => string.Format("Add{0}Async", dbObject.GetEntityName());

        public static string GetUpdateRepositoryMethodName(this ITable dbObject)
            => string.Format("Update{0}Async", dbObject.GetEntityName());

        public static string GetDeleteRepositoryMethodName(this ITable dbObject)
            => string.Format("Remove{0}Async", dbObject.GetEntityName());

        public static string GetEntityLayerNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(project.Name, project.Namespaces.EntityLayer);

        public static string GetEntityLayerNamespace(this DapperProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : codeNamingConvention.GetNamespace(project.Name, project.Namespaces.EntityLayer, ns);

        public static string GetDataLayerNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(project.Name, project.Namespaces.DataLayer);

        public static string GetDataLayerContractsNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(project.Name, project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(project.Name, project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(project.Name, project.Namespaces.DataLayer, project.Namespaces.Repositories);
    }
}
