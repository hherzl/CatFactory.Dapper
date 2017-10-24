using System.Collections.Generic;
using System.IO;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DapperProjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static DapperProjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

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

        public static string GetEntityLayerDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.EntityLayer);

        public static string GetDataLayerDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer);

        public static string GetDataLayerContractsDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static string GetEntityLayerNamespace(this DapperProject project)
            => string.Format("{0}.{1}", namingConvention.GetClassName(project.Name), project.Namespaces.EntityLayer);

        public static IEnumerable<Column> GetInsertColumns(this DapperProject project, ITable table)
        {
            foreach (var column in table.Columns)
            {
                if (project.Settings.InsertExclusions.Contains(column.Name))
                {
                    continue;
                }

                if (table.Identity != null && table.Identity.Name == column.Name)
                {
                    continue;
                }

                yield return column;
            }
        }

        public static IEnumerable<Column> GetUpdateColumns(this DapperProject project, ITable table)
        {
            foreach (var column in table.GetColumnsWithOutPrimaryKey())
            {
                if (project.Settings.UpdateExclusions.Contains(column.Name))
                {
                    continue;
                }

                yield return column;
            }
        }
    }
}
