using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static partial class RepositoryClassBuilder
    {
        private static MethodDefinition GetGetAllMethodWithConnectionAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(table);
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

                    lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
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

                    lines.Add(new CodeLine("query.Append(\" {0} \");", db.GetColumnName(table.Columns.First())));

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
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperation)
                {
                    lines.Add(new CodeLine(" order by "));

                    lines.Add(new CodeLine(" {0} ", db.GetColumnName(table.Columns.First())));

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

                    lines.Add(new CodeLine("parameters.Add(\"{0}\", {1});", db.GetParameterName(column), column.GetParameterName()));
                }

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Retrieve result from database and convert to typed list"));

            if (filters)
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(new CommandDefinition(query.ToString(), parameters));", table.GetEntityName()));
            else
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", table.GetEntityName())); ;

            var parameters = new List<ParameterDefinition>();

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

                    parameters.Add(new ParameterDefinition(db.ResolveDatebaseType(column), column.GetParameterName()) { DefaultValue = "null" });
                }
            }

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", table.GetEntityName()), table.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethodWithConnectionAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, IView view)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(view);
            var db = projectFeature.Project.Database;

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

                    lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < view.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine("query.Append(\" from \");"));
                lines.Add(new CodeLine("query.Append(\"  {0} \");", db.GetFullName(view)));
                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(" Create sql statement"));
                lines.Add(new CodeLine("var query = @\" select "));

                for (var i = 0; i < view.Columns.Count; i++)
                {
                    var column = view.Columns[i];

                    lines.Add(new CodeLine("  {0}{1} ", db.GetColumnName(column), i < view.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(" from "));
                lines.Add(new CodeLine("  {0} ", db.GetFullName(view)));
                lines.Add(new CodeLine(" \";"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Retrieve result from database and convert to typed list"));
            lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", view.GetEntityName()));

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", view.GetEntityName()), view.GetGetAllRepositoryMethodName())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetMethodAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var db = projectFeature.Project.Database;
            var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

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

                    lines.Add(new CodeLine("query.Append(\"  {0}{1} \");", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
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

                    lines.Add(new CodeLine("  {0}{1} ", db.GetColumnName(column), i < table.Columns.Count - 1 ? "," : string.Empty));
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
                var column = key[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Retrieve result from database and convert to entity class"));
            lines.Add(new CodeLine("return await Connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetByUniqueMethodAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, ITable table, Unique unique)
        {
            // todo: Add flag to validate if StringBuilder must be used to create query inside of methods

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
            lines.Add(new CodeLine("return await Connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethodAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, TableFunction tableFunction)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(tableFunction);
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
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", tableFunction.GetEntityName()));
            else
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString(), parameters);", tableFunction.GetEntityName()));

            var parameters = new List<ParameterDefinition>();

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

        private static MethodDefinition GetGetAllMethodAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, ScalarFunction scalarFunction)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(scalarFunction);
            var db = projectFeature.Project.Database;
            var typeToReturn = db.ResolveType(scalarFunction.Parameters.FirstOrDefault(item => string.IsNullOrEmpty(item.Name)).Type).GetClrType().Name;
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
                lines.Add(new CodeLine("var scalar = await Connection.ExecuteScalarAsync(query.ToString());"));
                lines.Add(new CodeLine("return ({0})scalar;", typeToReturn));
            }
            else
            {
                lines.Add(new CodeLine("var scalar = await Connection.ExecuteScalarAsync(query.ToString(), parameters);", typeToReturn));
                lines.Add(new CodeLine("return ({0})scalar;", typeToReturn));
            }

            var parameters = new List<ParameterDefinition>();

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

        private static MethodDefinition GetAddMethodAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();
            var db = projectFeature.Project.Database;

            if (table.PrimaryKey != null && db.PrimaryKeyIsGuid(table))
            {
                lines.Add(new CommentLine(" Generate value for Guid property"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", table.GetColumnsFromConstraint(table.PrimaryKey).First().GetPropertyName()));
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

                    lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
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

                    lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
                }

                var identityColumn = table.GetIdentityColumn();

                lines.Add(new CodeLine("parameters.Add(\"{0}\", dbType: {1}, direction: ParameterDirection.Output);", identityColumn.GetParameterName(), db.ResolveDbType(identityColumn)));

                lines.Add(new CodeLine());
                lines.Add(new CommentLine(" Execute query in database"));
                lines.Add(new CodeLine("var affectedRows = await Connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(" Retrieve value for output parameters"));
                lines.Add(new CodeLine("entity.{0} = parameters.Get<{1}>(\"{2}\");", identityColumn.GetPropertyName(), db.ResolveDatebaseType(identityColumn), identityColumn.GetParameterName()));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine("return affectedRows;"));
            }

            return new MethodDefinition("Task<Int32>", table.GetAddRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethodAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var db = projectFeature.Project.Database;
            var sets = projectFeature.GetDapperProject().GetUpdateColumns(table).ToList();
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

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
            }

            for (var i = 0; i < key.Count; i++)
            {
                var column = key[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), table.GetPropertyNameHack(column)));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Execute query in database"));
            lines.Add(new CodeLine("return await Connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            return new MethodDefinition("Task<Int32>", table.GetUpdateRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethodAsLocal(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var db = projectFeature.Project.Database;
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
                    var column = key[i];

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
                var column = columns[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Execute query in database"));
            lines.Add(new CodeLine("return await Connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            return new MethodDefinition("Task<Int32>", table.GetDeleteRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
