using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.Mapping;
using CatFactory.OOP;

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
                    "Dapper",
                    "Microsoft.Extensions.Options"
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
                    new ClassConstructorDefinition(new ParameterDefinition("IOptions<AppSettings>", "appSettings"))
                    {
                        Invocation = "base(appSettings)"
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
            var views = db.Views.Where(v => dbos.Contains(v.FullName)).ToList();
            var tableFunctions = db.TableFunctions.Where(tf => dbos.Contains(tf.FullName)).ToList();
            var scalarFunctions = db.ScalarFunctions.Where(sf => dbos.Contains(sf.FullName)).ToList();

            foreach (var table in tables)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(table);

                if (selection.Settings.DeclareConnectionAsParameter)
                {
                    classDefinition.Methods.Add(GetGetAllMethodWithConnectionAsParameter(projectFeature, table));

                    if (table.PrimaryKey != null)
                        classDefinition.Methods.Add(GetGetMethodAsParameter(projectFeature, table));

                    foreach (var unique in table.Uniques)
                        classDefinition.Methods.Add(GetByUniqueMethodAsParameter(projectFeature, table, unique));

                    classDefinition.Methods.Add(GetAddMethodAsParameter(projectFeature, table));

                    if (table.PrimaryKey != null)
                    {
                        classDefinition.Methods.Add(GetUpdateMethodAsParameter(projectFeature, table));
                        classDefinition.Methods.Add(GetRemoveMethodAsParameter(projectFeature, table));
                    }
                }
                else
                {
                    classDefinition.Methods.Add(GetGetAllMethodWithConnectionAsLocal(projectFeature, table));

                    if (table.PrimaryKey != null)
                        classDefinition.Methods.Add(GetGetMethodAsLocal(projectFeature, table));

                    foreach (var unique in table.Uniques)
                        classDefinition.Methods.Add(GetByUniqueMethodAsLocal(projectFeature, table, unique));

                    classDefinition.Methods.Add(GetAddMethodAsLocal(projectFeature, table));

                    if (table.PrimaryKey != null)
                    {
                        classDefinition.Methods.Add(GetUpdateMethodAsLocal(projectFeature, table));
                        classDefinition.Methods.Add(GetRemoveMethodAsLocal(projectFeature, table));
                    }
                }
            }

            foreach (var view in views)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(view);

                if (selection.Settings.DeclareConnectionAsParameter)
                    classDefinition.Methods.Add(GetGetAllMethodWithConnectionAsParameter(projectFeature, view));
                else
                    classDefinition.Methods.Add(GetGetAllMethodWithConnectionAsLocal(projectFeature, view));
            }

            foreach (var scalarFunction in scalarFunctions)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(scalarFunction);

                if (selection.Settings.DeclareConnectionAsParameter)
                    classDefinition.Methods.Add(GetGetAllMethodAsParameter(projectFeature, scalarFunction));
                else
                    classDefinition.Methods.Add(GetGetAllMethodAsLocal(projectFeature, scalarFunction));
            }

            foreach (var tableFunction in tableFunctions)
            {
                var selection = projectFeature.GetDapperProject().GetSelection(tableFunction);

                if (selection.Settings.DeclareConnectionAsParameter)
                    classDefinition.Methods.Add(GetGetAllMethodAsParameter(projectFeature, tableFunction));
                else
                    classDefinition.Methods.Add(GetGetAllMethodAsLocal(projectFeature, tableFunction));
            }

            return classDefinition;
        }
    }
}
