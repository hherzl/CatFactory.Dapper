using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static partial class RepositoryClassBuilder
    {
        public static RepositoryClassDefinition GetRepositoryClassDefinition(this ProjectFeature<DapperProjectSettings> projectFeature)
        {
            var classDefinition = new RepositoryClassDefinition
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
                Namespace = projectFeature.GetDapperProject().GetDataLayerRepositoriesNamespace(),
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
                        Invocation = "base(connection)"
                    }
                }
            };

            foreach (var table in projectFeature.Project.Database.Tables)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(table);

                if (projectFeature.Project.Database.HasDefaultSchema(table))
                    classDefinition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetEntityLayerNamespace());
                else
                    classDefinition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetEntityLayerNamespace(table.Schema));

                classDefinition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetDataLayerContractsNamespace());
            }

            var dbos = projectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();

            var db = projectFeature.Project.Database;

            var tables = db.Tables.Where(t => dbos.Contains(t.FullName)).ToList();

            foreach (var table in tables)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(table);

                classDefinition.Methods.Add(GetGetAllMethod(projectFeature, table));

                if (table.PrimaryKey != null)
                    classDefinition.Methods.Add(GetGetMethod(projectFeature, table));

                foreach (var unique in table.Uniques)
                    classDefinition.Methods.Add(GetByUniqueMethod(projectFeature, table, unique));

                classDefinition.Methods.Add(GetAddMethod(projectFeature, table));

                if (table.PrimaryKey != null)
                {
                    classDefinition.Methods.Add(GetUpdateMethod(projectFeature, table));
                    classDefinition.Methods.Add(GetRemoveMethod(projectFeature, table));
                }
            }

            var views = db.Views.Where(v => dbos.Contains(v.FullName)).ToList();

            foreach (var view in views)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(view);

                classDefinition.Methods.Add(GetGetAllMethod(projectFeature, view));
            }

            var scalarFunctions = db.ScalarFunctions.Where(sf => dbos.Contains(sf.FullName)).ToList();

            foreach (var scalarFunction in scalarFunctions)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(scalarFunction);

                classDefinition.Methods.Add(GetGetAllMethod(projectFeature, scalarFunction));
            }

            var tableFunctions = db.TableFunctions.Where(tf => dbos.Contains(tf.FullName)).ToList();

            foreach (var tableFunction in tableFunctions)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(tableFunction);

                classDefinition.Methods.Add(GetGetAllMethod(projectFeature, tableFunction));
            }

            var storedProcedures = db.StoredProcedures.Where(sp => dbos.Contains(sp.FullName)).ToList();

            foreach (var storedProcedure in storedProcedures)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(storedProcedure);

                classDefinition.Methods.Add(GetGetAllMethod(projectFeature, storedProcedure));
            }

            return classDefinition;
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
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

                    parameters.Add(new ParameterDefinition(db.ResolveDatabaseType(column), column.GetParameterName()) { DefaultValue = "null" });
                }
            }

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", table.GetEntityName()), table.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, IView view)
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

        private static MethodDefinition GetGetMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
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

        private static MethodDefinition GetByUniqueMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var selection = projectFeature.GetDapperProject().GetSelection(table);
            var lines = new List<ILine>();
            var db = projectFeature.Project.Database;
            var key = table.GetColumnsFromConstraint(unique).ToList();

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
                var column = key[i];

                lines.Add(new CodeLine("parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(" Retrieve result from database and convert to entity class"));

            if (selection.Settings.UseStringBuilderForQueries)
                lines.Add(new CodeLine("return await Connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));
            else
                lines.Add(new CodeLine("return await Connection.QueryFirstOrDefaultAsync<{0}>(query, parameters);", table.GetEntityName()));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, ScalarFunction scalarFunction)
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
                parameters.Add(new ParameterDefinition(db.ResolveType(parameter.Type).GetClrType().Name, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));

            return new MethodDefinition(string.Format("Task<{0}>", typeToReturn), scalarFunction.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, TableFunction tableFunction)
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
            {
                if (selection.Settings.UseStringBuilderForQueries)
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", tableFunction.GetResultName()));
                else
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query);", tableFunction.GetResultName()));
            }
            else
            {
                if (selection.Settings.UseStringBuilderForQueries)
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString(), parameters);", tableFunction.GetResultName()));
                else
                    lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query, parameters);", tableFunction.GetResultName()));
            }

            var parameters = new List<ParameterDefinition>();

            foreach (var parameter in tableFunction.Parameters)
                parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveType(parameter.Type).ClrAliasType, NamingConvention.GetCamelCase(parameter.Name.Replace("@", ""))));

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", tableFunction.GetResultName()), tableFunction.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, StoredProcedure storedProcedure)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetDapperProject().GetSelection(storedProcedure);
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
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString());", storedProcedure.GetResultName()));
            else
                lines.Add(new CodeLine("return await Connection.QueryAsync<{0}>(query.ToString(), parameters);", storedProcedure.GetResultName()));

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

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", storedProcedure.GetResultName()), storedProcedure.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetAddMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
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
                lines.Add(new CodeLine("entity.{0} = parameters.Get<{1}>(\"{2}\");", identityColumn.GetPropertyName(), db.ResolveDatabaseType(identityColumn), identityColumn.GetParameterName()));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine("return affectedRows;"));
            }

            return new MethodDefinition("Task<Int32>", table.GetAddRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
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

        private static MethodDefinition GetRemoveMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
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
