using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class EntityClassBuilder
    {
        public static EntityClassDefinition GetEntityClassDefinition(this DapperProject project, ITable table)
        {
            var definition = new EntityClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.Database.HasDefaultSchema(table) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityName(table),
                Constructors =
                {
                    new ClassConstructorDefinition
                    {
                        AccessModifier = AccessModifier.Public
                    }
                }
            };

            var selection = project.GetSelection(table);

            if (selection.Settings.EnableDataBindings)
            {
                definition.Namespaces.Add("System.ComponentModel");
                definition.Implements.Add("INotifyPropertyChanged");

                definition.Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged")
                {
                    AccessModifier = AccessModifier.Public
                });
            }

            if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
            {
                var column = table.GetColumnsFromConstraint(table.PrimaryKey).First();

                definition.Constructors.Add(new ClassConstructorDefinition(AccessModifier.Public, new ParameterDefinition(project.Database.ResolveDatabaseType(column), project.GetParameterName(column)))
                {
                    Lines =
                    {
                        new CodeLine("{0} = {1};", project.GetPropertyName(table, column), project.GetParameterName(column))
                    }
                });
            }

            if (!string.IsNullOrEmpty(table.Description))
                definition.Documentation.Summary = table.Description;

            foreach (var column in table.Columns)
            {
                if (selection.Settings.EnableDataBindings)
                {
                    definition.AddViewModelProperty(project.Database.ResolveDatabaseType(column), project.GetPropertyName(table, column));
                }
                else
                {
                    if (selection.Settings.UseAutomaticPropertiesForEntities)
                        definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropertyName(table, column))
                        {
                            IsAutomatic = true
                        });
                    else
                        definition.AddPropertyWithField(project.Database.ResolveDatabaseType(column), project.GetPropertyName(table, column));
                }
            }

            definition.Implements.Add("IEntity");

            if (selection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            return definition;
        }

        public static EntityClassDefinition GetEntityClassDefinition(this DapperProject project, IView view)
        {
            var definition = new EntityClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.Database.HasDefaultSchema(view) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(view.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityName(view)
            };

            definition.Constructors.Add(new ClassConstructorDefinition(AccessModifier.Public));

            if (!string.IsNullOrEmpty(view.Description))
                definition.Documentation.Summary = view.Description;

            var selection = project.GetSelection(view);

            foreach (var column in view.Columns)
            {
                if (selection.Settings.UseAutomaticPropertiesForEntities)
                    definition.Properties.Add(new PropertyDefinition(project.Database.ResolveDatabaseType(column), project.GetPropertyName(view, column))
                    {
                        IsAutomatic = true
                    });
                else
                    definition.AddPropertyWithField(project.Database.ResolveDatabaseType(column), project.GetPropertyName(view, column));
            }

            definition.Implements.Add("IEntity");

            if (selection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            return definition;
        }

        public static EntityClassDefinition GetEntityClassDefinition(this DapperProject project, ScalarFunction scalarFunction)
        {
            var definition = new EntityClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.Database.HasDefaultSchema(scalarFunction) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(scalarFunction.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetEntityName(scalarFunction),
                Constructors =
                {
                    new ClassConstructorDefinition(AccessModifier.Public)
                }
            };

            if (!string.IsNullOrEmpty(scalarFunction.Description))
                definition.Documentation.Summary = scalarFunction.Description;

            var selection = project.GetSelection(scalarFunction);

            definition.Implements.Add("IEntity");

            if (selection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            return definition;
        }

        public static EntityClassDefinition GetEntityClassDefinition(this DapperProject project, ITableFunction tableFunction)
        {
            var definition = new EntityClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.Database.HasDefaultSchema(tableFunction) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(tableFunction.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetResultName(tableFunction),
                Constructors =
                {
                    new ClassConstructorDefinition(AccessModifier.Public)
                }
            };

            if (!string.IsNullOrEmpty(tableFunction.Description))
                definition.Documentation.Summary = tableFunction.Description;

            var selection = project.GetSelection(tableFunction);

            foreach (var column in tableFunction.Columns)
            {
                definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropertyName(column.Name))
                {
                    IsAutomatic = true
                });
            }

            definition.Implements.Add("IEntity");

            if (selection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            return definition;
        }

        public static EntityClassDefinition GetEntityClassDefinition(this DapperProject project, StoredProcedure storedProcedure)
        {
            var definition = new EntityClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.Database.HasDefaultSchema(storedProcedure) ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(storedProcedure.Schema),
                AccessModifier = AccessModifier.Public,
                Name = project.GetResultName(storedProcedure),
                Constructors =
                {
                    new ClassConstructorDefinition(AccessModifier.Public)
                }
            };

            foreach (var resultSet in storedProcedure.FirstResultSetsForObject)
            {
                var type = project.Database.ResolveDatabaseType(resultSet.SystemTypeName);

                definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, type, resultSet.Name)
                {
                    IsAutomatic = true
                });
            }

            return definition;
        }
    }
}
