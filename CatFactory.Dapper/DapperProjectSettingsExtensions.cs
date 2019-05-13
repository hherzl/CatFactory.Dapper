using System.Linq;
using CatFactory.ObjectRelationalMapping.Actions;

namespace CatFactory.Dapper
{
    public static class DapperProjectSettingsExtensions
    {
        public static DapperProjectSettings RemoveAction<TAction>(this DapperProjectSettings settings) where TAction : IEntityAction
        {
            settings.Actions.Remove(settings.Actions.First(item => item is TAction));

            return settings;
        }
    }
}
