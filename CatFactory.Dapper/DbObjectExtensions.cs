﻿using System.Linq;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.Dapper
{
    public static class DbObjectExtensions
    {
        public static string GetFullColumnName(this ITable table, Column column)
            => string.Join(".", new string[] { table.Schema, table.Name, column.Name });

        public static bool HasTypeMappedToClr(this Database database, Column column)
        {
            var type = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == column.Type);

            if (type == null)
                return false;

            if (!string.IsNullOrEmpty(type.ParentDatabaseType))
            {
                var parentType = type.GetParentType(database.DatabaseTypeMaps);

                if (parentType == null)
                    return false;
                else
                    return true;
            }

            if (type.GetClrType() != null)
                return true;

            return false;
        }

        public static bool HasTypeMappedToClr(this Database database, Parameter parameter)
        {
            var type = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == parameter.Type);

            if (type == null)
                return false;

            if (!string.IsNullOrEmpty(type.ParentDatabaseType))
            {
                var parentType = type.GetParentType(database.DatabaseTypeMaps);

                if (parentType == null)
                    return false;
                else
                    return true;
            }

            if (type.GetClrType() != null)
                return true;

            return false;
        }

        public static DatabaseTypeMap GetClrMapForType(this Database database, Column column)
        {
            var type = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == column.Type);

            if (type == null)
                return null;

            if (!string.IsNullOrEmpty(type.ParentDatabaseType))
            {
                var parentType = type.GetParentType(database.DatabaseTypeMaps);

                if (parentType == null)
                    return null;
                else
                    return parentType;
            }

            if (type.GetClrType() != null)
                return type;

            return null;
        }

        public static DatabaseTypeMap GetClrMapForType(this Database database, Parameter parameter)
        {
            var type = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == parameter.Type);

            if (type == null)
                return null;

            if (!string.IsNullOrEmpty(type.ParentDatabaseType))
            {
                var parentType = type.GetParentType(database.DatabaseTypeMaps);

                if (parentType == null)
                    return null;
                else
                    return parentType;
            }

            if (type.GetClrType() != null)
                return type;

            return null;
        }
    }
}
