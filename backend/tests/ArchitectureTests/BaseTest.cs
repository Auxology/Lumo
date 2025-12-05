using System.Reflection;

namespace ArchitectureTests;

public abstract class BaseTest
{
    protected static Assembly[] GetDomainAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.EndsWith(".Domain", StringComparison.Ordinal) == true)
            .Where(a => a.GetName().Name?.Contains("SharedKernel", StringComparison.Ordinal) != true)
            .ToArray();

    protected static Assembly[] GetApplicationAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.EndsWith(".Application", StringComparison.Ordinal) == true)
            .Where(a => a.GetName().Name?.Contains("SharedKernel", StringComparison.Ordinal) != true)
            .ToArray();

    protected static Assembly[] GetInfrastructureAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.EndsWith(".Infrastructure", StringComparison.Ordinal) == true)
            .Where(a => a.GetName().Name?.Contains("SharedKernel", StringComparison.Ordinal) != true)
            .ToArray();

    protected static Assembly[] GetApiAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.EndsWith(".Api", StringComparison.Ordinal) == true)
            .Where(a => a.GetName().Name?.Contains("SharedKernel", StringComparison.Ordinal) != true)
            .ToArray();
}