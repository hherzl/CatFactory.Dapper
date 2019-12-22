using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Actions;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static partial class RepositoryClassBuilder
    {
        public static RepositoryClassDefinition GetRepositoryClassDefinition(this ProjectFeature<DapperProjectSettings> projectFeature)
        {
            var dapperProject = projectFeature.GetDapperProject();

            var definition = new RepositoryClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Data",
                    "System.Data.SqlClient",
                    "System.Linq",
                    "System.Text",
                    "System.Threading.Tasks",
                    "Dapper"
                },
                Namespace = dapperProject.GetDataLayerRepositoriesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = projectFeature.GetClassRepositoryName(),
                BaseClass = "Repository",
                Implements =
                {
                    projectFeature.GetInterfaceRepositoryName()
                },
                Constructors =
                {
                    new ClassConstructorDefinition(new ParameterDefinition("IDbConnection", "connection"))
                    {
                        AccessModifier = AccessModifier.Public,
                        Invocation = "base(connection)"
                    }
                }
            };

            foreach (var table in dapperProject.Database.Tables)
            {
                if (projectFeature.Project.Database.HasDefaultSchema(table))
                    definition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetEntityLayerNamespace());
                else
                    definition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetEntityLayerNamespace(table.Schema));

                definition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetDataLayerContractsNamespace());
            }

            var dbos = projectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();

            var db = dapperProject.Database;

            var tables = db.Tables.Where(item => dbos.Contains(item.FullName)).ToList();

            foreach (var table in tables)
            {
                var selection = dapperProject.GetSelection(table);

                if (selection.Settings.Actions.Any(item => item is ReadAllAction))
                    definition.Methods.Add(GetGetAllMethod(projectFeature, table));

                if (selection.Settings.Actions.Any(item => item is ReadByKeyAction))
                {
                    if (table.PrimaryKey != null)
                        definition.Methods.Add(GetGetMethod(projectFeature, table));
                }

                foreach (var unique in table.Uniques)
                {
                    if (selection.Settings.Actions.Any(item => item is ReadByUniqueAction))
                        definition.Methods.Add(GetByUniqueMethod(projectFeature, table, unique));
                }

                if (selection.Settings.Actions.Any(item => item is AddEntityAction))
                    definition.Methods.Add(GetAddMethod(projectFeature, table));

                if (table.PrimaryKey != null)
                {
                    if (selection.Settings.Actions.Any(item => item is UpdateEntityAction))
                        definition.Methods.Add(GetUpdateMethod(projectFeature, table));

                    if (selection.Settings.Actions.Any(item => item is RemoveEntityAction))
                        definition.Methods.Add(GetRemoveMethod(projectFeature, table));
                }
            }

            var views = db.Views.Where(item => dbos.Contains(item.FullName)).ToList();

            foreach (var view in views)
            {
                var selection = dapperProject.GetSelection(view);

                if (selection.Settings.Actions.Any(item => item is ReadAllAction))
                    definition.Methods.Add(GetGetAllMethod(projectFeature, view));
            }

            var scalarFunctions = db.GetScalarFunctions().Where(item => dbos.Contains(item.FullName)).ToList();

            foreach (var scalarFunction in scalarFunctions)
            {
                definition.Methods.Add(GetGetAllMethod(projectFeature, scalarFunction));
            }

            var tableFunctions = db.GetTableFunctions().Where(item => dbos.Contains(item.FullName)).ToList();

            foreach (var tableFunction in tableFunctions)
            {
                definition.Methods.Add(GetGetAllMethod(projectFeature, tableFunction));
            }

            var storedProcedures = db.GetStoredProcedures().Where(item => dbos.Contains(item.FullName)).ToList();

            foreach (var storedProcedure in storedProcedures)
            {
                definition.Methods.Add(GetGetAllMethod(projectFeature, storedProcedure));
            }

            return definition;
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(table);
            var db = projectFeature.Project.Database;
            var filters = table.ForeignKeys.Count > 0 || selection.Settings.AddPagingForGetAllOperation ? true : false;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    var columnName = db.GetColumnName(column);
                    var propertyName = project.GetPropertyName(column);

                    if (column.Name != propertyName)
                        lines.Add(new CodeLine("query.Append(\"  {0} {1}{2} \");", columnName, propertyName, i < table.Columns.Count - 1 ? "," : string.Empty));
                    else
                        lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", columnName, i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\" from \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(table)));

                if (filters)
                {
                    lines.Add(new CodeLine("query.Append(\" where \");"));

                    for (var i = 0; i < table.ForeignKeys.Count; i++)
                    {
                        var foreignKey = table.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                            lines.Add(new CodeLine("query.Append(\"  ({0} is null or {1} = {0}) {2} \");", db.GetParameterName(column), db.GetColumnName(column), i < table.ForeignKeys.Count - 1 ? "and" : string.Empty));
                        }
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine("query.Append(\" order by \");"));

                    lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetColumnName(table.Columns.First())));

                    lines.Add(new CodeLine("query.Append(\" offset @pageSize * (@pageNumber - 1) rows \");"));
                    lines.Add(new CodeLine("query.Append(\" fetch next @pageSize rows only \");"));
                }
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));

                lines.Add(new CodeLine("var query = @\""));
                lines.Add(new CodeLine(" select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    var columnName = db.GetColumnName(column);
                    var propertyName = project.GetPropertyName(column);

                    if (column.Name != propertyName)
                        lines.Add(new CodeLine("  {0}{1}{2} ", db.GetColumnName(column), propertyName, i < table.Columns.Count - 1 ? "," : string.Empty));
                    else
                        lines.Add(new CodeLine("  {0}{1} ", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(" from "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(table)));

                if (filters && table.ForeignKeys.Count > 0)
                {
                    lines.Add(new CodeLine(" where "));

                    for (var i = 0; i < table.ForeignKeys.Count; i++)
                    {
                        var foreignKey = table.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                            lines.Add(new CodeLine("  ({0} is null or {1} = {0}) {2} ", db.GetParameterName(column), db.GetColumnName(column), i < table.ForeignKeys.Count - 1 ? "and" : string.Empty));
                        }
                        else
                        {
                            // todo: add foreign key with multiple columns
                        }
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine(" order by "));

                    lines.Add(new CodeLine("  {0} ", db.GetColumnName(table.Columns.First())));

                    lines.Add(new CodeLine(" offset @pageSize * (@pageNumber - 1) rows "));
                    lines.Add(new CodeLine(" fetch next @pageSize rows only "));
                }

                lines.Add(new CodeLine(" \";"));
            }

            lines.Add(new CodeLine());

            if (filters)
            {
                lines.Add(new CommentLine(" Create parameters collection"));
                lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Add parameters to collection"));

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine("parameters.Add(\"@pageSize\", pageSize);"));
                    lines.Add(new CodeLine("parameters.Add(\"@pageNumber\", pageNumber);"));
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                    lines.Add(new CodeLine("parameters.Add(\"{0}\", {1});", db.GetParameterName(column), project.GetParameterName(column)));
                }

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Retrieve result from database and convert to typed list"));

            if (filters)
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(new CommandDefinition(query.ToString(), parameters));", project.GetEntityName(table)));
            else
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", project.GetEntityName(table)));

            var parameters = new List<ParameterDefinition>();

            if (filters)
            {
                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    parameters.Add(new ParameterDefinition("int", "pageSize") { DefaultValue = "10" });
                    parameters.Add(new ParameterDefinition("int", "pageNumber") { DefaultValue = "1" });
                }

                // todo: Add logic to retrieve multiple columns from foreign key

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var column = (Column)table.GetColumnsFromConstraint(foreignKey).ToList().First();

                    parameters.Add(new ParameterDefinition(db.ResolveDatabaseType(column), project.GetParameterName(column)) { DefaultValue = "null" });
                }
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<IEnumerable<{0}>>", project.GetEntityName(table)),
                Name = project.GetGetAllRepositoryMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, IView view)
        {
            var lines = new List<ILine>();

            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(view);
            var db = projectFeature.Project.Database;
            var primaryKeys = db
                .Tables
                .Where(item => item.PrimaryKey != null)
                .Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First())
                .ToList();

            var pksInView = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" select \");"));

                for (var i = 0; i < view.Columns.Count; i++)
                {
                    var column = view.Columns[i];

                    var columnName = db.GetColumnName(column);
                    var propertyName = project.GetPropertyName(column);

                    if (column.Name != propertyName)
                        lines.Add(new CodeLine("query.Append(\"  {0} {1}{2} \");", columnName, propertyName, i < view.Columns.Count - 1 ? "," : string.Empty));
                    else
                        lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", columnName, i < view.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\" from \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(view)));

                if (pksInView.Count > 0)
                {
                    lines.Add(new CodeLine("query.Append(\" where \");"));

                    for (var i = 0; i < pksInView.Count; i++)
                    {
                        var pk = pksInView[i];

                        lines.Add(new CodeLine("query.Append(\"  ({0} is null or {1} = {0}) {2} \");", db.GetParameterName(pk), db.GetColumnName(pk), i < primaryKeys.Count - 1 ? "and" : string.Empty));
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine("query.Append(\" order by \");"));

                    if (primaryKeys.Count == 0)
                    {
                        lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetColumnName(view.Columns.First())));
                    }
                    else
                    {
                        lines.Add(new CodeLine("query.Append(\"  {0} \");", string.Join(", ", pksInView.Select(item => db.NamingConvention.GetObjectName(item.Name)))));
                    }

                    lines.Add(new CodeLine("query.Append(\" offset @pageSize * (@pageNumber - 1) rows \");"));
                    lines.Add(new CodeLine("query.Append(\" fetch next @pageSize rows only \");"));
                }

                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" select "));

                for (var i = 0; i < view.Columns.Count; i++)
                {
                    var column = view.Columns[i];

                    var columnName = db.GetColumnName(column);
                    var propertyName = project.GetPropertyName(column);

                    if (column.Name != propertyName)
                        lines.Add(new CodeLine("  {0}{1}{2} ", db.GetColumnName(column), propertyName, i < view.Columns.Count - 1 ? "," : string.Empty));
                    else
                        lines.Add(new CodeLine("  {0}{1} ", db.GetColumnName(column), i < view.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(" from "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(view)));

                if (pksInView.Count > 0)
                {
                    lines.Add(new CodeLine(" where "));

                    for (var i = 0; i < pksInView.Count; i++)
                    {
                        var pk = pksInView[i];

                        lines.Add(new CodeLine("  ({0} is null or {1} = {0}) {2} ", db.GetParameterName(pk), db.GetColumnName(pk), i < pksInView.Count - 1 ? "and" : string.Empty));
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine(" order by "));

                    if (pksInView.Count == 0)
                        lines.Add(new CodeLine("  {0} ", db.GetColumnName(view.Columns.First())));
                    else
                        lines.Add(new CodeLine("  {0} ", string.Join(", ", pksInView.Select(item => db.NamingConvention.GetObjectName(item.Name)))));

                    lines.Add(new CodeLine(" offset @pageSize * (@pageNumber - 1) rows "));
                    lines.Add(new CodeLine(" fetch next @pageSize rows only "));
                }

                lines.Add(new CodeLine(" \";"));
                lines.Add(new CodeLine());
            }

            if (pksInView.Count > 0)
            {
                lines.Add(new CommentLine(" Create parameters collection"));
                lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Add parameters to collection"));

                foreach (var column in pksInView)
                {
                    lines.Add(new CodeLine("parameters.Add(\"{0}\", {1});", db.GetParameterName(column), project.GetParameterName(column)));
                }

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Retrieve result from database and convert to typed list"));
            lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", project.GetEntityName(view)));

            var parameters = new List<ParameterDefinition>();

            if (selection.Settings.AddPagingForGetAllOperation)
            {
                parameters.Add(new ParameterDefinition("int", "pageSize") { DefaultValue = "10" });
                parameters.Add(new ParameterDefinition("int", "pageNumber") { DefaultValue = "1" });
            }

            foreach (var pk in pksInView)
            {
                parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(pk), projectFeature.Project.CodeNamingConvention.GetParameterName(pk.Name), "null"));
            }

            return new MethodDefinition(AccessModifier.Public, string.Format("Task<IEnumerable<{0}>>", project.GetEntityName(view)), project.GetGetAllRepositoryMethodName(view))
            {
                IsAsync = true,
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(table);
            var db = project.Database;
            var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            var lines = new List<ILine>();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    var columnName = db.GetColumnName(column);
                    var propertyName = project.GetPropertyName(column);

                    if (column.Name != propertyName)
                        lines.Add(new CodeLine("query.Append(\"  {0} {1}{2} \");", columnName, propertyName, i < table.Columns.Count - 1 ? "," : string.Empty));
                    else
                        lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", columnName, i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\" from \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(table)));

                lines.Add(new CodeLine("query.Append(\" where \");"));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine("query.Append(\"  {0} = {1} \");", db.GetColumnName(column), db.GetParameterName(column)));
                    lines.Add(new CodeLine());
                }
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    var columnName = db.GetColumnName(column);
                    var propertyName = project.GetPropertyName(column);

                    if (column.Name == propertyName)
                        lines.Add(new CodeLine("  {0}{1} ", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
                    else
                        lines.Add(new CodeLine("  {0}{1}{2} ", db.GetColumnName(column), propertyName, i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(" from "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(table)));

                lines.Add(new CodeLine(" where "));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine("  {0} = {1} ", db.GetColumnName(column), db.GetParameterName(column)));
                }

                lines.Add(new CodeLine(" \";"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Create parameters collection"));
            lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Add parameters to collection"));

            for (var i = 0; i < key.Count; i++)
            {
                var column = (Column)key[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", project.GetParameterName(column), project.GetPropertyName(table, column)));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Retrieve result from database and convert to entity class"));
            lines.Add(new CodeLine("return await Connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", project.GetEntityName(table)));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<{0}>", project.GetEntityName(table)),
                Name = project.GetGetRepositoryMethodName(table),
                Parameters =
                {
                    new ParameterDefinition(project.GetEntityName(table), "entity")
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetByUniqueMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(table);
            var db = project.Database;
            var key = table.GetColumnsFromConstraint(unique).ToList();

            var lines = new List<ILine>();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    var columnName = db.GetColumnName(column);
                    var propertyName = project.GetPropertyName(column);

                    if (column.Name == propertyName)
                        lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", columnName, i < table.Columns.Count - 1 ? "," : string.Empty));
                    else
                        lines.Add(new CodeLine("query.Append(\"  {0} {1}{2} \");", columnName, propertyName, i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\" from \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(table)));

                lines.Add(new CodeLine("query.Append(\" where \");"));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine("query.Append(\"  {0} = {1} {2} \");", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? "and" : string.Empty));
                }

                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CodeLine("var query = @\" select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine("  {0}{1} ", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(" from "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(table)));

                lines.Add(new CodeLine(" where "));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine("  {0} = {1} {2} ", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? "and" : string.Empty));
                }

                lines.Add(new CodeLine("\";"));

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Create parameters collection"));
            lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Add parameters to collection"));

            for (var i = 0; i < key.Count; i++)
            {
                var column = (Column)key[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", project.Database.GetParameterName(column), project.GetPropertyName(table, column)));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Retrieve result from database and convert to entity class"));

            if (selection.Settings.UseStringBuilderForQueries)
                lines.Add(new CodeLine("return await Connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", project.GetEntityName(table)));
            else
                lines.Add(new CodeLine("return await Connection.QueryFirstOrDefaultAsync<{0}>(query, parameters);", project.GetEntityName(table)));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<{0}>", project.GetEntityName(table)),
                Name = project.GetGetByUniqueRepositoryMethodName(table, unique),
                Parameters =
                {
                    new ParameterDefinition(project.GetEntityName(table), "entity")
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, ScalarFunction scalarFunction)
        {
            var lines = new List<ILine>();

            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(scalarFunction);
            var db = projectFeature.Project.Database;
            var typeToReturn = db.ResolveDatabaseType(scalarFunction.Parameters.FirstOrDefault(item => string.IsNullOrEmpty(item.Name)).Type);
            var scalarFunctionParameters = scalarFunction.Parameters.Where(item => !string.IsNullOrEmpty(item.Name)).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" select \");"));

                var selectParameters = scalarFunctionParameters.Count == 0 ? string.Empty : string.Join(", ", scalarFunctionParameters.Select(item => item.Name));

                lines.Add(new CodeLine("query.Append(\"  {0}({1}) \");", db.GetFullName(scalarFunction), selectParameters));
                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" select "));

                var selectParameters = scalarFunctionParameters.Count == 0 ? string.Empty : string.Join(", ", scalarFunctionParameters.Select(item => item.Name));

                lines.Add(new CodeLine("  {0}({1}) ", db.GetFullName(scalarFunction), selectParameters));
                lines.Add(new CodeLine(" \";"));
                lines.Add(new CodeLine());
            }

            if (scalarFunctionParameters.Count > 0)
            {
                lines.Add(new CommentLine(" Create parameters collection"));
                lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Add parameters to collection"));

                foreach (var parameter in scalarFunctionParameters)
                    lines.Add(new CodeLine("parameters.Add(\"{0}\", {1});", parameter.Name, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Retrieve scalar from database and cast to specific CLR type"));

            if (scalarFunctionParameters.Count == 0)
            {
                if (selection.Settings.UseStringBuilderForQueries)
                    lines.Add(new CodeLine("var scalar = await Connection.ExecuteScalarAsync(query.ToString());"));
                else
                    lines.Add(new CodeLine("var scalar = await Connection.ExecuteScalarAsync(query);"));

                lines.Add(new CodeLine("return ({0})scalar;", typeToReturn));
            }
            else
            {
                if (selection.Settings.UseStringBuilderForQueries)
                    lines.Add(new CodeLine("var scalar = await Connection.ExecuteScalarAsync(query.ToString(), parameters);", typeToReturn));
                else
                    lines.Add(new CodeLine("var scalar = await Connection.ExecuteScalarAsync(query, parameters);", typeToReturn));

                lines.Add(new CodeLine("return ({0})scalar;", typeToReturn));
            }

            var parameters = new List<ParameterDefinition>();

            foreach (var parameter in scalarFunctionParameters)
            {
                parameters.Add(new ParameterDefinition(db.ResolveDatabaseType(parameter.Type), NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<{0}>", typeToReturn),
                Name = project.GetGetAllRepositoryMethodName(scalarFunction),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, TableFunction tableFunction)
        {
            var lines = new List<ILine>();

            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(tableFunction);
            var db = projectFeature.Project.Database;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" select \");"));

                for (var i = 0; i < tableFunction.Columns.Count; i++)
                {
                    var column = tableFunction.Columns[i];

                    lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < tableFunction.Columns.Count - 1 ? "," : string.Empty));
                }

                var selectParameters = tableFunction.Parameters.Count == 0 ? string.Empty : string.Join(", ", tableFunction.Parameters.Select(item => item.Name));

                lines.Add(new CodeLine("query.Append(\" from \");"));
                lines.Add(new CodeLine("query.Append(\"  {0}({1}) \");", db.GetFullName(tableFunction), selectParameters));
                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" select "));

                for (var i = 0; i < tableFunction.Columns.Count; i++)
                {
                    var column = tableFunction.Columns[i];

                    lines.Add(new CodeLine("  {0}{1} ", db.GetColumnName(column), i < tableFunction.Columns.Count - 1 ? "," : string.Empty));
                }

                var selectParameters = tableFunction.Parameters.Count == 0 ? string.Empty : string.Join(", ", tableFunction.Parameters.Select(item => item.Name));

                lines.Add(new CodeLine(" from "));
                lines.Add(new CodeLine("  {0}({1}) ", db.GetFullName(tableFunction), selectParameters));
                lines.Add(new CodeLine(" \";"));
                lines.Add(new CodeLine());
            }

            if (tableFunction.Parameters.Count > 0)
            {
                lines.Add(new CommentLine(" Create parameters collection"));
                lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Add parameters to collection"));

                foreach (var parameter in tableFunction.Parameters)
                    lines.Add(new CodeLine("parameters.Add(\"{0}\", {1});", parameter.Name, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Retrieve result from database and convert to typed list"));

            if (tableFunction.Parameters.Count == 0)
            {
                if (selection.Settings.UseStringBuilderForQueries)
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", project.GetResultName(tableFunction)));
                else
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query);", project.GetResultName(tableFunction)));
            }
            else
            {
                if (selection.Settings.UseStringBuilderForQueries)
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString(), parameters);", project.GetResultName(tableFunction)));
                else
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query, parameters);", project.GetResultName(tableFunction)));
            }

            var parameters = new List<ParameterDefinition>();

            foreach (var parameter in tableFunction.Parameters)
            {
                parameters.Add(new ParameterDefinition(db.ResolveDatabaseType(parameter.Type), NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<IEnumerable<{0}>>", project.GetResultName(tableFunction)),
                Name = project.GetGetAllRepositoryMethodName(tableFunction),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, StoredProcedure storedProcedure)
        {
            var lines = new List<ILine>();

            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(storedProcedure);
            var db = projectFeature.Project.Database;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" exec \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(storedProcedure)));

                var callingParameters = storedProcedure.Parameters.Select(item => item.Name).ToList();

                for (var i = 0; i < callingParameters.Count; i++)
                {
                    var parameter = callingParameters[i];

                    lines.Add(new CodeLine("query.Append(\"   {0}{1} \");", parameter, i < callingParameters.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" exec "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(storedProcedure)));

                var callingParameters = storedProcedure.Parameters.Select(item => item.Name).ToList();

                for (var i = 0; i < callingParameters.Count; i++)
                {
                    var parameter = callingParameters[i];

                    lines.Add(new CodeLine("   {0}{1}", parameter, i < callingParameters.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("\";"));

                lines.Add(new CodeLine());
            }

            if (storedProcedure.Parameters.Count > 0)
            {
                lines.Add(new CommentLine(" Create parameters collection"));
                lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Add parameters to collection"));

                foreach (var parameter in storedProcedure.Parameters)
                    lines.Add(new CodeLine("parameters.Add(\"{0}\", {1});", parameter.Name, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Retrieve result from database and convert to typed list"));

            if (storedProcedure.Parameters.Count == 0)
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", project.GetResultName(storedProcedure)));
            else
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString(), parameters);", project.GetResultName(storedProcedure)));

            var parameters = new List<ParameterDefinition>();

            foreach (var parameter in storedProcedure.Parameters)
            {
                if (projectFeature.Project.Database.HasTypeMappedToClr(parameter))
                {
                    var clrType = projectFeature.Project.Database.GetClrMapForType(parameter);

                    var propertyType = clrType.AllowClrNullable ? string.Format("{0}?", clrType.GetClrType().Name) : clrType.GetClrType().Name;

                    parameters.Add(new ParameterDefinition(propertyType, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));
                }
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = string.Format("Task<IEnumerable<{0}>>", project.GetResultName(storedProcedure)),
                Name = project.GetGetAllRepositoryMethodName(storedProcedure),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetAddMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();
            var project = projectFeature.GetDapperProject();
            var db = project.Database;

            if (table.PrimaryKey != null && db.PrimaryKeyIsGuid(table))
            {
                lines.Add(new CommentLine(" Generate value for Guid property"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", project.GetPropertyName(table, (Column)table.GetColumnsFromConstraint(table.PrimaryKey).First())));
                lines.Add(new CodeLine());
            }

            var insertColumns = projectFeature.GetDapperProject().GetInsertColumns(table).ToList();

            var selection = projectFeature.GetDapperProject().GetSelection(table);

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" insert into \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(table)));
                lines.Add(new CodeLine("query.Append(\"  ( \");"));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine("query.Append(\"   {0}{1} \");", db.GetColumnName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\"  ) \");"));
                lines.Add(new CodeLine("query.Append(\" values \");"));
                lines.Add(new CodeLine("query.Append(\" ( \");"));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", db.GetParameterName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\" ) \");"));

                if (table.Identity != null)
                {
                    var identityColumn = table.GetIdentityColumn();

                    lines.Add(new CodeLine("query.Append(\"  select {0} = @@identity \");", db.GetParameterName(identityColumn)));
                }
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" insert into "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(table)));
                lines.Add(new CodeLine("  ( "));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine("   {0}{1} ", db.GetColumnName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("  ) "));
                lines.Add(new CodeLine(" values "));
                lines.Add(new CodeLine(" ( "));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine("  {0}{1} ", db.GetParameterName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(" ) "));

                if (table.Identity != null)
                {
                    var identityColumn = table.GetIdentityColumn();

                    lines.Add(new CodeLine("  select {0} = @@identity ", db.GetParameterName(identityColumn)));
                }

                lines.Add(new CodeLine(" \";"));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Create parameters collection"));
            lines.Add(new CodeLine("var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Add parameters to collection"));

            if (table.Identity == null)
            {
                for (var i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", project.Database.GetParameterName(column), project.GetPropertyName(table, column)));
                }

                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Execute query in database"));
                lines.Add(new CodeLine("return await Connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));
            }
            else
            {
                for (var i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", project.Database.GetParameterName(column), project.GetPropertyName(table, column)));
                }

                var identityColumn = (Column)table.GetIdentityColumn();

                lines.Add(new CodeLine("parameters.Add(\"{0}\", dbType: {1}, direction: ParameterDirection.Output);", project.Database.GetParameterName(identityColumn), db.ResolveDbType(identityColumn)));

                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Execute query in database"));
                lines.Add(new CodeLine("var affectedRows = await Connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Retrieve value for output parameters"));
                lines.Add(new CodeLine("entity.{0} = parameters.Get<{1}>(\"{2}\");", project.GetPropertyName(table, identityColumn), db.ResolveDatabaseType(identityColumn), project.Database.GetParameterName(identityColumn)));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine("return affectedRows;"));
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<int>",
                Name = project.GetAddRepositoryMethodName(table),
                Parameters =
                {
                    new ParameterDefinition(project.GetEntityName(table), "entity")
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(table);
            var db = project.Database;
            var sets = project.GetUpdateColumns(table).ToList();
            var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" update \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(table)));
                lines.Add(new CodeLine("query.Append(\" set \");"));

                for (var i = 0; i < sets.Count(); i++)
                {
                    var column = sets[i];

                    lines.Add(new CodeLine("query.Append(\"  {0} = {1}{2 } \");", db.GetColumnName(column), db.GetParameterName(column), i < sets.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\" where \");"));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine("query.Append(\"  {0} = {1}{2} \");", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" update "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(table)));
                lines.Add(new CodeLine(" set "));

                for (var i = 0; i < sets.Count(); i++)
                {
                    var column = sets[i];

                    lines.Add(new CodeLine("  {0} = {1}{2} ", db.GetColumnName(column), db.GetParameterName(column), i < sets.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(" where "));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine("  {0} = {1}{2} ", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }

                lines.Add(new CodeLine(" \"; "));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Create parameters collection"));
            lines.Add(new CodeLine("var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Add parameters to collection"));

            for (var i = 0; i < sets.Count; i++)
            {
                var column = sets[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", project.Database.GetParameterName(column), project.GetPropertyName(table, column)));
            }

            for (var i = 0; i < key.Count; i++)
            {
                var column = (Column)key[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", project.Database.GetParameterName(column), project.GetPropertyName(table, column)));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Execute query in database"));
            lines.Add(new CodeLine("return await Connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<int>",
                Name = project.GetUpdateRepositoryMethodName(table),
                Parameters =
                {
                    new ParameterDefinition(project.GetEntityName(table), "entity")
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var project = projectFeature.GetDapperProject();
            var selection = project.GetSelection(table);
            var db = project.Database;
            var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(" Create string builder for query"));
                lines.Add(new CodeLine("var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("query.Append(\" delete from \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(table)));
                lines.Add(new CodeLine("query.Append(\" where \");"));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = (Column)key[i];

                    lines.Add(new CodeLine("query.Append(\"  {0} = {1}{2} \");", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" delete from "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(table)));
                lines.Add(new CodeLine(" where "));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine("  {0} = {1}{2} ", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }

                lines.Add(new CodeLine(" \"; "));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Create parameters collection"));
            lines.Add(new CodeLine("var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Add parameters to collection"));

            var columns = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            for (var i = 0; i < columns.Count(); i++)
            {
                var column = (Column)columns[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", project.Database.GetParameterName(column), project.GetPropertyName(table, column)));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Execute query in database"));
            lines.Add(new CodeLine("return await Connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<int>",
                Name = project.GetDeleteRepositoryMethodName(table),
                Parameters =
                {
                    new ParameterDefinition(project.GetEntityName(table), "entity")
                },
                Lines = lines
            };
        }
    }
}
