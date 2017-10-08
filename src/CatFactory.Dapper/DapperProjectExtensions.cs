using System;
using System.Collections.Generic;
using System.IO;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper
{
    public static class DapperProjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static DapperProjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static String GetEntityLayerNamespace(this Project project)
        => namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, (project as DapperProject).Namespaces.EntityLayer));

        public static String GetInterfaceRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetInterfaceName(String.Format("{0}Repository", projectFeature.Name));

        public static String GetClassRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetClassName(String.Format("{0}Repository", projectFeature.Name));

        public static DapperProject GetDapperProject(this ProjectFeature projectFeature)
            => projectFeature.Project as DapperProject;

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

        public static IEnumerable<Column> GetInsertColumns(this DapperProject project, ITable table)
        {
            foreach (var column in table.Columns)
            {
                if (table.Identity != null && table.Identity.Name == column.Name)
                {
                    continue;
                }

                if (project.Settings.Exclusions.Contains(column.Name))
                {
                    continue;
                }

                yield return column;
            }
        }

        public static IEnumerable<Column> GetUpdateColumns(this DapperProject project, ITable table)
        {
            foreach (var column in table.Columns)
            {
                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                {
                    continue;
                }

                if (project.Settings.Exclusions.Contains(column.Name))
                {
                    continue;
                }

                yield return column;
            }
        }
    }
}
