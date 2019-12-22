using System.Collections.Generic;
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
        public static DapperProject Create(string name, Database database, string outputDirectory)
            => new DapperProject
            {
                Name = name,
                Database = database,
                OutputDirectory = outputDirectory
            };

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
            get => m_projectNamespaces ?? (m_projectNamespaces = new DapperProjectNamespaces());
            set => m_projectNamespaces = value;
        }

        protected override IEnumerable<DbObject> GetDbObjectsBySchema(string schema)
        {
            foreach (var item in base.GetDbObjectsBySchema(schema))
            {
                yield return item;
            }

            foreach (var item in Database.GetScalarFunctions().Where(tableFunction => tableFunction.Schema == schema))
            {
                yield return new DbObject(item.Schema, item.Name)
                {
                    Type = "ScalarFunction"
                };
            }

            foreach (var item in Database.GetTableFunctions().Where(tableFunction => tableFunction.Schema == schema))
            {
                yield return new DbObject(item.Schema, item.Name)
                {
                    Type = "TableFunction"
                };
            }

            foreach (var item in Database.GetStoredProcedures().Where(storedProcedure => storedProcedure.Schema == schema))
            {
                yield return new DbObject(item.Schema, item.Name)
                {
                    Type = "StoredProcedure"
                };
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
