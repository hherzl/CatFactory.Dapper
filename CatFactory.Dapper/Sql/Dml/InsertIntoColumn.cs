namespace CatFactory.Dapper.Sql.Dml
{
    public class InsertIntoColumn
    {
        public InsertIntoColumn()
        {
        }

        public InsertIntoColumn(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
