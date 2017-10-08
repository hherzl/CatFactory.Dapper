using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CatFactory.Dapper
{
    public class DapperProjectSettings
    {
        public Boolean SimplifyDataTypes { get; set; }

        public Boolean UseAutomaticPropertiesForEntities { get; set; } = true;

        public Boolean EnableDataBindings { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<String> m_exclusions;

        public List<String> Exclusions
        {
            get
            {
                return m_exclusions ?? (m_exclusions = new List<String>());
            }
            set
            {
                m_exclusions = value;
            }
        }
    }
}
