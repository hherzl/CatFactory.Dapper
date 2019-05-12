using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CatFactory.Dapper.Sql.Dml
{
    public class DeleteFrom<TEntity> : Query
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<Condition> m_where;

        public DeleteFrom()
            : base()
        {
        }

        public string Schema { get; set; }

        public string Table { get; set; }

        public string Key { get; set; }

        public List<Condition> Where
        {
            get => m_where ?? (m_where = new List<Condition>());
            set => m_where = value;
        }

        public override string ToString()
        {
            var output = new StringBuilder();

            for (var i = 0; i < Headers.Count; i++)
            {
                output.AppendFormat("{0}", Headers[i]);
                output.AppendLine();
            }

            output.AppendLine("delete from");

            output.AppendFormat(" {0}", NamingConvention.GetObjectName(Schema, Table));
            output.AppendLine();

            if (Where.Count > 0)
            {
                output.Append("where");
                output.AppendLine();

                for (var i = 0; i < Where.Count; i++)
                {
                    var item = Where[i];

                    if (i > 0)
                    {
                        if (item.LogicOperator == LogicOperator.And)
                            output.Append(" and");
                        else if (item.LogicOperator == LogicOperator.Or)
                            output.Append(" or");
                    }

                    var comparisonOperator = string.Empty;

                    if (item.ComparisonOperator == ComparisonOperator.Equals)
                        comparisonOperator = "=";
                    else if (item.ComparisonOperator == ComparisonOperator.NotEquals)
                        comparisonOperator = "<>";

                    var columnName = NamingConvention.GetObjectName(item.Column);
                    var parameterName = NamingConvention.GetParameterName(item.Column);

                    output.AppendFormat(" {0} {1} {2}", columnName, comparisonOperator, parameterName);
                    output.AppendLine();
                }
            }

            return output.ToString();
        }
    }
}
