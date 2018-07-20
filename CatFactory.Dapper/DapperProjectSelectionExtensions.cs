using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class DapperProjectSelectionExtensions
    {
        public static ProjectSelection<DapperProjectSettings> GetSelection(this DapperProject project, IDbObject dbObject)
        {
            // Searching by full name: Sales.Order
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == dbObject.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Searching by schema name: Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", dbObject.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // Searching by name: *.Order
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", dbObject.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }
    }
}
