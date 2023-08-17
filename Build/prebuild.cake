using System.Xml.Linq;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var isBamboo = Environment.GetEnvironmentVariables().Keys.Cast<string>().Any(k => k.StartsWith("bamboo_", StringComparison.OrdinalIgnoreCase));

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("CreateNugetConfig")
    .WithCriteria(isBamboo)
    .Does(() =>
{
    var progetUrl = EnvironmentVariableStrict("bamboo_PROGET_URL");
    Information($"Proget: {progetUrl}");

    var nugetConfigFile = Directory(EnvironmentVariableStrict("bamboo_build_working_directory")) + File("NuGet.config");
    var progetKey = "proget-default";
    var progetUb = new UriBuilder(EnvironmentVariableStrict("bamboo_PROGET_URL"));

    progetUb.Path = "/nuget/Default/";

    new XElement("configuration",
        new XElement("packageSources",
            new XElement("add",
                new XAttribute("key", progetKey),
                new XAttribute("value", progetUb.Uri.AbsoluteUri)
            )
        ),
        new XElement("packageSourceCredentials",
            new XElement(progetKey,
                new XElement("add",
                    new XAttribute("key", "Username"),
                    new XAttribute("value", EnvironmentVariableStrict("bamboo_ATLAS_PROGET_USERNAME"))
                ),
                new XElement("add",
                    new XAttribute("key", "ClearTextPassword"),
                    new XAttribute("value", EnvironmentVariableStrict("bamboo_ATLAS_PROGET_PASSWORD"))
                )
            )
        )
    )
    .Save(nugetConfigFile);

    Information($"Wrote {nugetConfigFile}");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("CreateNugetConfig");

RunTarget(target);

string EnvironmentVariableStrict(string name)
{
    var value = EnvironmentVariable(name);

    if (string.IsNullOrEmpty(value))
    {
        throw new Exception($"Environment variable '{name}' is undefined!");
    }

    return value;
}