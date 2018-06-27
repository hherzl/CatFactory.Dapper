using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public class DapperProject : Project<DapperProjectSettings>
    {
        public DapperProject()
        {
        }

        public override void BuildFeatures()
        {
            if (Database == null)
                return;

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item => new ProjectFeature<DapperProjectSettings>(item, GetDbObjects(Database, item)) { Project = this })
                .ToList();
        }

        private IEnumerable<DbObject> GetDbObjects(Database database, string schema)
        {
            var result = new List<DbObject>();

            result.AddRange(Database
                .Tables
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "Table" }));

            result.AddRange(Database
                .Views
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "View" }));

            result.AddRange(Database
                .TableFunctions
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "TableFunction" }));

            // todo: add scalar functions

            result.AddRange(Database
                .ScalarFunctions
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "ScalarFunction" }));

            return result;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ProjectNamespaces m_namespaces;

        public ProjectNamespaces Namespaces
            => m_namespaces ?? (m_namespaces = new ProjectNamespaces());
    }
}
