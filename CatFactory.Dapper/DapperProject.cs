using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore;
using CatFactory.ObjectRelationalMapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.Dapper
{
    public class DapperProject : CSharpProject<DapperProjectSettings>
    {
        public DapperProject()
            : base()
        {
        }

        public DapperProject(ILogger<DapperProject> logger)
            : base(logger)
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DapperProjectNamespaces m_projectNamespaces;

        public DapperProjectNamespaces ProjectNamespaces
        {
            get
            {
                return m_projectNamespaces ?? (m_projectNamespaces = new DapperProjectNamespaces());
            }
            set
            {
                m_projectNamespaces = value;
            }
        }

        public override void BuildFeatures()
        {
            if (Database == null)
                return;

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item => new ProjectFeature<DapperProjectSettings>(item, GetDbObjectsBySchema(item), this))
                .ToList();
        }
    }
}
