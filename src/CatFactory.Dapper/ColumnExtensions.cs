using CatFactory.Mapping;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class ColumnExtensions
    {
        public static string GetColumnName(this ITable table, Column column)
            => string.Format("[{0}].[{1}].[{2}]", table.Schema, table.Name, column.Name);

        public static string GetColumnName(this IView view, Column column)
            => string.Format("[{0}].[{1}].[{2}]", view.Schema, view.Name, column.Name);

        public static string GetColumnName(this Column column)
            => string.Format("[{0}]", column.Name);

        public static string GetSqlServerParameterName(this Column column)
            => string.Format("@{0}", column.GetParameterName());
    }
}
