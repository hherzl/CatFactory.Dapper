using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CatFactory.Dapper.Sql.Dml
{
    public class InsertInto<TEntity> : Query
    {
        public string Table { get; set; }

        public string Identity { get; set; }

        public List<string> Columns { get; set; } = new List<string>();

        public override string ToString()
        {
            var output = new StringBuilder();

            for (var i = 0; i < Headers.Count; i++)
            {
                output.AppendFormat("{0}", Headers[i]);
                output.AppendLine();
            }

            output.AppendFormat(" insert into {0} ", Table);
            output.AppendLine();

            var columns = string.IsNullOrEmpty(Identity) ? Columns : Columns.Where(item => item != Identity).ToList();

            output.Append("(");
            output.AppendLine();

            for (var i = 0; i < columns.Count; i++)
            {
                output.AppendFormat("{0}{1}", columns[i], i < columns.Count - 1 ? ", " : string.Empty);
                output.AppendLine();
            }

            output.Append(")");
            output.AppendLine();

            output.Append(" values ");
            output.AppendLine();

            output.Append("(");
            output.AppendLine();

            for (var i = 0; i < columns.Count; i++)
            {
                output.AppendFormat(" {0}{1}", columns[i], i < columns.Count - 1 ? ", " : string.Empty);
                output.AppendLine();
            }

            output.Append(")");
            output.AppendLine();

            if (!string.IsNullOrEmpty(Footer))
            {
                output.AppendFormat("select {0} = @@identity", Identity);
                output.AppendLine();
            }

            return output.ToString();
        }
    }
}
