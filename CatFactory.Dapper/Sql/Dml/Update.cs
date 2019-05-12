using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CatFactory.Dapper.Sql.Dml
{
    public class Update<TEntity> : Query
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<UpdateColumn> m_columns;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<Condition> m_where;

        public Update()
            : base()
        {
        }

        public string Table { get; set; }

        public string Key { get; set; }

        public List<UpdateColumn> Columns
        {
            get => m_columns ?? (m_columns = new List<UpdateColumn>());
            set => m_columns = value;
        }

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

            output.AppendFormat(" update {0} ", Table);
            output.AppendLine();

            output.Append(" set ");
            output.AppendLine();

            var columns = string.IsNullOrEmpty(Key) ? Columns : Columns.Where(item => item.Name != Key).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var item = columns[i];

                var columnName = NamingConvention.GetObjectName(item.Name);
                var parameterName = NamingConvention.GetParameterName(item.Name);

                output.AppendFormat("{0} = {1}{2}", columnName, parameterName, i < columns.Count - 1 ? ", " : string.Empty);
                output.AppendLine();
            }

            if (Where.Count > 0)
            {
                output.Append(" where ");
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
