using CatFactory.Mapping;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class ColumnExtensions
    {
        public static string GetColumnName(this Column column)
            => string.Format("[{0}]", column.Name);

        public static string GetSqlServerParameterName(this Column column)
            => string.Format("@{0}", column.GetParameterName());
    }
}
