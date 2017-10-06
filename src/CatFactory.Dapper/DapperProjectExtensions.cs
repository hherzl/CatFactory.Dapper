using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper
{
    public static class DapperProjectExtensions
    {
        public static Boolean IsDecimal(this Column column)
        {
            switch (column.Type)
            {
                case "decimal":
                    return true;

                default:
                    return false;
            }
        }

        public static Boolean IsDouble(this Column column)
        {
            switch (column.Type)
            {
                case "float":
                    return true;

                default:
                    return false;
            }
        }

        public static Boolean IsSingle(this Column column)
        {
            switch (column.Type)
            {
                case "real":
                    return true;

                default:
                    return false;
            }
        }

        public static Boolean IsString(this Column column)
        {
            switch (column.Type)
            {
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    return true;

                default:
                    return false;
            }
        }

        public static Boolean IsPrimaryKeyGuid(this Table table)
            => table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1 && table.Columns[0].Type == "uniqueidentifier" ? true : false;
        public static String GetGetByUniqueRepositoryMethodName(this IDbObject dbObject, Unique unique)
            => String.Format("Get{0}By{1}Async", dbObject.GetSingularName(), String.Join("And", unique.Key.Select(item => namingConvention.GetPropertyName(item))));

        public static String GetInterfaceRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetInterfaceName(String.Format("{0}Repository", projectFeature.Name));

        public static String GetClassRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetClassName(String.Format("{0}Repository", projectFeature.Name));

        public static DapperProject GetDapperProject(this ProjectFeature projectFeature)
            => projectFeature.Project as DapperProject;

        private static ICodeNamingConvention namingConvention;

        static DapperProjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static String GetPluralName(this IDbObject dbObject)
            => NamingService.GetPluralName(dbObject.GetEntityName());

        public static String GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => String.Format("Get{0}Async", dbObject.GetPluralName());

        public static String GetGetRepositoryMethodName(this IDbObject dbObject)
            => String.Format("Get{0}Async", dbObject.GetSingularName());

        public static String GetAddRepositoryMethodName(this ITable dbObject)
            => String.Format("Add{0}Async", dbObject.GetSingularName());

        public static String GetUpdateRepositoryMethodName(this ITable dbObject)
            => String.Format("Update{0}Async", dbObject.GetSingularName());

        public static String GetRemoveRepositoryMethodName(this ITable dbObject)
            => String.Format("Remove{0}Async", dbObject.GetSingularName());

        public static String GetEntityLayerNamespace(this Project project)
        => namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, (project as DapperProject).Namespaces.EntityLayer));

        public static String GetEntityLayerNamespace(this DapperProject project, String ns)
            => String.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : String.Join(".", project.Name, project.Namespaces.EntityLayer, ns);

        public static String GetDataLayerNamespace(this DapperProject project)
            => namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, project.Namespaces.DataLayer));

        public static String GetDataLayerContractsNamespace(this DapperProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Contracts));

        public static String GetDataLayerDataContractsNamespace(this DapperProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.DataContracts));

        public static String GetDataLayerRepositoriesNamespace(this DapperProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Repositories));

        public static String GetEntityLayerDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.EntityLayer);

        public static String GetDataLayerDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer);

        public static String GetDataLayerContractsDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static String GetDataLayerDataContractsDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static String GetDataLayerRepositoriesDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static CSharpClassDefinition GetAppSettingsClassDefinition(this DapperProject project)
        {
            return new CSharpClassDefinition
            {
                Namespace = project.GetDataLayerNamespace(),
                Namespaces = new List<String> { "System" },
                Name = "AppSettings",
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition("String", "ConnectionString")
                }
            };
        }
    }
}
