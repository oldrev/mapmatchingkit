//#addin nuget:?package=Cake.DocFx&version=0.5.0
//#tool "docfx.console"

var solutionFile = "Sandwych.MapMatchingKit.sln";

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    DotNetCoreRestore(solutionFile, new DotNetCoreRestoreSettings
    {
        Verbosity = DotNetCoreVerbosity.Minimal,
    });
});


Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    Information("Building Libraries...");
    var libSettings = new DotNetCoreBuildSettings() {
        NoRestore = true,
        Configuration = configuration,
    };

    if(!IsRunningOnWindows()) 
    {
        libSettings.Framework = "netstandard2.0";
    }

    var libProjects = GetFiles("./src/**/*.csproj");
    foreach(var project in libProjects)
    {
        // .NET Core
        DotNetCoreBuild(project.ToString(), libSettings);
    }

    Information("Building Unit tests...");
    var testSettings = new DotNetCoreBuildSettings() {
        NoRestore = true,
        Configuration = configuration,
    };

    var testProjects = GetFiles("./test/**/*.csproj");
    foreach(var project in testProjects)
    {
        // .NET Core
        DotNetCoreBuild(project.ToString(), testSettings);
    }

});


Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Framework = "netcoreapp2.1",
        NoBuild = true,
        NoRestore = true,
        Configuration = configuration,
    };

    var projects = GetFiles("./test/**/*.Tests.csproj");
    foreach(var project in projects)
    {
        DotNetCoreTest(project.ToString(), settings);
    }
});


Task("Create-NuGet-Packages")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    // Build libraries
    var projects = GetFiles("./src/**/*.csproj");
    foreach(var project in projects)
    {
        var name = project.GetDirectory().FullPath;
        if(name.EndsWith("Tests") || name.EndsWith("Xunit"))
        {
            continue;
        }

        DotNetCorePack(project.FullPath, new DotNetCorePackSettings {
            NoBuild = true,
            NoRestore = true,
            IncludeSymbols = false,
            Configuration = configuration,
        });
    }
});


/*
Task("Generate-Docs").Does(() => {
    DocFxBuild("./docs/docfx.json");
});


Task("View-Docs").Does(() => {
    DocFxBuild("./docs/docfx.json", new DocFxBuildSettings {
        Serve = true,
    });
});
*/


Task("Travis")
    .IsDependentOn("Run-Unit-Tests");


Task("Appveyor-Build")
    .IsDependentOn("Create-NuGet-Packages");


Task("Appveyor-Test")
    .IsDependentOn("Run-Unit-Tests");


Task("Default")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => {
    Information("Executing the default task...");
});


RunTarget(target);
