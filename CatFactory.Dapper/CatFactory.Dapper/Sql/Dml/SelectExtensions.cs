using System.Linq;

namespace CatFactory.Dapper.Sql.Dml
{
    public static class SelectExtensions
    {
        internal static Select<TEntity> Where<TEntity>(this Select<TEntity> select, LogicOperator logicOperator, string column, ComparisonOperator comparisonOperator, object value)
        {
            if (!select.Where.Any(item => item.LogicOperator == logicOperator && item.Column == column && item.ComparisonOperator == comparisonOperator && item.Value == value))
            {
                select.Where.Add(new Condition { LogicOperator = logicOperator, Column = column, ComparisonOperator = comparisonOperator, Value = value });
            }

            return select;
        }

        public static Select<TEntity> Where<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, object value)
        {
            if (!select.Where.Any(item => item.LogicOperator == LogicOperator.And && item.Column == column && item.ComparisonOperator == comparisonOperator && item.Value == value))
            {
                select.Where.Add(new Condition { LogicOperator = LogicOperator.And, Column = column, ComparisonOperator = comparisonOperator, Value = value });
            }

            return select;
        }

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, short value)
        {
            return select.Where(LogicOperator.And, column, comparisonOperator, value);
        }

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, int value)
        {
            return select.Where(LogicOperator.And, column, comparisonOperator, value);
        }

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, long value)
        {
            return select.Where(LogicOperator.And, column, comparisonOperator, value);
        }

        public static Select<TEntity> And<TEntity>(this Select<TEntity> select, string column, ComparisonOperator comparisonOperator, string value)
        {
            return select.Where(LogicOperator.And, column, comparisonOperator, value);
        }
    }
}
