using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CatFactory.Dapper.Sql.Dml
{
    public class InsertInto<TEntity> : Query
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<InsertIntoColumn> m_columns;

        public InsertInto()
            : base()
        {
        }

        public string Table { get; set; }

        public string Identity { get; set; }

        public List<InsertIntoColumn> Columns
        {
            get => m_columns ?? (m_columns = new List<InsertIntoColumn>());
            set => m_columns = value;
        }

        public override string ToString()
        {
            var output = new StringBuilder();

            for (var i = 0; i < Headers.Count; i++)
            {
                output.AppendFormat("{0}", Headers[i]);
                output.AppendLine();
            }

            output.AppendFormat("insert into {0} ", Table);
            output.AppendLine();

            var columns = string.IsNullOrEmpty(Identity) ? Columns : Columns.Where(item => item.Name != Identity).ToList();

            output.Append("(");
            output.AppendLine();

            for (var i = 0; i < columns.Count; i++)
            {
                var item = columns[i];

                output.AppendFormat(" {0}{1}", NamingConvention.GetObjectName(item.Name), i < columns.Count - 1 ? ", " : string.Empty);
                output.AppendLine();
            }

            output.Append(")");
            output.AppendLine();

            output.Append("values ");
            output.AppendLine();

            output.Append("(");
            output.AppendLine();

            for (var i = 0; i < columns.Count; i++)
            {
                var item = columns[i];

                output.AppendFormat(" {0}{1}", NamingConvention.GetParameterName(item.Name), i < columns.Count - 1 ? ", " : string.Empty);
                output.AppendLine();
            }

            output.Append(")");
            output.AppendLine();

            if (!string.IsNullOrEmpty(Identity))
            {
                output.AppendLine();

                output.AppendFormat("select {0} = scope_identity()", NamingConvention.GetParameterName(Identity));
                output.AppendLine();
            }

            return output.ToString();
        }
    }
}
