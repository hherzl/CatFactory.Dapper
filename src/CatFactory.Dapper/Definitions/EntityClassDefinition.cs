using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper.Definitions
{
    public static class EntityClassDefinition
    {
        public static CSharpClassDefinition CreateEntity(this DapperProject project, ITable table)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");

            if (project.Settings.EnableDataBindings)
            {
                classDefinition.Namespaces.Add("System.ComponentModel");

                classDefinition.Implements.Add("INotifyPropertyChanged");

                classDefinition.Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));
            }

            classDefinition.Namespace = table.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema);

            classDefinition.Name = table.GetSingularName();

            classDefinition.Constructors.Add(new ClassConstructorDefinition());

            if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
            {
                var column = table.GetColumnsFromConstraint(table.PrimaryKey).First();

                classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(project.Database.ResolveType(column), column.GetParameterName()))
                {
                    Lines = new List<ILine>()
                    {
                        new CodeLine("{0} = {1};", column.GetPropertyName(), column.GetParameterName())
                    }
                });
            }

            if (!string.IsNullOrEmpty(table.Description))
            {
                classDefinition.Documentation.Summary = table.Description;
            }

            foreach (var column in table.Columns)
            {
                if (project.Settings.EnableDataBindings)
                {
                    classDefinition.AddViewModelProperty(project.Database.ResolveType(column), column.GetPropertyName());
                }
                else
                {
                    if (project.Settings.UseAutomaticPropertiesForEntities)
                    {
                        classDefinition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), column.GetPropertyName()));
                    }
                    else
                    {
                        classDefinition.AddPropertyWithField(project.Database.ResolveType(column), column.GetPropertyName());
                    }
                }
            }

            classDefinition.Implements.Add("IEntity");

            if (project.Settings.SimplifyDataTypes)
            {
                classDefinition.SimplifyDataTypes();
            }

            return classDefinition;
        }

        public static CSharpClassDefinition CreateView(this DapperProject project, IView view)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");

            definition.Namespace = view.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(view.Schema);

            definition.Name = view.GetSingularName();

            definition.Constructors.Add(new ClassConstructorDefinition());

            if (!string.IsNullOrEmpty(view.Description))
            {
                definition.Documentation.Summary = view.Description;
            }

            foreach (var column in view.Columns)
            {
                if (project.Settings.UseAutomaticPropertiesForEntities)
                {
                    definition.Properties.Add(new PropertyDefinition(project.Database.ResolveType(column), column.GetPropertyName()));
                }
                else
                {
                    definition.AddPropertyWithField(project.Database.ResolveType(column), column.GetPropertyName());
                }
            }

            definition.Implements.Add("IEntity");

            if (project.Settings.SimplifyDataTypes)
            {
                definition.SimplifyDataTypes();
            }

            return definition;
        }
    }
}
