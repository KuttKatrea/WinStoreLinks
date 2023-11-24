using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.System;
using Net.Katrea.WinStoreLinks;
using Ookii.CommandLine;

void CreateLinks(CommandLineOptions commandLineOptions)
{
    var packageManager = new PackageManager();

    var packages =
        (IEnumerable<Package>)packageManager.FindPackagesForUser("");
       
    foreach (var packageItem in packages)
    {
        Console.WriteLine("----");
        Console.WriteLine(packageItem.DisplayName);
        Console.WriteLine(packageItem.InstalledLocation.Path);

        var manifestFile = packageItem.InstalledLocation.GetFileAsync("AppxManifest.xml").GetAwaiter().GetResult();
        var manifestXml = FileIO.ReadTextAsync(manifestFile).GetAwaiter().GetResult();

        var doc = XDocument.Parse(manifestXml);
        XNamespace packageNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
        XNamespace uapNamespace = "http://schemas.microsoft.com/appx/manifest/uap/windows10";

        var hidden = new HashSet<string>();

        foreach (var it in doc.Descendants(packageNamespace + "Application"))
        {
            var appId = it.Attribute("Id")?.Value ?? "";
            var ve = it.Descendants(uapNamespace + "VisualElements").FirstOrDefault();
            if (ve?.Attribute("AppListEntry")?.Value == "none")
            {
                hidden.Add(appId);
            }
        }
            
        Console.WriteLine($"Hidden: {string.Join(", ", hidden)}");
            
        var packageApps = AppDiagnosticInfo.RequestInfoForPackageAsync(packageItem.Id.FamilyName).GetAwaiter().GetResult();
        foreach (var k in packageApps)
        {
            Console.WriteLine($"{k.AppInfo.DisplayInfo.DisplayName} ({k.AppInfo.Id})");
            if (hidden.Contains(k.AppInfo.Id))
            {
                Console.WriteLine("HIDDEN");
                continue;
            } 
            CreateLink(commandLineOptions.DirectoryPath, packageItem.Id.FamilyName, k.AppInfo.Id, k.AppInfo.DisplayInfo.DisplayName, k.AppInfo.DisplayInfo.Description);    
        }
    }
}

void CreateLink(string linkFolder, string appFamily, string appId, string appName, string appDescription)
{
    var targetFile = Path.Combine(linkFolder, $"{appName}.lnk");

    if (File.Exists(targetFile))
    {
        return;
    }
    
    IShellLink link = (IShellLink)new ShellLink();

    // setup shortcut information
    link.SetDescription(appDescription);
    link.SetPath($@"shell:appsFolder\{appFamily}!{appId}");

    // save it
    ((IPersistFile)link).Save(targetFile, false);
}

var parser = new CommandLineParser(typeof(CommandLineOptions));
CommandLineOptions arguments;
try
{
    arguments = (CommandLineOptions)parser.Parse(args)!;
}
catch (CommandLineArgumentException ex)
{
    Console.WriteLine(ex.Message);
    parser.WriteUsage();
    return;
}

CreateLinks(arguments);

public class CommandLineOptions
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    [Description("Folder to put the links")]
    public string? DirectoryPath { get; set; }
}