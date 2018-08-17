using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static partial class RepositoryClassBuilder
    {
        private static MethodDefinition GetGetAllMethodWithConnectionAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var lines = new List<ILine>();
            var tab = 0;
            var db = projectFeature.Project.Database;
            var filters = table.ForeignKeys.Count > 0 || selection.Settings.AddPagingForGetAllOperation ? true : false;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(tab, " Create string builder for query"));
                lines.Add(new CodeLine(tab, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, "query.Append(\" from \");"));
                lines.Add(new CodeLine(tab, "query.Append(\"  {0} \");", db.GetFullName(table)));

                if (filters)
                {
                    lines.Add(new CodeLine(tab, "query.Append(\" where \");"));

                    for (var i = 0; i < table.ForeignKeys.Count; i++)
                    {
                        var foreignKey = table.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                            lines.Add(new CodeLine(tab, "query.Append(\"  ({0} is null or {1} = {0}) {2} \");", db.GetParameterName(column), db.GetColumnName(column), i < table.ForeignKeys.Count - 1 ? "and" : string.Empty));
                        }
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine(tab, "query.Append(\" order by \");"));

                    lines.Add(new CodeLine(tab, "query.Append(\" {0} \");", db.GetColumnName(table.Columns.First())));

                    lines.Add(new CodeLine(tab, "query.Append(\" offset @pageSize * (@pageNumber - 1) rows \");"));
                    lines.Add(new CodeLine(tab, "query.Append(\" fetch next @pageSize rows only \");"));
                }
            }
            else
            {
                lines.Add(new CommentLine(tab, " Create sql statement"));

                lines.Add(new CodeLine(tab, "var query = @\""));
                lines.Add(new CodeLine(tab, " select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(tab, "  {0}{1} ", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, " from "));
                lines.Add(new CodeLine(tab, "  {0} ", db.GetFullName(table)));

                if (filters && table.ForeignKeys.Count > 0)
                {
                    lines.Add(new CodeLine(1, " where "));

                    for (var i = 0; i < table.ForeignKeys.Count; i++)
                    {
                        var foreignKey = table.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                            lines.Add(new CodeLine(tab, "  ({0} is null or {1} = {0}) {2} ", db.GetParameterName(column), db.GetColumnName(column), i < table.ForeignKeys.Count - 1 ? "and" : string.Empty));
                        }
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine(tab, " order by "));

                    lines.Add(new CodeLine(tab, " {0} ", db.GetColumnName(table.Columns.First())));

                    lines.Add(new CodeLine(tab, " offset @pageSize * (@pageNumber - 1) rows "));
                    lines.Add(new CodeLine(tab, " fetch next @pageSize rows only "));
                }

                lines.Add(new CodeLine(tab, " \";"));
            }

            lines.Add(new CodeLine());

            if (filters)
            {
                lines.Add(new CommentLine(tab, " Create parameters collection"));
                lines.Add(new CodeLine(tab, "var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(tab, " Add parameters to collection"));

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine(tab, "parameters.Add(\"@pageSize\", pageSize);"));
                    lines.Add(new CodeLine(tab, "parameters.Add(\"@pageNumber\", pageNumber);"));
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                    lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", {1});", db.GetParameterName(column), column.GetParameterName()));
                }

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(tab, " Retrieve result from database"));

            if (filters)
                lines.Add(new CodeLine(tab, "return await connection.QueryAsync<{0}>(new CommandDefinition({1}, parameters));", table.GetEntityName(), selection.Settings.UseStringBuilderForQueries ? "query.ToString()" : "query"));
            else
                lines.Add(new CodeLine(tab, "return await connection.QueryAsync<{0}>({1});", table.GetEntityName(), selection.Settings.UseStringBuilderForQueries ? "query.ToString()" : "query"));

            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition("IDbConnection", "connection")
            };

            if (filters)
            {
                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    parameters.Add(new ParameterDefinition("Int32", "pageSize") { DefaultValue = "10" });
                    parameters.Add(new ParameterDefinition("Int32", "pageNumber") { DefaultValue = "1" });
                }

                // todo: Add logic to retrieve multiple columns from foreign key
                foreach (var foreignKey in table.ForeignKeys)
                {
                    var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                    parameters.Add(new ParameterDefinition(db.ResolveType(column), column.GetParameterName()) { DefaultValue = "null" });
                }
            }

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", table.GetEntityName()), table.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethodWithConnectionAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, IView view)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(view);
            var tab = 0;
            var lines = new List<ILine>();
            var db = projectFeature.Project.Database;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(tab, " Create string builder for query"));
                lines.Add(new CodeLine(tab, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "query.Append(\" select \");"));

                for (var i = 0; i < view.Columns.Count; i++)
                {
                    var column = view.Columns[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < view.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, "query.Append(\" from \");"));
                lines.Add(new CodeLine(tab, "query.Append(\"  {0} \");", db.GetFullName(view)));
                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "var query = @\" select "));

                for (var i = 0; i < view.Columns.Count; i++)
                {
                    var column = view.Columns[i];

                    lines.Add(new CodeLine(tab, "  {0}{1} ", db.GetColumnName(column), i < view.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, " from "));
                lines.Add(new CodeLine(tab, "  {0} ", db.GetFullName(view)));
                lines.Add(new CodeLine(tab, " \";"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(tab, " Retrieve result from database"));
            lines.Add(new CodeLine(tab, "return await connection.QueryAsync<{0}>(query.ToString());", view.GetEntityName()));
            lines.Add(new CodeLine("}"));

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", view.GetEntityName()), view.GetGetAllRepositoryMethodName(), new ParameterDefinition("IDbConnection", "connection"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetMethodAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var lines = new List<ILine>();
            var tab = 0;
            var db = projectFeature.Project.Database;
            var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(tab, " Create string builder for query"));
                lines.Add(new CodeLine(tab, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, "query.Append(\" from \");"));
                lines.Add(new CodeLine(tab, "query.Append(\"  {0} \");", db.GetFullName(table)));

                lines.Add(new CodeLine(tab, "query.Append(\" where \");"));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0} = {1} \");", db.GetColumnName(column), db.GetParameterName(column)));
                    lines.Add(new CodeLine());
                }
            }
            else
            {
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "var query = @\" select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(tab, "  {0}{1} ", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, " from "));
                lines.Add(new CodeLine(tab, "  {0} ", db.GetFullName(table)));

                lines.Add(new CodeLine(tab, " where "));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine(tab, "  {0} = {1} ", db.GetColumnName(column), db.GetParameterName(column)));
                }

                lines.Add(new CodeLine(tab, " \";"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(tab, " Create parameters collection"));
            lines.Add(new CodeLine(tab, "var parameters = new DynamicParameters();"));
            lines.Add(new CodeLine());

            lines.Add(new CommentLine(tab, " Add parameters to collection"));

            for (var i = 0; i < key.Count; i++)
            {
                var column = key[i];

                lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(tab, " Retrieve result from database and convert to entity class"));
            lines.Add(new CodeLine(tab, "return await connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetRepositoryMethodName(), new ParameterDefinition("IDbConnection", "connection"), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetByUniqueMethodAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var lines = new List<ILine>
            {
                new CommentLine(" Create string builder for query"),
                new CodeLine("var query = new StringBuilder();"),
                new CodeLine(),
                new CommentLine(" Create sql statement"),
                new CodeLine("query.Append(\" select \");")
            };

            var db = projectFeature.Project.Database;

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine("query.Append(\" from \");"));
            lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(table)));

            lines.Add(new CodeLine("query.Append(\" where \");"));

            var key = table.GetColumnsFromConstraint(unique).ToList();

            for (var i = 0; i < key.Count; i++)
            {
                var column = key[i];

                lines.Add(new CodeLine("query.Append(\"  {0} = {1} {2} \");", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? "and" : string.Empty));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Create parameters collection"));
            lines.Add(new CodeLine("var parameters = new DynamicParameters();"));
            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Add parameters to collection"));

            for (var i = 0; i < key.Count; i++)
            {
                var column = key[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Retrieve result from database and convert to entity class"));
            lines.Add(new CodeLine("return await connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition("IDbConnection", "connection"), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethodAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, TableFunction tableFunction)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(tableFunction);
            var lines = new List<ILine>();
            var db = projectFeature.Project.Database;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" select \");"));

                for (var i = 0; i < tableFunction.Columns.Count; i++)
                {
                    var column = tableFunction.Columns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < tableFunction.Columns.Count - 1 ? "," : string.Empty));
                }

                var selectParameters = tableFunction.Parameters.Count == 0 ? string.Empty : string.Join(", ", tableFunction.Parameters.Select(item => item.Name));

                lines.Add(new CodeLine(1, "query.Append(\" from \");"));
                lines.Add(new CodeLine(1, "query.Append(\"  {0}({1}) \");", db.GetFullName(tableFunction), selectParameters));
                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "var query = @\" select "));

                for (var i = 0; i < tableFunction.Columns.Count; i++)
                {
                    var column = tableFunction.Columns[i];

                    lines.Add(new CodeLine(1, "  {0}{1} ", db.GetColumnName(column), i < tableFunction.Columns.Count - 1 ? "," : string.Empty));
                }

                var selectParameters = tableFunction.Parameters.Count == 0 ? string.Empty : string.Join(", ", tableFunction.Parameters.Select(item => item.Name));

                lines.Add(new CodeLine(1, " from "));
                lines.Add(new CodeLine(1, "  {0}({1}) ", db.GetFullName(tableFunction), selectParameters));
                lines.Add(new CodeLine(1, " \";"));
                lines.Add(new CodeLine());
            }

            if (tableFunction.Parameters.Count > 0)
            {
                lines.Add(new CommentLine(1, " Create parameters collection"));
                lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Add parameters to collection"));

                foreach (var parameter in tableFunction.Parameters)
                    lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", {1});", parameter.Name, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Retrieve result from database and convert to typed list"));

            if (tableFunction.Parameters.Count == 0)
                lines.Add(new CodeLine(1, "return await connection.QueryAsync<{0}>(query.ToString());", tableFunction.GetEntityName()));
            else
                lines.Add(new CodeLine(1, "return await connection.QueryAsync<{0}>(query.ToString(), parameters);", tableFunction.GetEntityName()));

            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition("IDbConnection", "connection")
            };

            foreach (var parameter in tableFunction.Parameters)
            {
                parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveType(parameter.Type).ClrAliasType, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));
            }

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", tableFunction.GetEntityName()), tableFunction.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethodAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, ScalarFunction scalarFunction)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(scalarFunction);
            var lines = new List<ILine>();
            var db = projectFeature.Project.Database;
            var scalarFunctionParameters = scalarFunction.Parameters.Where(item => !string.IsNullOrEmpty(item.Name)).ToList();
            var typeToReturn = db.ResolveType(scalarFunction.Parameters.FirstOrDefault(item => string.IsNullOrEmpty(item.Name)).Type).GetClrType().Name;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" select \");"));

                var selectParameters = scalarFunctionParameters.Count == 0 ? string.Empty : string.Join(", ", scalarFunctionParameters.Select(item => item.Name));

                lines.Add(new CodeLine(1, "query.Append(\"  {0}({1}) \");", db.GetFullName(scalarFunction), selectParameters));
                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "var query = @\" select "));

                var selectParameters = scalarFunctionParameters.Count == 0 ? string.Empty : string.Join(", ", scalarFunctionParameters.Select(item => item.Name));

                lines.Add(new CodeLine(1, "  {0}({1}) ", db.GetFullName(scalarFunction), selectParameters));
                lines.Add(new CodeLine(1, " \";"));
                lines.Add(new CodeLine());
            }

            if (scalarFunctionParameters.Count > 0)
            {
                lines.Add(new CommentLine(1, " Create parameters collection"));
                lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Add parameters to collection"));

                foreach (var parameter in scalarFunctionParameters)
                    lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", {1});", parameter.Name, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Retrieve scalar from database and cast to specific CLR type"));

            if (scalarFunctionParameters.Count == 0)
            {
                lines.Add(new CodeLine(1, "var scalar = await connection.ExecuteScalarAsync(query.ToString());"));
                lines.Add(new CodeLine(1, "return ({0})scalar;", typeToReturn));
            }
            else
            {
                lines.Add(new CodeLine(1, "var scalar = await connection.ExecuteScalarAsync(query.ToString(), parameters);", typeToReturn));
                lines.Add(new CodeLine(1, "return ({0})scalar;", typeToReturn));
            }

            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition("IDbConnection", "connection")
            };

            foreach (var parameter in scalarFunctionParameters)
            {
                parameters.Add(new ParameterDefinition(db.ResolveType(parameter.Type).GetClrType().Name, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));
            }

            return new MethodDefinition(string.Format("Task<{0}>", typeToReturn), scalarFunction.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetAddMethodAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            if (projectFeature.Project.Database.PrimaryKeyIsGuid(table))
            {
                lines.Add(new CommentLine(" Generate value for Guid property"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", table.GetColumnsFromConstraint(table.PrimaryKey).First().GetPropertyName()));
                lines.Add(new CodeLine());
            }

            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var tab = 0;
            var db = projectFeature.Project.Database;
            var insertColumns = projectFeature.GetDapperProject().GetInsertColumns(table).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(tab, " Create string builder for query"));
                lines.Add(new CodeLine(tab, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "query.Append(\" insert into \");"));
                lines.Add(new CodeLine(tab, "query.Append(\"  {0} \");", db.GetFullName(table)));
                lines.Add(new CodeLine(tab, "query.Append(\"  ( \");"));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"   {0}{1} \");", db.GetColumnName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, "query.Append(\"  ) \");"));
                lines.Add(new CodeLine(tab, "query.Append(\" values \");"));
                lines.Add(new CodeLine(tab, "query.Append(\" ( \");"));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0}{1} \");", db.GetParameterName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, "query.Append(\" ) \");"));

                if (table.Identity != null)
                {
                    var identityColumn = table.GetIdentityColumn();

                    lines.Add(new CodeLine(tab, "query.Append(\"  select {0} = @@identity \");", db.GetParameterName(identityColumn)));
                }
            }
            else
            {
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "var query = @\" insert into "));
                lines.Add(new CodeLine(tab, "  {0} ", db.GetFullName(table)));
                lines.Add(new CodeLine(tab, "  ( "));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(tab, "   {0}{1} ", db.GetColumnName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, "  ) "));
                lines.Add(new CodeLine(tab, " values "));
                lines.Add(new CodeLine(tab, " ( "));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(tab, "  {0}{1} ", db.GetParameterName(column), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, " ) "));

                if (table.Identity != null)
                {
                    var identityColumn = table.GetIdentityColumn();

                    lines.Add(new CodeLine(tab, "  select {0} = @@identity ", db.GetParameterName(identityColumn)));
                }

                lines.Add(new CodeLine(tab, " \";"));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(tab, " Create parameters collection"));
            lines.Add(new CodeLine(tab, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(tab, " Add parameters to collection"));

            if (table.Identity == null)
            {
                for (var i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
                }

                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Execute query in database"));
                lines.Add(new CodeLine(tab, "return await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));
            }
            else
            {
                for (var i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
                }

                var identityColumn = table.GetIdentityColumn();

                lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", dbType: {1}, direction: ParameterDirection.Output);", identityColumn.GetParameterName(), db.ResolveDbType(identityColumn)));

                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Execute query in database"));
                lines.Add(new CodeLine(tab, "var affectedRows = await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(tab, " Retrieve value for output parameters"));
                lines.Add(new CodeLine(tab, "entity.{0} = parameters.Get<{1}>(\"{2}\");", identityColumn.GetPropertyName(), db.ResolveType(identityColumn), identityColumn.GetParameterName()));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(tab, "return affectedRows;"));
            }

            return new MethodDefinition("Task<Int32>", table.GetAddRepositoryMethodName(), new ParameterDefinition("IDbConnection", "connection"), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethodAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var lines = new List<ILine>();
            var tab = 0;
            var db = projectFeature.Project.Database;
            var sets = projectFeature.GetDapperProject().GetUpdateColumns(table).ToList();
            var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(tab, " Create string builder for query"));
                lines.Add(new CodeLine(tab, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "query.Append(\" update \");"));
                lines.Add(new CodeLine(tab, "query.Append(\"  {0} \");", db.GetFullName(table)));
                lines.Add(new CodeLine(tab, "query.Append(\" set \");"));

                for (var i = 0; i < sets.Count(); i++)
                {
                    var column = sets[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0} = {1}{2 } \");", db.GetColumnName(column), db.GetParameterName(column), i < sets.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "query.Append(\" where \");"));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0} = {1}{2} \");", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }
            }
            else
            {
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "var query = @\" update "));
                lines.Add(new CodeLine(tab, "  {0} ", db.GetFullName(table)));
                lines.Add(new CodeLine(tab, " set "));

                for (var i = 0; i < sets.Count(); i++)
                {
                    var column = sets[i];

                    lines.Add(new CodeLine(tab, "  {0} = {1}{2} ", db.GetColumnName(column), db.GetParameterName(column), i < sets.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(tab, " where "));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine(tab, "  {0} = {1}{2} ", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }

                lines.Add(new CodeLine(tab, " \"; "));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(tab, " Create parameters collection"));
            lines.Add(new CodeLine(tab, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(tab, " Add parameters to collection"));

            for (var i = 0; i < sets.Count; i++)
            {
                var column = sets[i];

                lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
            }

            for (var i = 0; i < key.Count; i++)
            {
                var column = key[i];

                lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(tab, " Execute query in database"));
            lines.Add(new CodeLine(tab, "return await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            return new MethodDefinition("Task<Int32>", table.GetUpdateRepositoryMethodName(), new ParameterDefinition("IDbConnection", "connection"), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethodAsParameter(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var lines = new List<ILine>();
            var tab = 0;
            var db = projectFeature.Project.Database;
            var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(tab, " Create string builder for query"));
                lines.Add(new CodeLine(tab, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "query.Append(\" delete from \");"));
                lines.Add(new CodeLine(tab, "query.Append(\"  {0} \");", db.GetFullName(table)));
                lines.Add(new CodeLine(tab, "query.Append(\" where \");"));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine(tab, "query.Append(\"  {0} = {1}{2} \");", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }
            }
            else
            {
                lines.Add(new CommentLine(tab, " Create sql statement"));
                lines.Add(new CodeLine(tab, "var query = @\" delete from "));
                lines.Add(new CodeLine(tab, "  {0} ", db.GetFullName(table)));
                lines.Add(new CodeLine(tab, " where "));

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    lines.Add(new CodeLine(tab, "  {0} = {1}{2} ", db.GetColumnName(column), db.GetParameterName(column), i < key.Count - 1 ? " and " : string.Empty));
                }

                lines.Add(new CodeLine(tab, " \"; "));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(tab, " Create parameters collection"));
            lines.Add(new CodeLine(tab, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(tab, " Add parameters to collection"));

            var columns = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(tab, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(tab, " Execute query in database"));
            lines.Add(new CodeLine(tab, "return await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            return new MethodDefinition("Task<Int32>", table.GetDeleteRepositoryMethodName(), new ParameterDefinition("IDbConnection", "connection"), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
