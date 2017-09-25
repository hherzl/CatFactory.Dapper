using System;

namespace CatFactory.Dapper
{
    public class DapperProjectSettings
    {
        public DapperProjectSettings()
        {
        }

        public Boolean SimplifyDataTypes { get; set; }

        public Boolean UseAutomaticPropertiesForEntities { get; set; } = true;

        public Boolean EnableDataBindings { get; set; }
    }
}
