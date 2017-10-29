using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.Mapping;

namespace CatFactory.Dapper
{
    public class DapperProject : Project
    {
        public DapperProject()
        {
        }

        public override void BuildFeatures()
        {
            if (Database == null)
            {
                return;
            }

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item => new ProjectFeature(item, GetDbObjects(Database, item)) { Project = this })
                .ToList();
        }

        private IEnumerable<DbObject> GetDbObjects(Database database, string schema)
        {
            var result = new List<DbObject>();

            result.AddRange(Database
                .Tables
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "USER_TABLE" }));

            result.AddRange(Database
                .Views
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "VIEW" }));

            return result;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ProjectNamespaces m_namespaces;

        public ProjectNamespaces Namespaces
            => m_namespaces ?? (m_namespaces = new ProjectNamespaces());

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DapperProjectSettings m_settings;

        public DapperProjectSettings Settings
            => m_settings ?? (m_settings = new DapperProjectSettings());
    }
}
