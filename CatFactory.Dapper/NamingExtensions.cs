using System.Linq;
using System.Text.RegularExpressions;
using CatFactory.CodeFactory;
using CatFactory.Mapping;
using CatFactory.NetCore;

namespace CatFactory.Dapper
{
    public static class NamingExtensions
    {
        public static ICodeNamingConvention codeNamingConvention;
        public static IDatabaseNamingConvention databaseNamingConvention;
        public static INamingService namingService;

        static NamingExtensions()
        {
            codeNamingConvention = new DotNetNamingConvention();
            databaseNamingConvention = new SqlServerDatabaseNamingConvention();
            namingService = new NamingService();
        }

        public static string GetPropertyName(this Column column)
        {
            var name = column.Name;

            foreach (var item in DotNetNamingConvention.invalidChars)
                name = name.Replace(item, '_');

            return codeNamingConvention.GetPropertyName(name);
        }

        public static string GetParameterName(this Column column)
            => codeNamingConvention.GetParameterName(column.Name);

        public static string GetPropertyNameHack(this ITable table, Column column)
        {
            var propertyName = column.HasSameNameEnclosingType(table) ? column.GetNameForEnclosing() : column.GetPropertyName();

            var regex = new Regex(@"^[0-9]+$");

            if (regex.IsMatch(propertyName))
                propertyName = string.Format("V{0}", propertyName);

            return propertyName;
        }

        public static string GetPropertyNameHack(this IView view, Column column)
        {
            var propertyName = column.HasSameNameEnclosingType(view) ? column.GetNameForEnclosing() : column.GetPropertyName();

            var regex = new Regex(@"^[0-9]+$");

            if (regex.IsMatch(propertyName))
                propertyName = string.Format("V{0}", propertyName);

            return propertyName;
        }

        public static string GetColumnName(this ITable table, Column column)
            => databaseNamingConvention.GetObjectName(table.Schema, table.Name, column.Name);

        public static string GetColumnName(this IView view, Column column)
            => databaseNamingConvention.GetObjectName(view.Schema, view.Name, column.Name);

        public static string GetColumnName(this TableFunction tableFunction, Column column)
            => databaseNamingConvention.GetObjectName(tableFunction.Schema, tableFunction.Name, column.Name);

        public static string GetColumnName(this Column column)
            => databaseNamingConvention.GetObjectName(column.Name);

        public static string GetSqlServerParameterName(this Column column)
            => databaseNamingConvention.GetParameterName(column.Name);

        public static string GetSqlServerParameterName(this Parameter param)
            => databaseNamingConvention.GetParameterName(param.Name);

        public static bool HasSameNameEnclosingType(this Column column, ITable table)
            => column.Name == table.Name;

        public static bool HasSameNameEnclosingType(this Column column, IView view)
            => column.Name == view.Name;

        public static string GetNameForEnclosing(this Column column)
            => string.Format("{0}1", column.Name);

        public static string GetInterfaceRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => codeNamingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => codeNamingConvention.GetClassName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetEntityName(this IDbObject dbObject)
            => string.Format("{0}", codeNamingConvention.GetClassName(dbObject.Name));

        public static string GetFullName(this IDbObject dbObject)
            => databaseNamingConvention.GetObjectName(dbObject.Schema, dbObject.Name);

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
            => codeNamingConvention.GetNamespace(codeNamingConvention.GetClassName(project.Name), project.Namespaces.EntityLayer);

        public static string GetEntityLayerNamespace(this DapperProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : codeNamingConvention.GetNamespace(project.Name, project.Namespaces.EntityLayer, ns);

        public static string GetDataLayerNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(codeNamingConvention.GetClassName(project.Name), project.Namespaces.DataLayer);

        public static string GetDataLayerContractsNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(codeNamingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(codeNamingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this DapperProject project)
            => codeNamingConvention.GetNamespace(codeNamingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Repositories);
    }
}
