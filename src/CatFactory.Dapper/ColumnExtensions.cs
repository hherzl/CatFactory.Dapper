﻿using System;
using CatFactory.Mapping;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class ColumnExtensions
    {
        public static Boolean IsDecimal(this Column column)
        {
            switch (column.Type)
            {
                case "decimal":
                    return true;

                default:
                    return false;
            }
        }

        public static Boolean IsDouble(this Column column)
        {
            switch (column.Type)
            {
                case "float":
                    return true;

                default:
                    return false;
            }
        }

        public static Boolean IsSingle(this Column column)
        {
            switch (column.Type)
            {
                case "real":
                    return true;

                default:
                    return false;
            }
        }

        public static Boolean IsString(this Column column)
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

        public static String GetColumnName(this Column column)
            => String.Format("[{0}]", column.Name);

        public static String GetSqlServerParameterName(this Column column)
            => String.Format("@{0}", column.GetParameterName());
    }
}