using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.Dapper.Sql
{
    public class Query : IQuery
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IDatabaseNamingConvention m_namingConvention;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_headers;

        public Query()
        {
        }

        public IDatabaseNamingConvention NamingConvention
        {
            get => m_namingConvention ?? (m_namingConvention = new DatabaseNamingConvention());
            set => m_namingConvention = value;
        }

        public List<string> Headers
        {
            get => m_headers ?? (m_headers = new List<string>());
            set => m_headers = value;
        }

        public string Footer { get; set; }
    }
}
