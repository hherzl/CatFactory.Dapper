using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions
{
    public static class RepositoryClassDefinition
    {
        public static CSharpClassDefinition GetRepositoryClassDefinition(this ProjectFeature<DapperProjectSettings> projectFeature)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Collections.Generic");
            classDefinition.Namespaces.Add("System.Data");
            classDefinition.Namespaces.Add("System.Data.SqlClient");
            classDefinition.Namespaces.Add("System.Linq");
            classDefinition.Namespaces.Add("System.Text");
            classDefinition.Namespaces.Add("System.Threading.Tasks");
            classDefinition.Namespaces.Add("Dapper");
            classDefinition.Namespaces.Add("Microsoft.Extensions.Options");

            foreach (var table in projectFeature.Project.Database.Tables)
            {
                if (table.HasDefaultSchema())
                    classDefinition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetEntityLayerNamespace());
                else
                    classDefinition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetEntityLayerNamespace(table.Schema));

                classDefinition.Namespaces.AddUnique(projectFeature.GetDapperProject().GetDataLayerContractsNamespace());
            }

            classDefinition.Namespace = projectFeature.GetDapperProject().GetDataLayerRepositoriesNamespace();

            classDefinition.Name = projectFeature.GetClassRepositoryName();

            classDefinition.BaseClass = "Repository";

            classDefinition.Implements.Add(projectFeature.GetInterfaceRepositoryName());

            classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
            {
                Invocation = "base(appSettings)"
            });

            var dbos = projectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = projectFeature.Project.Database.Tables.Where(t => dbos.Contains(t.FullName)).ToList();
            var views = projectFeature.Project.Database.Views.Where(v => dbos.Contains(v.FullName)).ToList();

            foreach (var table in tables)
            {
                classDefinition.Methods.Add(GetGetAllMethod(projectFeature, table));

                if (table.PrimaryKey != null)
                    classDefinition.Methods.Add(GetGetMethod(projectFeature, table));

                classDefinition.Methods.Add(GetAddMethod(projectFeature, table));

                if (table.PrimaryKey != null)
                {
                    classDefinition.Methods.Add(GetUpdateMethod(projectFeature, table));
                    classDefinition.Methods.Add(GetRemoveMethod(projectFeature, table));
                }

                foreach (var unique in table.Uniques)
                {
                    classDefinition.Methods.Add(GetByUniqueMethod(projectFeature, table, unique));
                }
            }

            foreach (var view in views)
            {
                classDefinition.Methods.Add(GetGetAllMethod(projectFeature, view));
            }

            return classDefinition;
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Create connection instance"));
            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));

            var selection = projectFeature.GetDapperProject().GetSelection(table);

            var filters = table.ForeignKeys.Count > 0 || selection.Settings.AddPagingForGetAllOperations ? true : false;

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.GetColumnName(), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "query.Append(\" from \");"));
                lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.GetFullName()));

                if (filters)
                {
                    lines.Add(new CodeLine(1, "query.Append(\" where \");"));

                    for (var i = 0; i < table.ForeignKeys.Count; i++)
                    {
                        var foreignKey = table.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                            lines.Add(new CodeLine(1, "query.Append(\"  ({0} is null or {1} = {0}) {2} \");", column.GetSqlServerParameterName(), column.GetColumnName(), i < table.ForeignKeys.Count - 1 ? "and" : string.Empty));
                        }
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperations)
                {
                    lines.Add(new CodeLine(1, "query.Append(\" order by \");"));

                    lines.Add(new CodeLine(1, "query.Append(\" {0} \");", table.Columns.First().GetColumnName()));

                    lines.Add(new CodeLine(1, "query.Append(\" offset @pageSize * (@pageNumber - 1) rows \");"));
                    lines.Add(new CodeLine(1, "query.Append(\" fetch next @pageSize rows only \");"));
                }
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));

                lines.Add(new CodeLine(1, "var query = @\""));
                lines.Add(new CodeLine(1, " select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(1, "  {0}{1} ", column.GetColumnName(), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, " from "));
                lines.Add(new CodeLine(1, "  {0} ", table.GetFullName()));

                if (filters)
                {
                    if (table.ForeignKeys.Count > 0)
                    {
                        lines.Add(new CodeLine(1, " where "));

                        for (var i = 0; i < table.ForeignKeys.Count; i++)
                        {
                            var foreignKey = table.ForeignKeys[i];

                            if (foreignKey.Key.Count == 1)
                            {
                                var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                                lines.Add(new CodeLine(1, "  ({0} is null or {1} = {0}) {2} ", column.GetSqlServerParameterName(), column.GetColumnName(), i < table.ForeignKeys.Count - 1 ? "and" : string.Empty));
                            }
                        }
                    }
                }

                if (selection.Settings.AddPagingForGetAllOperations)
                {
                    lines.Add(new CodeLine(1, " order by "));

                    lines.Add(new CodeLine(1, " {0} ", table.Columns.First().GetColumnName()));

                    lines.Add(new CodeLine(1, " offset @pageSize * (@pageNumber - 1) rows "));
                    lines.Add(new CodeLine(1, " fetch next @pageSize rows only "));
                }

                lines.Add(new CodeLine(1, " \";"));
            }

            lines.Add(new CodeLine());

            if (filters)
            {
                lines.Add(new CommentLine(1, " Create parameters collection"));
                lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Add parameters to collection"));

                if (selection.Settings.AddPagingForGetAllOperations)
                {
                    lines.Add(new CodeLine(1, "parameters.Add(\"@pageSize\", pageSize);"));
                    lines.Add(new CodeLine(1, "parameters.Add(\"@pageNumber\", pageNumber);"));
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                    lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", {1});", column.GetSqlServerParameterName(), column.GetParameterName()));
                }

                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Retrieve result from database and convert to typed list"));

            if (filters)
                lines.Add(new CodeLine(1, "return await connection.QueryAsync<{0}>(new CommandDefinition(query.ToString(), parameters));", table.GetEntityName()));
            else
                lines.Add(new CodeLine(1, "return await connection.QueryAsync<{0}>(query.ToString());", table.GetEntityName())); ;

            lines.Add(new CodeLine("}"));

            var parameters = new List<ParameterDefinition>();

            if (filters)
            {
                if (selection.Settings.AddPagingForGetAllOperations)
                {
                    parameters.Add(new ParameterDefinition("Int32", "pageSize") { DefaultValue = "10" });
                    parameters.Add(new ParameterDefinition("Int32", "pageNumber") { DefaultValue = "1" });
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    // todo: add logic to retrieve multiple columns from foreign key
                    var column = table.GetColumnsFromConstraint(foreignKey).ToList().First();

                    parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveType(column), column.GetParameterName()) { DefaultValue = "null" });
                }
            }

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", table.GetEntityName()), table.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<DapperProjectSettings> projectFeature, IView table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Create connection instance"));
            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));

            var selection = projectFeature.GetDapperProject().GetSelection(table);

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.GetColumnName(), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "query.Append(\" from \");"));
                lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.GetFullName()));
                lines.Add(new CodeLine());
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "var query = @\" select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(1, "  {0}{1} ", column.GetColumnName(), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, " from "));
                lines.Add(new CodeLine(1, "  {0} ", table.GetFullName()));
                lines.Add(new CodeLine(1, " \";"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Retrieve result from database and convert to typed list"));
            lines.Add(new CodeLine(1, "return await connection.QueryAsync<{0}>(query.ToString());", table.GetEntityName()));
            lines.Add(new CodeLine("}"));

            return new MethodDefinition(string.Format("Task<IEnumerable<{0}>>", table.GetEntityName()), table.GetGetAllRepositoryMethodName())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Create connection instance"));
            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));

            var keyColumns = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            var selection = projectFeature.GetDapperProject().GetSelection(table);

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" select \");"));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.GetColumnName(), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "query.Append(\" from \");"));
                lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.GetFullName()));

                lines.Add(new CodeLine(1, "query.Append(\" where \");"));

                for (var i = 0; i < keyColumns.Count; i++)
                {
                    var column = keyColumns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1} \");", column.GetColumnName(), column.GetSqlServerParameterName()));
                    lines.Add(new CodeLine());
                }
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "var query = @\" select "));

                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];

                    lines.Add(new CodeLine(1, "  {0}{1} ", column.GetColumnName(), i < table.Columns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, " from "));
                lines.Add(new CodeLine(1, "  {0} ", table.GetFullName()));

                lines.Add(new CodeLine(1, " where "));

                for (var i = 0; i < keyColumns.Count; i++)
                {
                    var column = keyColumns[i];

                    lines.Add(new CodeLine(1, "  {0} = {1} ", column.GetColumnName(), column.GetSqlServerParameterName()));
                }

                lines.Add(new CodeLine(1, " \";"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Create parameters collection"));
            lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));
            lines.Add(new CodeLine());

            lines.Add(new CommentLine(1, " Add parameters to collection"));

            for (var i = 0; i < keyColumns.Count; i++)
            {
                var column = keyColumns[i];

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(1, " Retrieve result from database and convert to entity class"));
            lines.Add(new CodeLine(1, "return await connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));
            lines.Add(new CodeLine("}"));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetByUniqueMethod(ProjectFeature<DapperProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Create connection instance"));
            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CommentLine(1, " Create string builder for query"));
            lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(1, " Create sql statement"));
            lines.Add(new CodeLine(1, "query.Append(\" select \");"));

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.GetColumnName(), i < table.Columns.Count - 1 ? "," : string.Empty));
            }

            lines.Add(new CodeLine(1, "query.Append(\" from \");"));
            lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.GetFullName()));

            lines.Add(new CodeLine(1, "query.Append(\" where \");"));

            if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
            {
                var column = table.GetColumnsFromConstraint(unique).First();

                lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1} \");", column.GetColumnName(), column.GetSqlServerParameterName()));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Create parameters collection"));
                lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve result from database and convert to entity class"));
                lines.Add(new CodeLine(1, "return await connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));
                lines.Add(new CodeLine("}"));
            }

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetAddMethod(ProjectFeature<DapperProjectSettings> projectFeature, Table table)
        {
            var lines = new List<ILine>();

            if (projectFeature.Project.Database.PrimaryKeyIsGuid(table))
            {
                lines.Add(new CommentLine(" Generate value for Guid property"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", table.GetColumnsFromConstraint(table.PrimaryKey).First().GetPropertyName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Create connection instance"));
            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));

            var insertColumns = projectFeature.GetDapperProject().GetInsertColumns(table).ToList();

            var selection = projectFeature.GetDapperProject().GetSelection(table);

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" insert into \");"));
                lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.GetFullName()));
                lines.Add(new CodeLine(1, "query.Append(\"  ( \");"));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"   {0}{1} \");", column.GetColumnName(), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "query.Append(\"  ) \");"));
                lines.Add(new CodeLine(1, "query.Append(\" values \");"));
                lines.Add(new CodeLine(1, "query.Append(\" ( \");"));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.GetSqlServerParameterName(), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "query.Append(\" ) \");"));

                if (table.Identity != null)
                {
                    var identityColumn = table.GetIdentityColumn();

                    lines.Add(new CodeLine(1, "query.Append(\"  select {0} = @@identity \");", identityColumn.GetSqlServerParameterName()));
                }
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "var query = @\" insert into "));
                lines.Add(new CodeLine(1, "  {0} ", table.GetFullName()));
                lines.Add(new CodeLine(1, "  ( "));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(1, "   {0}{1} ", column.GetColumnName(), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "  ) "));
                lines.Add(new CodeLine(1, " values "));
                lines.Add(new CodeLine(1, " ( "));

                for (var i = 0; i < insertColumns.Count(); i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(1, "  {0}{1} ", column.GetSqlServerParameterName(), i < insertColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, " ) "));

                if (table.Identity != null)
                {
                    var identityColumn = table.GetIdentityColumn();

                    lines.Add(new CodeLine(1, "  select {0} = @@identity ", identityColumn.GetSqlServerParameterName()));
                }

                lines.Add(new CodeLine(1, " \";"));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(1, " Create parameters collection"));
            lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(1, " Add parameters to collection"));

            if (table.Identity == null)
            {
                for (var i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
                }

                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Execute query in database"));
                lines.Add(new CodeLine(1, "return await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

                lines.Add(new CodeLine("}"));
            }
            else
            {
                for (var i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];

                    lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
                }

                var identityColumn = table.GetIdentityColumn();

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", dbType: {1}, direction: ParameterDirection.Output);", identityColumn.GetParameterName(), projectFeature.Project.Database.ResolveDbType(identityColumn)));

                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Execute query in database"));
                lines.Add(new CodeLine(1, "var affectedRows = await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve value for output parameters"));
                lines.Add(new CodeLine(1, "entity.{0} = parameters.Get<{1}>(\"{2}\");", identityColumn.GetPropertyName(), projectFeature.Project.Database.ResolveType(identityColumn), identityColumn.GetParameterName()));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "return affectedRows;"));

                lines.Add(new CodeLine("}"));
            }

            return new MethodDefinition("Task<Int32>", table.GetAddRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethod(ProjectFeature<DapperProjectSettings> projectFeature, Table table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Create connection instance"));
            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));

            var updateColumns = projectFeature.GetDapperProject().GetUpdateColumns(table).ToList();
            var keyColumns = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            var selection = projectFeature.GetDapperProject().GetSelection(table);

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" update \");"));
                lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.GetFullName()));
                lines.Add(new CodeLine(1, "query.Append(\" set \");"));

                for (var i = 0; i < updateColumns.Count(); i++)
                {
                    var column = updateColumns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1}{2 } \");", column.GetColumnName(), column.GetSqlServerParameterName(), i < updateColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, "query.Append(\" where \");"));

                for (var i = 0; i < keyColumns.Count; i++)
                {
                    var column = keyColumns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1}{2} \");", column.GetColumnName(), column.GetSqlServerParameterName(), i < keyColumns.Count - 1 ? " and " : string.Empty));
                }
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "var query = @\" update "));
                lines.Add(new CodeLine(1, "  {0} ", table.GetFullName()));
                lines.Add(new CodeLine(1, " set "));

                for (var i = 0; i < updateColumns.Count(); i++)
                {
                    var column = updateColumns[i];

                    lines.Add(new CodeLine(1, "  {0} = {1}{2 } ", column.GetColumnName(), column.GetSqlServerParameterName(), i < updateColumns.Count - 1 ? "," : string.Empty));
                }

                lines.Add(new CodeLine(1, " where "));

                for (var i = 0; i < keyColumns.Count; i++)
                {
                    var column = keyColumns[i];

                    lines.Add(new CodeLine(1, "  {0} = {1}{2} ", column.GetColumnName(), column.GetSqlServerParameterName(), i < keyColumns.Count - 1 ? " and " : string.Empty));
                }

                lines.Add(new CodeLine(1, " \"; "));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(1, " Create parameters collection"));
            lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(1, " Add parameters to collection"));

            for (var i = 0; i < updateColumns.Count; i++)
            {
                var column = updateColumns[i];

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            for (var i = 0; i < keyColumns.Count; i++)
            {
                var column = keyColumns[i];

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(1, " Execute query in database"));
            lines.Add(new CodeLine(1, "return await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            lines.Add(new CodeLine("}"));

            return new MethodDefinition("Task<Int32>", table.GetUpdateRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethod(ProjectFeature<DapperProjectSettings> projectFeature, Table table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Create connection instance"));
            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));

            var keyColumns = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            var selection = projectFeature.GetDapperProject().GetSelection(table);

            if (selection.Settings.UseStringBuilderForQueries)
            {
                lines.Add(new CommentLine(1, " Create string builder for query"));
                lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
                lines.Add(new CodeLine());
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "query.Append(\" delete from \");"));
                lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.GetFullName()));
                lines.Add(new CodeLine(1, "query.Append(\" where \");"));

                for (var i = 0; i < keyColumns.Count; i++)
                {
                    var column = keyColumns[i];

                    lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1}{2} \");", column.GetColumnName(), column.GetSqlServerParameterName(), i < keyColumns.Count - 1 ? " and " : string.Empty));
                }
            }
            else
            {
                lines.Add(new CommentLine(1, " Create sql statement"));
                lines.Add(new CodeLine(1, "var query = @\" delete from "));
                lines.Add(new CodeLine(1, "  {0} ", table.GetFullName()));
                lines.Add(new CodeLine(1, " where "));

                for (var i = 0; i < keyColumns.Count; i++)
                {
                    var column = keyColumns[i];

                    lines.Add(new CodeLine(1, "  {0} = {1}{2} ", column.GetColumnName(), column.GetSqlServerParameterName(), i < keyColumns.Count - 1 ? " and " : string.Empty));
                }

                lines.Add(new CodeLine(1, " \"; "));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(1, " Create parameters collection"));
            lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(1, " Add parameters to collection"));

            var columns = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());
            lines.Add(new CommentLine(1, " Execute query in database"));
            lines.Add(new CodeLine(1, "return await connection.ExecuteAsync(new CommandDefinition(query.ToString(), parameters));"));

            lines.Add(new CodeLine("}"));

            return new MethodDefinition("Task<Int32>", table.GetDeleteRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
