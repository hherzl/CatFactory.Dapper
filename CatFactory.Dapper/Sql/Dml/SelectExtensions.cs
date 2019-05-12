using System.Linq;

namespace CatFactory.Dapper.Sql.Dml
{
    public static class SelectExtensions
    {
        internal static Select<TEntity> Where<TEntity>(this Select<TEntity> select, LogicOperator logicOpr, string column, ComparisonOperator comparisonOpr, object value)
        {
            if (!select.Where.Any(item => item.LogicOperator == logicOpr && item.Column == column && item.ComparisonOperator == comparisonOpr && item.Value == value))
                select.Where.Add(new Condition { LogicOperator = logicOpr, Column = column, ComparisonOperator = comparisonOpr, Value = value });

            return select;
        }

        public static Select<TEntity> Where<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOpr, object value)
        {
            if (!select.Where.Any(item => item.LogicOperator == LogicOperator.And && item.Column == column && item.ComparisonOperator == comparisonOpr && item.Value == value))
                select.Where.Add(new Condition { LogicOperator = LogicOperator.And, Column = column, ComparisonOperator = comparisonOpr, Value = value });

            return select;
        }

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, short value)
            => select.Where(LogicOperator.And, column, comparisonOperator, value);

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, int value)
            => select.Where(LogicOperator.And, column, comparisonOperator, value);

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, long value)
            => select.Where(LogicOperator.And, column, comparisonOperator, value);

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, string value)
            => select.Where(LogicOperator.And, column, comparisonOperator, value);

        public static Select<TEntity> Or<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, short value)
            => select.Where(LogicOperator.Or, column, comparisonOperator, value);

        public static Select<TEntity> Or<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, int value)
            => select.Where(LogicOperator.Or, column, comparisonOperator, value);

        public static Select<TEntity> Or<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, long value)
            => select.Where(LogicOperator.Or, column, comparisonOperator, value);

        public static Select<TEntity> Or<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, string value)
            => select.Where(LogicOperator.Or, column, comparisonOperator, value);
    }
}
