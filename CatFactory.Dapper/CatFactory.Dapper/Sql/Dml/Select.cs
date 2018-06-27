using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CatFactory.Dapper.Sql.Dml
{
    public class Select<Entity> : Query
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_columns;

        public List<string> Columns
        {
            get
            {
                return m_columns ?? (m_columns = new List<string>());
            }
            set
            {
                m_columns = value;
            }
        }

        public string From { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Condition> m_where;

        public List<Condition> Where
        {
            get
            {
                return m_where ?? (m_where = new List<Condition>());
            }
            set
            {
                m_where = value;
            }
        }

        public override string ToString()
        {
            var output = new StringBuilder();

            for (var i = 0; i < Headers.Count; i++)
            {
                output.AppendFormat("{0}", Headers[i]);
                output.AppendLine();
            }

            output.Append("select");
            output.AppendLine();

            for (var i = 0; i < Columns.Count; i++)
            {
                output.AppendFormat(" {0}{1}", Columns[i], i < Columns.Count - 1 ? "," : string.Empty);
                output.AppendLine();
            }

            output.AppendFormat(" from {0} ", From);
            output.AppendLine();

            if (Where.Count > 0)
            {
                output.Append(" where ");
                output.AppendLine();

                for (var i = 0; i < Where.Count; i++)
                {
                    if (i > 0)
                        output.AppendFormat(" {0} ", Where[i].LogicOperator);

                    var comparisonOperator = string.Empty;

                    if (Where[i].ComparisonOperator == ComparisonOperator.Equals)
                        comparisonOperator = "=";
                    else if (Where[i].ComparisonOperator == ComparisonOperator.NotEquals)
                        comparisonOperator = "<>";

                    output.AppendFormat(" {0} {1} {2}", Where[i].Column, comparisonOperator, Where[i].Column);
                    output.AppendLine();
                }
            }

            return output.ToString();
        }
    }
}
