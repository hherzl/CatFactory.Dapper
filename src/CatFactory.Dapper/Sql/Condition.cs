namespace CatFactory.Dapper.Sql
{
    public class Condition
    {
        public LogicOperator LogicOperator { get; set; }

        public string Column { get; set; }

        public ComparisonOperator ComparisonOperator { get; set; }

        public object Value { get; set; }
    }
}
