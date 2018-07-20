using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.Mapping;

namespace CatFactory.Dapper.Sql
{
    public class Query
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IDatabaseNamingConvention m_namingConvention;

        public IDatabaseNamingConvention NamingConvention
        {
            get
            {
                return m_namingConvention ?? (m_namingConvention = new DatabaseNamingConvention());
            }
            set
            {
                m_namingConvention = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_headers;

        public List<string> Headers
        {
            get
            {
                return m_headers ?? (m_headers = new List<string>());
            }
            set
            {
                m_headers = value;
            }
        }

        public string Footer { get; set; }
    }
}
