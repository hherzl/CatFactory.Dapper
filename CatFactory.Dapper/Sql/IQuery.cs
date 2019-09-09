using System.Collections.Generic;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.Dapper.Sql
{
    public interface IQuery
    {
        IDatabaseNamingConvention NamingConvention { get; set; }

        List<string> Headers { get; set; }

        string Footer { get; set; }
    }
}
