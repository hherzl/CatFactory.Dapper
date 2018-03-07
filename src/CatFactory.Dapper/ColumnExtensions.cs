using System.Text.RegularExpressions;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public static class ColumnExtensions
    {
        public static string GetPropertyNameHack(this ITable table, Column column)
        {
            var propertyName = column.HasSameNameEnclosingType(table) ? column.GetNameForEnclosing() : column.GetPropertyName();

            var regex = new Regex(@"^[0-9]+$");

            if (regex.IsMatch(propertyName))
                propertyName = string.Format("V{0}", propertyName);

            return propertyName;
        }

        public static string GetPropertyNameHack(this IView view, Column column)
        {
            var propertyName = column.HasSameNameEnclosingType(view) ? column.GetNameForEnclosing() : column.GetPropertyName();

            var regex = new Regex(@"^[0-9]+$");

            if (regex.IsMatch(propertyName))
                propertyName = string.Format("V{0}", propertyName);

            return propertyName;
        }

        public static string GetColumnName(this ITable table, Column column)
            => string.Format("[{0}].[{1}].[{2}]", table.Schema, table.Name, column.Name);

        public static string GetColumnName(this IView view, Column column)
            => string.Format("[{0}].[{1}].[{2}]", view.Schema, view.Name, column.Name);

        public static string GetColumnName(this TableFunction tableFunction, Column column)
            => string.Format("[{0}].[{1}].[{2}]", tableFunction.Schema, tableFunction.Name, column.Name);

        public static string GetColumnName(this Column column)
            => string.Format("[{0}]", column.Name);

        public static string GetSqlServerParameterName(this Column column)
            => string.Format("@{0}", column.GetParameterName());

        public static string GetSqlServerParameterName(this Parameter param)
            => string.Format("@{0}", NamingConvention.GetCamelCase(param.Name));

        public static bool HasSameNameEnclosingType(this Column column, ITable table)
            => column.Name == table.Name;

        public static bool HasSameNameEnclosingType(this Column column, IView view)
            => column.Name == view.Name;

        public static string GetNameForEnclosing(this Column column)
            => string.Format("{0}1", column.Name);
    }
}
