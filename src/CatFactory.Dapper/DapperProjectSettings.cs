using System.Collections.Generic;
using System.Diagnostics;

namespace CatFactory.Dapper
{
    public class DapperProjectSettings
    {
        public bool ForceOverwrite { get; set; }

        public bool SimplifyDataTypes { get; set; }

        public bool UseAutomaticPropertiesForEntities { get; set; } = true;

        public bool EnableDataBindings { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> m_exclusions;

        public List<string> Exclusions
        {
            get
            {
                return m_exclusions ?? (m_exclusions = new List<string>());
            }
            set
            {
                m_exclusions = value;
            }
        }
    }
}
