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
        public static ICodeNamingConvention CodeNamingConvention;
        public static INamingService NamingService;

        static NamingExtensions()
        {
            CodeNamingConvention = new DotNetNamingConvention();
            NamingService = new NamingService();
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

        public static string GetPropertyName(this Column column)
            => CodeNamingConvention.GetPropertyName(column.Name);

        public static string GetParameterName(this Column column)
            => CodeNamingConvention.GetParameterName(column.Name);

        public static string GetParameterName(this Database database, Column column)
            => database.NamingConvention.GetParameterName(column.Name);

        public static string GetParameterName(this Database database, Parameter param)
            => database.NamingConvention.GetParameterName(param.Name);

        //public static string GetInterfaceRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
        //    => CodeNamingConvention.GetInterfaceName(string.Format("{0}Repository", projectFeature.Name));

        //public static string GetClassRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
        //    => CodeNamingConvention.GetClassName(string.Format("{0}Repository", projectFeature.Name));

        public static string GetInterfaceRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
        => string.Format("{0}Repository", CodeNamingConvention.GetInterfaceName(projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => string.Format("{0}Repository", CodeNamingConvention.GetClassName(projectFeature.Name));

        public static string GetEntityName(this IDbObject dbObject)
            => string.Format("{0}", CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetSingularName(this IDbObject dbObject)
            => NamingService.Singularize(dbObject.GetEntityName());

        public static string GetPluralName(this IDbObject dbObject)
            => NamingService.Pluralize(dbObject.GetEntityName());

        public static string GetResultName(this ITableFunction tableFunction)
            => string.Format("{0}Result", CodeNamingConvention.GetClassName(tableFunction.Name));

        public static string GetResultName(this StoredProcedure storedProcedure)
            => string.Format("{0}Result", CodeNamingConvention.GetClassName(storedProcedure.Name));

        public static string GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetPluralName());

        public static string GetGetRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetEntityName());

        public static string GetGetByUniqueRepositoryMethodName(this ITable dbObject, Unique unique)
            => string.Format("Get{0}By{1}Async", dbObject.GetEntityName(), string.Join("And", unique.Key.Select(item => CodeNamingConvention.GetPropertyName(item))));

        public static string GetAddRepositoryMethodName(this ITable dbObject)
            => string.Format("Add{0}Async", dbObject.GetEntityName());

        public static string GetUpdateRepositoryMethodName(this ITable dbObject)
            => string.Format("Update{0}Async", dbObject.GetEntityName());

        public static string GetDeleteRepositoryMethodName(this ITable dbObject)
            => string.Format("Remove{0}Async", dbObject.GetEntityName());

        public static string GetEntityLayerNamespace(this DapperProject project)
            => CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.EntityLayer);

        public static string GetEntityLayerNamespace(this DapperProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.EntityLayer, CodeNamingConvention.GetClassName(ns));

        public static string GetDataLayerNamespace(this DapperProject project)
            => CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer);

        public static string GetDataLayerContractsNamespace(this DapperProject project)
            => CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this DapperProject project)
            => CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this DapperProject project)
            => CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Repositories);
    }
}
