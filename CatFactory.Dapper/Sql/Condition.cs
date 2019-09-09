namespace CatFactory.Dapper.Sql
{
    public class Condition
    {
        public Condition()
        {
        }

        public Condition(string column, ComparisonOperator comparisonOperator, object value)
        {
            Column = column;
            ComparisonOperator = comparisonOperator;
            Value = value;
        }

        public Condition(LogicOperator logicOperator, string column, ComparisonOperator comparisonOperator, object value)
        {
            LogicOperator = logicOperator;
            Column = column;
            ComparisonOperator = comparisonOperator;
            Value = value;
        }

        public LogicOperator LogicOperator { get; set; }

        public string Column { get; set; }

        public ComparisonOperator ComparisonOperator { get; set; }

        public object Value { get; set; }
    }
}
