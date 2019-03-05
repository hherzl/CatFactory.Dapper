using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
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

        public override void Scaffold(IObjectDefinition objectDefinition, string outputDirectory, string subdirectory = "")
        {
            var codeBuilder = default(ICodeBuilder);

            var selection = objectDefinition.DbObject == null ? this.GlobalSelection() : this.GetSelection(objectDefinition.DbObject);

            if (objectDefinition is CSharpClassDefinition)
            {
                codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = outputDirectory,
                    ForceOverwrite = selection.Settings.ForceOverwrite,
                    ObjectDefinition = objectDefinition
                };
            }
            else if (objectDefinition is CSharpInterfaceDefinition)
            {
                codeBuilder = new CSharpInterfaceBuilder
                {
                    OutputDirectory = outputDirectory,
                    ForceOverwrite = selection.Settings.ForceOverwrite,
                    ObjectDefinition = objectDefinition
                };
            }

            OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

            codeBuilder.CreateFile(subdirectory: subdirectory);

            OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
        }
    }
}
