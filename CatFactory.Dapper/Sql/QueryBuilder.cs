using System.Linq;
using CatFactory.Dapper.Sql.Dml;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.Dapper.Sql
{
    public static class QueryBuilder
    {
        public static IDatabaseNamingConvention DatabaseNamingConvention;

        static QueryBuilder()
        {
            DatabaseNamingConvention = new DatabaseNamingConvention();
        }

        public static Select<TEntity> Select<TEntity>(string table = null, IDatabaseNamingConvention dbNamingConvention = null)
        {
            var type = typeof(TEntity);

            var query = new Select<TEntity>
            {
                NamingConvention = dbNamingConvention ?? DatabaseNamingConvention,
                From = string.IsNullOrEmpty(table) ? type.Name : table
            };

            foreach (var property in type.GetProperties())
            {
                query.Columns.Add(property.Name);
            }

            return query;
        }

        public static InsertInto<TEntity> InsertInto<TEntity>(TEntity entity, string table = null, string identity = null, IDatabaseNamingConvention dbNamingConvention = null)
        {
            var type = typeof(TEntity);

            var query = new InsertInto<TEntity>
            {
                NamingConvention = dbNamingConvention ?? DatabaseNamingConvention,
                Table = string.IsNullOrEmpty(table) ? type.Name : table
            };

            var properties = type.GetProperties().ToList();

            if (properties.Any(item => item.Name == identity))
                query.Identity = identity;

            foreach (var property in properties)
            {
                if (!string.IsNullOrEmpty(identity) && identity == property.Name)
                    continue;

                var value = property.GetValue(entity);

                query.Columns.Add(new InsertIntoColumn { Name = property.Name, Value = value });
            }

            return query;
        }

        public static Update<TEntity> Update<TEntity>(TEntity entity, string table = null, string key = null, IDatabaseNamingConvention dbNamingConvention = null)
        {
            var type = typeof(TEntity);

            var query = new Update<TEntity>
            {
                NamingConvention = dbNamingConvention ?? DatabaseNamingConvention,
                Table = string.IsNullOrEmpty(table) ? type.Name : table
            };

            var properties = type.GetProperties().ToList();

            if (properties.Any(item => item.Name == key))
                query.Key = key;

            foreach (var property in properties)
            {
                if (!string.IsNullOrEmpty(key) && key == property.Name)
                    continue;

                var value = property.GetValue(entity);

                query.Columns.Add(new UpdateColumn { Name = property.Name, Value = value });
            }

            query.Where.Add(new Condition { Column = key, ComparisonOperator = ComparisonOperator.Equals, Value = type.GetProperty(key).GetValue(entity) });

            return query;
        }

        public static DeleteFrom<TEntity> DeleteFrom<TEntity>(TEntity entity, string schema = null, string table = null, string key = null, IDatabaseNamingConvention dbNamingConvention = null)
        {
            var type = typeof(TEntity);

            var query = new DeleteFrom<TEntity>
            {
                NamingConvention = dbNamingConvention ?? DatabaseNamingConvention,
                Schema = schema,
                Table = string.IsNullOrEmpty(table) ? type.Name : table
            };

            var properties = type.GetProperties().ToList();

            if (properties.Any(item => item.Name == key))
                query.Key = key;

            query.Where.Add(new Condition { Column = key, ComparisonOperator = ComparisonOperator.Equals, Value = type.GetProperty(key).GetValue(entity) });

            return query;
        }
    }
}
