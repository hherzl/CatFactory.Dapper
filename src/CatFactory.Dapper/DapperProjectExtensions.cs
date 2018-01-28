using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DapperProjectExtensions
    {
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

        public static IEnumerable<Column> GetInsertColumns(this DapperProject project, ITable table)
        {
            foreach (var column in table.Columns)
            {
                if (project.Settings.InsertExclusions.Contains(column.Name))
                    continue;

                if (table.Identity != null && table.Identity.Name == column.Name)
                    continue;

                yield return column;
            }
        }

        public static IEnumerable<Column> GetUpdateColumns(this DapperProject project, ITable table)
        {
            foreach (var column in table.GetColumnsWithNoPrimaryKey())
            {
                if (project.Settings.UpdateExclusions.Contains(column.Name))
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

        public static DapperProject Select(this DapperProject project, string pattern, Action<DapperProjectSettings> action = null)
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
                        InsertExclusions = globalSettings.InsertExclusions,
                        UpdateExclusions = globalSettings.UpdateExclusions,
                    }
                };

                project.Selections.Add(selection);
            }

            action?.Invoke(selection.Settings);

            return project;
        }
    }
}
