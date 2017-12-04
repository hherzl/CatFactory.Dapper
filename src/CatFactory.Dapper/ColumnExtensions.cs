using CatFactory.Mapping;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class ColumnExtensions
    {
        public static bool IsDecimal(this Column column)
        {
            switch (column.Type)
            {
                case "decimal":
                case "money":
                case "smallmoney":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsDouble(this Column column)
        {
            switch (column.Type)
            {
                case "float":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsSingle(this Column column)
        {
            switch (column.Type)
            {
                case "real":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsString(this Column column)
        {
            switch (column.Type)
            {
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    return true;

                default:
                    return false;
            }
        }

        public static string GetColumnName(this Column column)
            => string.Format("[{0}]", column.Name);

        public static string GetSqlServerParameterName(this Column column)
            => string.Format("@{0}", column.GetParameterName());
    }
}
