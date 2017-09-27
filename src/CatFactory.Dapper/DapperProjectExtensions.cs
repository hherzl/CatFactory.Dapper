using System;
using System.IO;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;

namespace CatFactory.Dapper
{
    public static class DapperProjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static DapperProjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static String GetEntityLayerNamespace(this Project project)
            => namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, (project as DapperProject).Namespaces.EntityLayer));

        public static String GetEntityLayerNamespace(this DapperProject project, String ns)
            => String.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : String.Join(".", project.Name, project.Namespaces.EntityLayer, ns);

        public static String GetEntityLayerDirectory(this DapperProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.EntityLayer);
    }
}
