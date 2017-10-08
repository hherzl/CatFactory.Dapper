using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public class RepositoryInterfaceDefinition : CSharpInterfaceDefinition
    {
        public RepositoryInterfaceDefinition(DapperProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public DapperProject Project { get; }

        public void Init()
        {
            Namespace = Project.GetDataLayerContractsNamespace();

            Name = "IRepository";
        }
    }
}
