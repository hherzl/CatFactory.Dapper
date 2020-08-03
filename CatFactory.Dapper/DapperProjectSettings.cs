using System;
using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;
using CatFactory.ObjectRelationalMapping.Actions;

namespace CatFactory.Dapper
{
    public class DapperProjectSettings : IProjectSettings
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_insertExclusions;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_updateExclusions;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<IEntityAction> m_actions;

        public DapperProjectSettings()
        {
            Actions.Add(new ReadAllAction());
            Actions.Add(new ReadByKeyAction());
            Actions.Add(new ReadByUniqueAction());
            Actions.Add(new AddEntityAction());
            Actions.Add(new UpdateEntityAction());
            Actions.Add(new RemoveEntityAction());
        }

        public bool ForceOverwrite { get; set; }

        public bool SimplifyDataTypes { get; set; } = true;

        public bool UseAutomaticPropertiesForEntities { get; set; } = true;

        public bool EnableDataBindings { get; set; }

        public bool UseStringBuilderForQueries { get; set; } = true;

        public List<string> InsertExclusions
        {
            get => m_insertExclusions ?? (m_insertExclusions = new List<string>());
            set => m_insertExclusions = value;
        }

        public List<string> UpdateExclusions
        {
            get => m_updateExclusions ?? (m_updateExclusions = new List<string>());
            set => m_updateExclusions = value;
        }

        public bool AddPagingForGetAllOperation { get; set; }

        public List<IEntityAction> Actions
        {
            get => m_actions ?? (m_actions = new List<IEntityAction>());
            set => m_actions = value;
        }

        [Obsolete("Connection is not a parameter for repository methods anymore, set connection instance in repository constructor.")]
        public bool DeclareConnectionAsParameter { get; set; }

        // todo: Add this feature
        //public bool ScaffoldStoredProcedures { get; set; }

        public ValidationResult Validate()
        {
            // todo: Add this implementation

            throw new NotImplementedException();
        }
    }
}
