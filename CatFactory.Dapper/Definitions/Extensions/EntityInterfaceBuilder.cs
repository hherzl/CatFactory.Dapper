using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.Dapper.Definitions.Extensions
{
    public static class EntityInterfaceBuilder
    {
        public static EntityInterfaceDefinition GetEntityInterfaceDefinition(this DapperProject project)
            => new EntityInterfaceDefinition
            {
                Namespace = project.GetEntityLayerNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = "IEntity"
            };
    }
}
