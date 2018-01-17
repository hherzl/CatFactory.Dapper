using System.Linq;
using System.Reflection;
using CatFactory.Dapper.Sql.Dml;

namespace CatFactory.Dapper.Sql
{
    public class QueryBuilder
    {
        public static Select<TEntity> Select<TEntity>(string table = null)
        {
            var query = new Select<TEntity>();

            var type = typeof(TEntity);

            foreach (var property in type.GetProperties())
            {
                query.Columns.Add(property.Name);
            }

            query.From = string.IsNullOrEmpty(table) ? type.Name : table;

            return query;
        }

        public static InsertInto<TEntity> InsertInto<TEntity>(string table = null, string identity = null)
        {
            var query = new InsertInto<TEntity>();

            var type = typeof(TEntity);

            query.Table = string.IsNullOrEmpty(table) ? type.Name : table;

            var properties = type.GetProperties().ToList();

            if (properties.Any(item => item.Name == identity))
            {
                query.Identity = identity;
            }

            foreach (var property in properties)
            {
                if (!string.IsNullOrEmpty(identity) && identity == property.Name)
                {
                    continue;
                }

                query.Columns.Add(property.Name);
            }

            if (!string.IsNullOrEmpty(query.Identity))
            {
                query.Footer = identity;
            }

            return query;
        }

        public static Update<TEntity> Update<TEntity>(string key)
        {
            var query = new Update<TEntity>();

            var type = typeof(TEntity);

            query.Table = type.Name;

            var properties = type.GetProperties().ToList();

            if (properties.Any(item => item.Name == key))
            {
                query.Key = key;
            }

            foreach (var property in properties)
            {
                if (!string.IsNullOrEmpty(key) && key == property.Name)
                {
                    continue;
                }

                query.Columns.Add(property.Name);
            }

            query.Where.Add(new Condition { Column = key, ComparisonOperator = ComparisonOperator.Equals, Value = key });

            return query;
        }

        public static DeleteFrom<TEntity> DeleteFrom<TEntity>(string key)
        {
            var query = new DeleteFrom<TEntity>();

            var type = typeof(TEntity);

            query.Table = type.Name;

            var properties = type.GetProperties().ToList();

            if (properties.Any(item => item.Name == key))
            {
                query.Key = key;
            }

            query.Where.Add(new Condition { Column = key, ComparisonOperator = ComparisonOperator.Equals, Value = key });

            return query;
        }
    }
}
