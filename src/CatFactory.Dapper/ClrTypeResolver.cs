//namespace CatFactory.Dapper
//{
//    public class ClrTypeResolver : ITypeResolver
//    {
//        public ClrTypeResolver()
//        {
//        }

//        public bool UseNullableTypes { get; set; } = true;

//        public virtual string Resolve(string type)
//        {
//            var value = string.Empty;

//            switch (type)
//            {
//                case "bit":
//                    value = "Boolean";
//                    break;

//                case "char":
//                case "varchar":
//                case "text":
//                case "nchar":
//                case "nvarchar":
//                case "ntext":
//                    value = "String";
//                    break;

//                case "smallmoney":
//                case "money":
//                case "decimal":
//                case "numeric":
//                    value = "Decimal";
//                    break;

//                case "real":
//                    value = "Single";
//                    break;

//                case "float":
//                    value = "Double";
//                    break;

//                case "tinyint":
//                    value = "Byte";
//                    break;

//                case "image":
//                case "binary":
//                case "rowversion":
//                case "varbinary":
//                case "timestamp":
//                    value = "Byte[]";
//                    break;

//                case "smallint":
//                    value = "Int16";
//                    break;

//                case "int":
//                    value = "Int32";
//                    break;

//                case "bigint":
//                    value = "Int64";
//                    break;

//                case "uniqueidentifier":
//                    value = "Guid";
//                    break;

//                case "xml":
//                    value = "Xml";
//                    break;

//                case "smalldatetime":
//                case "datetime":
//                case "datetime2":
//                    value = "DateTime";
//                    break;

//                case "time":
//                    value = "TimeSpan";
//                    break;

//                default:
//                    value = "Object";
//                    break;
//            }

//            if (string.Compare("BYTE[]", value, true) == 0)
//            {
//                return value;
//            }
//            else if (string.Compare("STRING", value, true) == 0)
//            {
//                return value;
//            }
//            else if (string.Compare("OBJECT", value, true) == 0)
//            {
//                return value;
//            }
//            else
//            {
//                return UseNullableTypes ? string.Format("{0}?", value) : value;
//            }
//        }

//        public virtual string GetDbType(string type)
//        {
//            var value = string.Empty;

//            switch (type)
//            {
//                case "smallint":
//                    value = "Int16";
//                    break;

//                case "int":
//                    value = "Int32";
//                    break;

//                case "bigint":
//                    value = "Int64";
//                    break;
//                default:
//                    value = "Object";
//                    break;
//            }

//            return string.Format("DbType.{0}", value);
//        }
//    }
//}
