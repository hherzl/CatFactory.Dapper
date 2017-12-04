namespace CatFactory.Dapper
{
    public static class ProjectFeatureExtensions
    {
        public static DapperProject GetDapperProject(this ProjectFeature projectFeature)
            => projectFeature.Project as DapperProject;
    }
}
