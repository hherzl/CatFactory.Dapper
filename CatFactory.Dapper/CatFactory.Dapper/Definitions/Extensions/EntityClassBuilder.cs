using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.NetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class EntityClassBuilder
    {
        public static EntityClassDefinition CreateEntity(this DapperProject project, ITable table)
        {
            var classDefinition = new EntityClassDefinition();

            classDefinition.Namespaces.Add("System");

            var selection = project.GetSelection(table);

            if (selection.Settings.EnableDataBindings)
            {
                classDefinition.Namespaces.Add("System.ComponentModel");

                classDefinition.Implements.Add("INotifyPropertyChanged");

                classDefinition.Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));
            }

            classDefinition.Namespace = table.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema);

            classDefinition.Name = table.GetEntityName();

            classDefinition.Constructors.Add(new ClassConstructorDefinition());

            if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
            {
                var column = table.GetColumnsFromConstraint(table.PrimaryKey).First();

                classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(project.Database.ResolveType(column), column.GetParameterName()))
                {
                    Lines = new List<ILine>
                    {
                        new CodeLine("{0} = {1};", column.GetPropertyName(), column.GetParameterName())
                    }
                });
            }

            if (!string.IsNullOrEmpty(table.Description))
                classDefinition.Documentation.Summary = table.Description;

            foreach (var column in table.Columns)
            {
                if (selection.Settings.EnableDataBindings)
                {
                    classDefinition.AddViewModelProperty(project.Database.ResolveType(column), table.GetPropertyNameHack(column));
                }
                else
                {
                    if (selection.Settings.UseAutomaticPropertiesForEntities)
                        classDefinition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), table.GetPropertyNameHack(column)));
                    else
                        classDefinition.AddPropertyWithField(project.Database.ResolveType(column), table.GetPropertyNameHack(column));
                }
            }

            classDefinition.Implements.Add("IEntity");

            if (selection.Settings.SimplifyDataTypes)
                classDefinition.SimplifyDataTypes();

            return classDefinition;
        }

        public static EntityClassDefinition CreateView(this DapperProject project, IView view)
        {
            var definition = new EntityClassDefinition();

            definition.Namespaces.Add("System");

            definition.Namespace = view.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(view.Schema);

            definition.Name = view.GetEntityName();

            definition.Constructors.Add(new ClassConstructorDefinition());

            if (!string.IsNullOrEmpty(view.Description))
                definition.Documentation.Summary = view.Description;

            var selection = project.GetSelection(view);

            foreach (var column in view.Columns)
            {
                if (selection.Settings.UseAutomaticPropertiesForEntities)
                    definition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), view.GetPropertyNameHack(column)));
                else
                    definition.AddPropertyWithField(project.Database.ResolveType(column), view.GetPropertyNameHack(column));
            }

            definition.Implements.Add("IEntity");

            if (selection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            return definition;
        }

        public static EntityClassDefinition CreateView(this DapperProject project, TableFunction tableFunction)
        {
            var definition = new EntityClassDefinition();

            definition.Namespaces.Add("System");

            definition.Namespace = tableFunction.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(tableFunction.Schema);

            definition.Name = tableFunction.GetEntityName();

            definition.Constructors.Add(new ClassConstructorDefinition());

            if (!string.IsNullOrEmpty(tableFunction.Description))
                definition.Documentation.Summary = tableFunction.Description;

            var selection = project.GetSelection(tableFunction);

            foreach (var column in tableFunction.Columns)
            {
                definition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), column.GetPropertyName()));

                //if (selection.Settings.UseAutomaticPropertiesForEntities)
                //    definition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), tableFunction.GetPropertyNameHack(column)));
                //else
                //    definition.AddPropertyWithField(project.Database.ResolveType(column), tableFunction.GetPropertyNameHack(column));
            }

            definition.Implements.Add("IEntity");

            if (selection.Settings.SimplifyDataTypes)
                definition.SimplifyDataTypes();

            return definition;
        }
    }
}
