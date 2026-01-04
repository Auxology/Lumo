using System.Reflection;

namespace ArchitectureTests;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected static bool IsDomainProject(Assembly assembly)
    {
#pragma warning disable CA1062
        string? name = assembly.GetName().Name;
#pragma warning restore CA1062
        return name is not null &&
               !name.StartsWith("SharedKernel", StringComparison.Ordinal) &&
               !name.Contains(".Application", StringComparison.Ordinal) &&
               !name.Contains(".Api", StringComparison.Ordinal) &&
               !name.Contains(".Infrastructure", StringComparison.Ordinal);
    }

    protected static bool IsApplicationProject(Assembly assembly)
    {
#pragma warning disable CA1062
        string? name = assembly.GetName().Name;
#pragma warning restore CA1062
        return name is not null &&
               !name.StartsWith("SharedKernel", StringComparison.Ordinal) &&
               name.Contains(".Application", StringComparison.Ordinal);
    }

    protected static bool IsApiProject(Assembly assembly)
    {
#pragma warning disable CA1062
        string? name = assembly.GetName().Name;
#pragma warning restore CA1062
        return name is not null &&
               !name.StartsWith("SharedKernel", StringComparison.Ordinal) &&
               name.Contains(".Api", StringComparison.Ordinal);
    }

    protected static bool IsInfrastructureProject(Assembly assembly)
    {
#pragma warning disable CA1062
        string? name = assembly.GetName().Name;
#pragma warning restore CA1062
        return name is not null &&
               !name.StartsWith("SharedKernel", StringComparison.Ordinal) &&
               name.Contains(".Infrastructure", StringComparison.Ordinal);
    }

    protected static bool IsSharedKernel(Assembly assembly)
    {
#pragma warning disable CA1062
        string? name = assembly.GetName().Name;
#pragma warning restore CA1062
        return name is not null && name.StartsWith("SharedKernel", StringComparison.Ordinal);
    }

    protected static string GetModuleName(Assembly assembly)
    {
#pragma warning disable CA1062
        string? name = assembly.GetName().Name;
#pragma warning restore CA1062
        if (name is null)
        {
            return string.Empty;
        }

        int dotIndex = name.IndexOf('.', StringComparison.Ordinal);
        return dotIndex > 0 ? name[..dotIndex] : name;
    }
}