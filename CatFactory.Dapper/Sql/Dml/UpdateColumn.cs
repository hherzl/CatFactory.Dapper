namespace CatFactory.Dapper.Sql.Dml
{
    public class UpdateColumn
    {
        public UpdateColumn()
        {
        }

        public UpdateColumn(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
