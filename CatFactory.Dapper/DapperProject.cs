using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.Dapper
{
    public class DapperProject : Project<DapperProjectSettings>
    {
        public DapperProject()
            : base()
        {
            CodeNamingConvention = new DotNetNamingConvention();
            NamingService = new NamingService();
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
                .Select(item => new ProjectFeature<DapperProjectSettings>(item, GetDbObjects(Database, item)) { Project = this })
                .ToList();
        }

        private IEnumerable<DbObject> GetDbObjects(Database database, string schema)
        {
            foreach (var table in Database.Tables.Where(item => item.Schema == schema))
            {
                yield return new DbObject(table.Schema, table.Name)
                {
                    Type = "Table"
                };
            }

            foreach (var view in Database.Views.Where(item => item.Schema == schema))
            {
                yield return new DbObject(view.Schema, view.Name)
                {
                    Type = "View"
                };
            }

            foreach (var scalarFunction in Database.ScalarFunctions.Where(item => item.Schema == schema))
            {
                yield return new DbObject(scalarFunction.Schema, scalarFunction.Name)
                {
                    Type = "ScalarFunction"
                };
            }

            foreach (var tableFunction in Database.TableFunctions.Where(item => item.Schema == schema))
            {
                yield return new DbObject(tableFunction.Schema, tableFunction.Name)
                {
                    Type = "TableFunction"
                };
            }

            foreach (var storedProcedure in Database.StoredProcedures.Where(item => item.Schema == schema))
            {
                yield return new DbObject(storedProcedure.Schema, storedProcedure.Name)
                {
                    Type = "StoredProcedure"
                };
            }
        }
    }
}
