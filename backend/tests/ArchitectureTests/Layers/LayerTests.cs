using System.Reflection;
using NetArchTest.Rules;

using Shouldly;

namespace ArchitectureTests.Layers;

public sealed class LayerTests : BaseTest
{
    [Fact]
    public void DomainLayerShouldNotHaveDependencyOnApplicationLayer()
    {
        Assembly[] domainAssemblies = GetDomainAssemblies();
        
        foreach (Assembly assembly in domainAssemblies)
        {
            string applicationNamespace = assembly.GetName().Name!.Replace(".Domain", ".Application", StringComparison.Ordinal);
            
            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(applicationNamespace)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue
            (
                $"{assembly.GetName().Name} should not depend on {applicationNamespace}"
            );
        }
    }

    [Fact]
    public void DomainLayerShouldNotHaveDependencyOnInfrastructureLayer()
    {
        Assembly[] domainAssemblies = GetDomainAssemblies();
        
        foreach (Assembly assembly in domainAssemblies)
        {
            string infrastructureNamespace = assembly.GetName().Name!.Replace(".Domain", ".Infrastructure", StringComparison.Ordinal);
            
            TestResult result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(infrastructureNamespace)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue
            (
                $"{assembly.GetName().Name} should not depend on {infrastructureNamespace}"
            );
        }
    }

    [Fact]
    public void DomainLayerShouldNotHaveDependencyOnApiLayer()
    {
        Assembly[] domainAssemblies = GetDomainAssemblies();
        
        foreach (Assembly assembly in domainAssemblies)
        {
            string apiNamespace = assembly.GetName().Name!.Replace(".Domain", ".Api", StringComparison.Ordinal);
            
            TestResult? result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(apiNamespace)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue
            (
                $"{assembly.GetName().Name} should not depend on {apiNamespace}"
            );
        }
    }

    [Fact]
    public void ApplicationLayerShouldNotHaveDependencyOnInfrastructureLayer()
    {
        Assembly[] applicationAssemblies = GetApplicationAssemblies();
        
        foreach (Assembly assembly in applicationAssemblies)
        {
            string infrastructureNamespace = assembly.GetName().Name!.Replace(".Application", ".Infrastructure", StringComparison.Ordinal);
            
            TestResult result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(infrastructureNamespace)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue
            (
                $"{assembly.GetName().Name} should not depend on {infrastructureNamespace}"
            );
        }
    }

    [Fact]
    public void ApplicationLayerShouldNotHaveDependencyOnApiLayer()
    {
        Assembly[] applicationAssemblies = GetApplicationAssemblies();
        
        foreach (Assembly assembly in applicationAssemblies)
        {
            string apiNamespace = assembly.GetName().Name!.Replace(".Application", ".Api", StringComparison.Ordinal);
            
            TestResult result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(apiNamespace)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(
                $"{assembly.GetName().Name} should not depend on {apiNamespace}");
        }
    }

    [Fact]
    public void InfrastructureLayerShouldNotHaveDependencyOnApiLayer()
    {
        Assembly[] infrastructureAssemblies = GetInfrastructureAssemblies();
        
        foreach (var assembly in infrastructureAssemblies)
        {
            string apiNamespace = assembly.GetName().Name!.Replace(".Infrastructure", ".Api", StringComparison.Ordinal);
            
            TestResult result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(apiNamespace)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue
            (
                $"{assembly.GetName().Name} should not depend on {apiNamespace}"
            );
        }
    }
}

