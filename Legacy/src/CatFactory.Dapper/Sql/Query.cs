using System.Collections.Generic;
using CatFactory.Mapping;

namespace CatFactory.Dapper.Sql
{
    public class Query
    {
        public IDatabaseNamingConvention NamingConvention { get; set; } = new DatabaseNamingConvention();

        public List<string> Headers { get; set; } = new List<string>();

        public string Footer { get; set; }
    }
}
