using System.Reflection;

using FluentAssertions;

using NetArchTest.Rules;

namespace ArchitectureTests;

internal sealed class LayerTests : BaseTest
{
    private static IEnumerable<Assembly> GetFeatureAssemblies()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !IsSharedKernel(a) && a.GetName().Name is not null);
    }

    private static IEnumerable<Assembly> GetDomainAssemblies()
    {
        return GetFeatureAssemblies().Where(IsDomainProject);
    }

    private static IEnumerable<Assembly> GetApplicationAssemblies()
    {
        return GetFeatureAssemblies().Where(IsApplicationProject);
    }

    private static IEnumerable<Assembly> GetInfrastructureAssemblies()
    {
        return GetFeatureAssemblies().Where(IsInfrastructureProject);
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        IEnumerable<Assembly> domainAssemblies = GetDomainAssemblies();

        foreach (Assembly assembly in domainAssemblies)
        {
            string moduleName = GetModuleName(assembly);

            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn($"{moduleName}.Application")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Domain) must not depend on Application layer");
        }
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        IEnumerable<Assembly> domainAssemblies = GetDomainAssemblies();

        foreach (Assembly assembly in domainAssemblies)
        {
            string moduleName = GetModuleName(assembly);

            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn($"{moduleName}.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Domain) must not depend on Infrastructure layer");
        }
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Api()
    {
        IEnumerable<Assembly> domainAssemblies = GetDomainAssemblies();

        foreach (Assembly assembly in domainAssemblies)
        {
            string moduleName = GetModuleName(assembly);

            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn($"{moduleName}.Api")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Domain) must not depend on Api layer");
        }
    }

    [Fact]
    public void Domain_ShouldNotDependOn_AspNetCore()
    {
        IEnumerable<Assembly> domainAssemblies = GetDomainAssemblies();

        foreach (Assembly assembly in domainAssemblies)
        {
            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.AspNetCore")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Domain) must not depend on ASP.NET Core");
        }
    }

    [Fact]
    public void Domain_ShouldNotDependOn_EntityFrameworkCore()
    {
        IEnumerable<Assembly> domainAssemblies = GetDomainAssemblies();

        foreach (Assembly assembly in domainAssemblies)
        {
            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Domain) must not depend on EF Core");
        }
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        IEnumerable<Assembly> applicationAssemblies = GetApplicationAssemblies();

        foreach (Assembly assembly in applicationAssemblies)
        {
            string moduleName = GetModuleName(assembly);

            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn($"{moduleName}.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Application) must not depend on Infrastructure layer");
        }
    }

    [Fact]
    public void Application_ShouldNotDependOn_Api()
    {
        IEnumerable<Assembly> applicationAssemblies = GetApplicationAssemblies();

        foreach (Assembly assembly in applicationAssemblies)
        {
            string moduleName = GetModuleName(assembly);

            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn($"{moduleName}.Api")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Application) must not depend on Api layer");
        }
    }

    [Fact]
    public void Application_ShouldNotDependOn_AspNetCore()
    {
        IEnumerable<Assembly> applicationAssemblies = GetApplicationAssemblies();

        foreach (Assembly assembly in applicationAssemblies)
        {
            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.AspNetCore")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Application) must not depend on ASP.NET Core");
        }
    }

    [Fact]
    public void Application_ShouldNotDependOn_EntityFrameworkCore()
    {
        IEnumerable<Assembly> applicationAssemblies = GetApplicationAssemblies();

        foreach (Assembly assembly in applicationAssemblies)
        {
            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Application) must not depend on EF Core");
        }
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Api()
    {
        IEnumerable<Assembly> infrastructureAssemblies = GetInfrastructureAssemblies();

        foreach (Assembly assembly in infrastructureAssemblies)
        {
            string moduleName = GetModuleName(assembly);

            TestResult result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn($"{moduleName}.Api")
                .GetResult();

            result.IsSuccessful.Should()
                .BeTrue(because: $"{assembly.GetName().Name} (Infrastructure) must not depend on Api layer");
        }
    }
}