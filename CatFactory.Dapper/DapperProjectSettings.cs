using System;
using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.CodeFactory;

namespace CatFactory.Dapper
{
    public class DapperProjectSettings : ProjectSettings
    {
        public bool ForceOverwrite { get; set; }

        public bool SimplifyDataTypes { get; set; }

        public bool UseAutomaticPropertiesForEntities { get; set; } = true;

        public bool EnableDataBindings { get; set; }

        public bool UseStringBuilderForQueries { get; set; } = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_insertExclusions;

        public List<string> InsertExclusions
        {
            get
            {
                return m_insertExclusions ?? (m_insertExclusions = new List<string>());
            }
            set
            {
                m_insertExclusions = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_updateExclusions;

        public List<string> UpdateExclusions
        {
            get
            {
                return m_updateExclusions ?? (m_updateExclusions = new List<string>());
            }
            set
            {
                m_updateExclusions = value;
            }
        }

        public bool AddPagingForGetAllOperation { get; set; }

        [Obsolete("Connection is not a parameter for repository methods anymore, set connection instance in repository constructor.")]
        public bool DeclareConnectionAsParameter { get; set; }

        // todo: Add this feature
        //public bool ScaffoldStoredProcedures { get; set; }
    }
}
