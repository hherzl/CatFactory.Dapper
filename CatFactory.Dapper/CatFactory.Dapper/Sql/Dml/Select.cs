using System.Collections.Generic;
using System.Text;

namespace CatFactory.Dapper.Sql.Dml
{
    public class Select<Entity> : Query
    {
        public List<string> Columns { get; set; } = new List<string>();

        public string From { get; set; }

        public List<Condition> Where { get; set; } = new List<Condition>();

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
                    {
                        output.AppendFormat(" {0} ", Where[i].LogicOperator);
                    }

                    var comparisonOperator = string.Empty;

                    if (Where[i].ComparisonOperator == ComparisonOperator.Equals)
                    {
                        comparisonOperator = "=";
                    }
                    else if (Where[i].ComparisonOperator == ComparisonOperator.NotEquals)
                    {
                        comparisonOperator = "<>";
                    }

                    output.AppendFormat(" {0} {1} {2}", Where[i].Column, comparisonOperator, Where[i].Column);
                    output.AppendLine();
                }
            }

            return output.ToString();
        }
    }
}
