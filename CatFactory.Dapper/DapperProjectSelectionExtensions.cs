using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.Dapper
{
    public static class DapperProjectSelectionExtensions
    {
        public static ProjectSelection<DapperProjectSettings> GetSelection(this DapperProject project, IDbObject dbObj)
        {
            // Searching by full name: Sales.OrderHeader
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == dbObj.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Searching by schema name: Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", dbObj.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // Searching by name: *.OrderHeader
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", dbObj.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }
    }
}
