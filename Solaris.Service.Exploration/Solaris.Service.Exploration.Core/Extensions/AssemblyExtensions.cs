using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Solaris.Service.Exploration.Core.Extensions
{
    public static class AssemblyExtensions
    {
        public static List<Type> GetEnumsForPath(this Assembly assembly, string path)
        {
            return assembly.GetTypes().Where(t => t.IsEnum && t.IsNotAbstractNorNested() && t.IsPathValid(path) && t.IsPublic).ToList();
        }
        
        public static List<Type> GetTypesForPath(this Assembly assembly, string path)
        {
            return (assembly.GetTypes().Where(t => t.IsClass && t.IsNotAbstractNorNested() && t.IsPathValid(path))).ToList();
        }
    }
}