using System.Data;
using System.Linq;
using CatFactory.Dapper.Sql.Dml;

namespace CatFactory.Dapper
{
    public static class SqlQueryBuilder
    {
        public static IDbCommand CreateCommand<TEntity>(this Select<TEntity> query, IDbConnection connection)
        {
            var command = connection.CreateCommand();

            command.CommandText = query.ToString();

            foreach (var condition in query.Where)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = query.NamingConvention.GetParameterName(condition.Column);
                parameter.Value = condition.Value;

                command.Parameters.Add(parameter);
            }

            return command;
        }

        public static IDbCommand CreateCommand<TEntity>(this InsertInto<TEntity> query, IDbConnection connection)
        {
            var command = connection.CreateCommand();

            command.CommandText = query.ToString();

            foreach (var column in query.Columns)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = query.NamingConvention.GetParameterName(column.Name);
                parameter.Value = column.Value;

                command.Parameters.Add(parameter);
            }

            if (!string.IsNullOrEmpty(query.Identity))
            {
                var parameter = command.CreateParameter();

                var type = typeof(TEntity);
                var property = type.GetProperties().First(item => item.Name == query.Identity);

                if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                    parameter.DbType = DbType.Int32;

                parameter.Direction = ParameterDirection.Output;
                parameter.ParameterName = query.NamingConvention.GetParameterName(query.Identity);

                command.Parameters.Add(parameter);
            }

            return command;
        }

        public static IDbCommand CreateCommand<TEntity>(this Update<TEntity> query, IDbConnection connection)
        {
            var command = connection.CreateCommand();

            command.CommandText = query.ToString();

            foreach (var column in query.Columns)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = query.NamingConvention.GetParameterName(column.Name);
                parameter.Value = column.Value;

                command.Parameters.Add(parameter);
            }

            foreach (var condition in query.Where)
            {
                var parameter = command.CreateParameter();

                var type = condition.Value.GetType();

                parameter.ParameterName = query.NamingConvention.GetParameterName(condition.Column);
                parameter.Value = condition.Value;

                command.Parameters.Add(parameter);
            }

            return command;
        }

        public static IDbCommand CreateCommand<TEntity>(this DeleteFrom<TEntity> query, IDbConnection connection)
        {
            var command = connection.CreateCommand();

            command.CommandText = query.ToString();

            foreach (var condition in query.Where)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = query.NamingConvention.GetParameterName(condition.Column);
                parameter.Value = condition.Value;

                command.Parameters.Add(parameter);
            }

            return command;
        }
    }
}
