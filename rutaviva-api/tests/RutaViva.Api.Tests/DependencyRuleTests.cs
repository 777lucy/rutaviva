using System.Xml.Linq;

namespace RutaViva.Api.Tests;

public sealed class DependencyRuleTests
{
    [Fact]
    public void Domain_references_nothing()
    {
        Assert.Empty(ProjectReferences("RutaViva.Domain"));
    }

    [Fact]
    public void Application_references_only_the_domain()
    {
        Assert.Equal(["RutaViva.Domain"], ProjectReferences("RutaViva.Application"));
    }

    [Fact]
    public void Infrastructure_references_application_and_domain()
    {
        Assert.Equal(["RutaViva.Application", "RutaViva.Domain"], ProjectReferences("RutaViva.Infrastructure"));
    }

    [Fact]
    public void Api_does_not_reference_the_domain()
    {
        Assert.Equal(["RutaViva.Application", "RutaViva.Infrastructure"], ProjectReferences("RutaViva.Api"));
    }

    private static string[] ProjectReferences(string project)
    {
        string projectFile = Path.Combine(SolutionRoot(), "src", project, $"{project}.csproj");
        return XDocument.Load(projectFile)
            .Descendants("ProjectReference")
            .Select(node => Path.GetFileNameWithoutExtension(node.Attribute("Include")!.Value))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string SolutionRoot()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "RutaViva.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("RutaViva.sln was not found.");
    }
}
