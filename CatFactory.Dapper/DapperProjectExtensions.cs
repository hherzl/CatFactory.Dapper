using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.Dapper
{
    public static class DapperProjectExtensions
    {
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

        public static string GetPropertyName(this DapperProject project, Column column)
            => project.CodeNamingConvention.GetPropertyName(column.Name);

        public static string GetParameterName(this DapperProject project, Column column)
            => project.CodeNamingConvention.GetParameterName(column.Name);

        public static string GetParameterName(this Database database, Column column)
            => database.NamingConvention.GetParameterName(column.Name);

        public static string GetParameterName(this Database database, Parameter param)
            => database.NamingConvention.GetParameterName(param.Name);

        public static string GetInterfaceRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
            => string.Format("{0}Repository", projectFeature.Project.CodeNamingConvention.GetInterfaceName(projectFeature.Name));

        public static string GetClassRepositoryName(this ProjectFeature<DapperProjectSettings> projectFeature)
           => string.Format("{0}Repository", projectFeature.Project.CodeNamingConvention.GetClassName(projectFeature.Name));

        public static string GetEntityName(this DapperProject project, IDbObject dbObject)
            => string.Format("{0}", project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetResultName(this DapperProject project, ITableFunction tableFunction)
            => string.Format("{0}Result", project.CodeNamingConvention.GetClassName(tableFunction.Name));

        public static string GetResultName(this DapperProject project, StoredProcedure storedProcedure)
            => string.Format("{0}Result", project.CodeNamingConvention.GetClassName(storedProcedure.Name));

        public static string GetSingularName(this DapperProject project, IDbObject dbObject)
            => project.NamingService.Singularize(project.GetEntityName(dbObject));

        public static string GetPluralName(this DapperProject project, IDbObject dbObject)
            => project.NamingService.Pluralize(project.GetEntityName(dbObject));

        public static string GetGetAllRepositoryMethodName(this DapperProject project, IDbObject dbObject)
            => string.Format("Get{0}Async", project.GetPluralName(dbObject));

        public static string GetGetRepositoryMethodName(this DapperProject project, IDbObject dbObject)
            => string.Format("Get{0}Async", project.GetEntityName(dbObject));

        public static string GetGetByUniqueRepositoryMethodName(this DapperProject project, ITable dbObject, Unique unique)
            => string.Format("Get{0}By{1}Async", project.GetEntityName(dbObject), string.Join("And", unique.Key.Select(item => project.CodeNamingConvention.GetPropertyName(item))));

        public static string GetAddRepositoryMethodName(this DapperProject project, ITable dbObject)
            => string.Format("Add{0}Async", project.GetEntityName(dbObject));

        public static string GetUpdateRepositoryMethodName(this DapperProject project, ITable dbObject)
            => string.Format("Update{0}Async", project.GetEntityName(dbObject));

        public static string GetDeleteRepositoryMethodName(this DapperProject project, ITable dbObject)
            => string.Format("Remove{0}Async", project.GetEntityName(dbObject));

        public static string GetEntityLayerNamespace(this DapperProject project)
            => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.EntityLayer);

        public static string GetEntityLayerNamespace(this DapperProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.EntityLayer, project.CodeNamingConvention.GetClassName(ns));

        public static string GetDataLayerNamespace(this DapperProject project)
            => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer);

        public static string GetDataLayerContractsNamespace(this DapperProject project)
            => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this DapperProject project)
            => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this DapperProject project)
            => project.CodeNamingConvention.GetNamespace(project.Name, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Repositories);

        public static string GetEntityLayerDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.EntityLayer);

        public static string GetEntityLayerDirectory(this DapperProject project, string schema)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.EntityLayer, project.CodeNamingConvention.GetClassName(schema));

        public static string GetDataLayerDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer);

        public static string GetDataLayerContractsDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.ProjectNamespaces.DataLayer, project.ProjectNamespaces.Repositories);

        public static IEnumerable<Column> GetInsertColumns(this DapperProject project, ITable table)
        {
            var selection = project.GetSelection(table);

            foreach (var column in table.Columns)
            {
                if (table.Identity?.Name == column.Name || selection.Settings.InsertExclusions.Contains(column.Name))
                    continue;

                yield return column;
            }
        }

        public static IEnumerable<Column> GetUpdateColumns(this DapperProject project, ITable table)
        {
            var selection = project.GetSelection(table);

            foreach (var column in table.GetColumnsWithNoPrimaryKey())
            {
                if (selection.Settings.UpdateExclusions.Contains(column.Name))
                    continue;

                yield return column;
            }
        }

        public static DapperProject GlobalSelection(this DapperProject project, Action<DapperProjectSettings> action = null)
        {
            var settings = new DapperProjectSettings();

            var selection = project.Selections.FirstOrDefault(item => item.IsGlobal);

            if (selection == null)
            {
                selection = new ProjectSelection<DapperProjectSettings>
                {
                    Pattern = ProjectSelection<DapperProjectSettings>.GlobalPattern,
                    Settings = settings
                };

                project.Selections.Add(selection);
            }
            else
            {
                settings = selection.Settings;
            }

            action?.Invoke(settings);

            return project;
        }

        public static ProjectSelection<DapperProjectSettings> GlobalSelection(this DapperProject project)
            => project.Selections.FirstOrDefault(item => item.IsGlobal);

        public static DapperProject Selection(this DapperProject project, string pattern, Action<DapperProjectSettings> action = null)
        {
            var selection = project.Selections.FirstOrDefault(item => item.Pattern == pattern);

            if (selection == null)
            {
                var globalSettings = project.GlobalSelection().Settings;

                selection = new ProjectSelection<DapperProjectSettings>
                {
                    Pattern = pattern,
                    Settings = new DapperProjectSettings
                    {
                        ForceOverwrite = globalSettings.ForceOverwrite,
                        SimplifyDataTypes = globalSettings.SimplifyDataTypes,
                        UseAutomaticPropertiesForEntities = globalSettings.UseAutomaticPropertiesForEntities,
                        EnableDataBindings = globalSettings.EnableDataBindings,
                        UseStringBuilderForQueries = globalSettings.UseStringBuilderForQueries,
                        InsertExclusions = globalSettings.InsertExclusions.Select(item => item).ToList(),
                        UpdateExclusions = globalSettings.UpdateExclusions.Select(item => item).ToList(),
                        AddPagingForGetAllOperation = globalSettings.AddPagingForGetAllOperation
                    }
                };

                project.Selections.Add(selection);
            }

            action?.Invoke(selection.Settings);

            return project;
        }

        [Obsolete("Use Selection method.")]
        public static DapperProject Select(this DapperProject project, string pattern, Action<DapperProjectSettings> action = null)
            => project.Select(pattern, action);
    }
}
