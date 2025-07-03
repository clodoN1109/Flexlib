using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Flexlib.Common;

public static class ReflectionUtils
{
    public static List<Type> GetDerivedTypes<TBase>()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(TBase).IsAssignableFrom(t))
            .ToList();
    }
}
