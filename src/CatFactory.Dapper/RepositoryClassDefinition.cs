using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper
{
    public class RepositoryClassDefinition : CSharpClassDefinition
    {
        public RepositoryClassDefinition(ProjectFeature projectFeature)
            : base()
        {
            ProjectFeature = projectFeature;

            Init();
        }

        public ProjectFeature ProjectFeature { get; }

        public void Init()
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");
            Namespaces.Add("System.Data");
            Namespaces.Add("System.Data.SqlClient");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Text");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Dapper");
            Namespaces.Add("Microsoft.Extensions.Options");

            foreach (var dbObject in ProjectFeature.DbObjects)
            {
                var table = ProjectFeature.Project.Database.Tables.FirstOrDefault(item => item.FullName == dbObject.FullName);

                if (table == null)
                {
                    continue;
                }

                if (table.HasDefaultSchema())
                {
                    Namespaces.AddUnique(ProjectFeature.Project.GetEntityLayerNamespace());
                }
                else
                {
                    Namespaces.AddUnique(ProjectFeature.GetDapperProject().GetEntityLayerNamespace(table.Schema));
                }

                Namespaces.AddUnique(ProjectFeature.GetDapperProject().GetDataLayerContractsNamespace());
            }

            Namespace = ProjectFeature.GetDapperProject().GetDataLayerRepositoriesNamespace();

            Name = ProjectFeature.GetClassRepositoryName();

            BaseClass = "Repository";

            Implements.Add(ProjectFeature.GetInterfaceRepositoryName());

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
            {
                ParentInvoke = "base(appSettings)"
            });

            var dbos = ProjectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = ProjectFeature.Project.Database.Tables.Where(t => dbos.Contains(t.FullName)).ToList();

            foreach (var table in tables)
            {
                if (!Namespaces.Contains(ProjectFeature.GetDapperProject().GetDataLayerDataContractsNamespace()))
                {
                    //if (ProjectFeature.GetEfCoreProject().Settings.EntitiesWithDataContracts.Contains(table.FullName) && !Namespaces.Contains(ProjectFeature.GetEfCoreProject().GetDataLayerDataContractsNamespace()))
                    //{
                    //    Namespaces.Add(ProjectFeature.GetEfCoreProject().GetDataLayerDataContractsNamespace());
                    //}
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (String.IsNullOrEmpty(foreignKey.Child))
                    {
                        var child = ProjectFeature.Project.Database.FindTableBySchemaAndName(foreignKey.Child);

                        if (child != null)
                        {
                            Namespaces.AddUnique(ProjectFeature.GetDapperProject().GetDataLayerDataContractsNamespace());
                        }
                    }
                }

                Methods.Add(GetGetAllMethod(ProjectFeature, table));

                //AddGetByUniqueMethods(ProjectFeature, table);

                Methods.Add(GetGetMethod(ProjectFeature, table));
                Methods.Add(GetAddMethod(ProjectFeature, table));
                Methods.Add(GetUpdateMethod(ProjectFeature, table));
                Methods.Add(GetRemoveMethod(ProjectFeature, table));
            }
        }

        public MethodDefinition GetGetAllMethod(ProjectFeature projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "query.Append(\" select \");"));

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.Name, i < table.Columns.Count - 1 ? "," : String.Empty));
            }

            lines.Add(new CodeLine(1, "query.Append(\" from \");"));
            lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.FullName));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "return await connection.QueryAsync<{0}>(query.ToString());", table.GetEntityName()));
            lines.Add(new CodeLine("}"));

            return new MethodDefinition(String.Format("Task<IEnumerable<{0}>>", table.GetEntityName()), table.GetGetAllRepositoryMethodName())
            {
                IsAsync = true,
                Lines = lines
            };
        }

        //public void AddGetByUniqueMethods(ProjectFeature projectFeature, IDbObject dbObject)
        //{
        //    var table = dbObject as ITable;

        //    if (table == null)
        //    {
        //        table = projectFeature.Project.Database.FindTableBySchemaAndName(dbObject.FullName);
        //    }

        //    if (table == null)
        //    {
        //        return;
        //    }

        //    foreach (var unique in table.Uniques)
        //    {
        //        var expression = String.Format("item => {0}", String.Join(" && ", unique.Key.Select(item => String.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));

        //        Methods.Add(new MethodDefinition(String.Format("Task<{0}>", dbObject.GetSingularName()), dbObject.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
        //        {
        //            IsAsync = true,
        //            Lines = new List<ILine>()
        //            {
        //                new CommentLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", dbObject.GetPluralName(), expression)
        //            }
        //        });
        //    }
        //}

        public MethodDefinition GetGetMethod(ProjectFeature projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "query.Append(\" select \");"));

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.Name, i < table.Columns.Count - 1 ? "," : String.Empty));
            }

            lines.Add(new CodeLine(1, "query.Append(\" from \");"));
            lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.FullName));

            lines.Add(new CodeLine(1, "query.Append(\" where \");"));

            if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
            {
                var column = table.PrimaryKey.GetColumns(table).First();

                lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1} \");", column.Name, column.GetParameterName()));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "var parameters = new"));
                lines.Add(new CodeLine(1, "{"));
                lines.Add(new CodeLine(2, "{0} = entity.{1}", column.GetParameterName(), column.GetPropertyName()));
                lines.Add(new CodeLine(1, "};"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "return await connection.QueryFirstOrDefaultAsync<{0}>(query.ToString(), parameters);", table.GetEntityName()));
                lines.Add(new CodeLine("}"));
            }

            return new MethodDefinition(String.Format("Task<{0}>", table.GetSingularName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetAddMethod(ProjectFeature projectFeature, Table table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "query.Append(\" insert into \");"));
            lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.FullName));
            lines.Add(new CodeLine(1, "query.Append(\"  ( \");"));

            var insertColumns = table.GetColumnsWithOutIdentity().ToList();

            for (var i = 0; i < insertColumns.Count(); i++)
            {
                var column = insertColumns[i];

                lines.Add(new CodeLine(1, "query.Append(\"   {0}{1} \");", column.Name, i < insertColumns.Count - 1 ? "," : String.Empty));
            }

            lines.Add(new CodeLine(1, "query.Append(\"  ) \");"));
            lines.Add(new CodeLine(1, "query.Append(\" values \");"));
            lines.Add(new CodeLine(1, "query.Append(\" ( \");"));

            for (var i = 0; i < insertColumns.Count(); i++)
            {
                var column = insertColumns[i];

                lines.Add(new CodeLine(1, "query.Append(\"  {0}{1} \");", column.GetParameterName(), i < insertColumns.Count - 1 ? "," : String.Empty));
            }

            lines.Add(new CodeLine(1, "query.Append(\" ) \");"));

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());

            if (table.Identity == null)
            {
                var columns = table.Columns;

                for (var i = 0; i < columns.Count(); i++)
                {
                    var column = columns[i];

                    lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
                }

                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "return await connection.ExecuteAsync(query.ToString(), parameters);"));

                lines.Add(new CodeLine("}"));
            }
            else
            {
                var columns = table.GetColumnsWithOutIdentity().ToList();

                for (var i = 0; i < columns.Count(); i++)
                {
                    var c = columns[i];

                    lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", c.GetParameterName(), c.GetPropertyName()));
                }

                // todo: add logic to retrieve the identity column
                var identityColumn = table.Columns[0];

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", dbType: {1}, direction: ParameterDirection.Output);", identityColumn.GetParameterName(), new ClrTypeResolver().GetDbType(identityColumn.Type)));

                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "var affectedRows = await connection.ExecuteAsync(query.ToString(), parameters);"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "entity.{0} = parameters.Get<{1}>(\"{2}\");", identityColumn.GetPropertyName(), new ClrTypeResolver().Resolve(identityColumn.Type), identityColumn.GetParameterName()));
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

        public MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, Table table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "query.Append(\" update \");"));
            lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.FullName));
            lines.Add(new CodeLine(1, "query.Append(\" set \");"));

            var updateColumns = table.GetColumnsWithOutKey().ToList();

            for (var i = 0; i < updateColumns.Count(); i++)
            {
                var column = updateColumns[i];

                lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1} \");", column.Name, column.GetParameterName(), i < updateColumns.Count - 1 ? "," : String.Empty));
            }

            lines.Add(new CodeLine(1, "query.Append(\" where \");"));

            var key = table.PrimaryKey.GetColumns(table).ToList();

            for (var i = 0; i < key.Count(); i++)
            {
                var column = key[i];

                lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1}{2} \");", column.Name, column.GetParameterName(), i < key.Count - 1 ? " and " : String.Empty));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            
            var columns = table.GetColumnsWithOutKey().ToList();

            for (var i = 0; i < columns.Count(); i++)
            {
                var c = columns[i];

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", c.GetParameterName(), c.GetPropertyName()));
            }

            // todo: add logic to retrieve the identity column
            var identityColumn = table.Columns[0];

            lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", identityColumn.GetParameterName(), identityColumn.GetPropertyName()));

            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "return await connection.ExecuteAsync(query.ToString(), parameters);"));
            
            lines.Add(new CodeLine("}"));
            
            return new MethodDefinition("Task<Int32>", table.GetUpdateRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetRemoveMethod(ProjectFeature projectFeature, Table table)
        {
            var lines = new List<ILine>();

            lines.Add(new CodeLine("using (var connection = new SqlConnection(ConnectionString))"));
            lines.Add(new CodeLine("{"));
            lines.Add(new CodeLine(1, "var query = new StringBuilder();"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "query.Append(\" delete from \");"));
            lines.Add(new CodeLine(1, "query.Append(\"  {0} \");", table.FullName));
            lines.Add(new CodeLine(1, "query.Append(\" where \");"));

            var key = table.PrimaryKey.GetColumns(table).ToList();

            for (var i = 0; i < key.Count(); i++)
            {
                var column = key[i];

                lines.Add(new CodeLine(1, "query.Append(\"  {0} = {1}{2} \");", column.Name, column.GetParameterName(), i < key.Count - 1 ? " and " : String.Empty));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "var parameters = new DynamicParameters();"));

            lines.Add(new CodeLine());
            
            var columns = table.PrimaryKey.GetColumns(table).ToList();

            for (var i = 0; i < columns.Count(); i++)
            {
                var column = columns[i];

                lines.Add(new CodeLine(1, "parameters.Add(\"{0}\", entity.{1});", column.GetParameterName(), column.GetPropertyName()));
            }

            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "return await connection.ExecuteAsync(query.ToString(), parameters);"));
            
            lines.Add(new CodeLine("}"));
            
            return new MethodDefinition("Task<Int32>", table.GetRemoveRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
