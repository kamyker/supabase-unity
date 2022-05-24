using System.Diagnostics;

string appDir = Environment.CurrentDirectory;
var repoDir = Directory.GetParent(appDir).Parent.Parent.Parent.Parent;
var submodulesDirPath = Path.Combine(repoDir.FullName, ".Submodules");
if(!Directory.Exists(submodulesDirPath))
	throw new Exception($"Something went wrong, directory doesn't exists: {submodulesDirPath}");

var unityDirPath = Path.Combine(repoDir.FullName, "Unity");
var unityDir = new DirectoryInfo(unityDirPath);
var supClonedDirPath = Path.Combine(unityDirPath, "supabase-cloned");

// make sure we aren't deleting random "Unity" folder
if(Directory.Exists(supClonedDirPath))
	unityDir.Delete(true);
unityDir.Create();
Directory.CreateDirectory(supClonedDirPath);


// copy cs files
DirectoryInfo submodulesDir = new DirectoryInfo(submodulesDirPath);
Utils.CopyDirectoryRecursive(submodulesDir, supClonedDirPath);


// build dlls
DirectoryInfo supaDir = new DirectoryInfo(Path.Combine(submodulesDir.FullName, "supabase-csharp", "Supabase"));
Process cmd = new Process()
{
	StartInfo = new ProcessStartInfo("cmd.exe", "/c " + @"dotnet publish -o ../../../.build")
	{
		RedirectStandardOutput = true,
		CreateNoWindow = true,
		UseShellExecute = false,
		WorkingDirectory = supaDir.FullName
	}
};
cmd.Start();

await cmd.WaitForExitAsync();
Console.WriteLine(cmd.StandardOutput.ReadToEnd());



// create main package
string packageJsonTemplate = @"
{
	""name"": ""name_replace"",
	""version"": ""1.0.0""
}";
await File.WriteAllTextAsync(Path.Combine(unityDirPath, "package.json"), packageJsonTemplate.Replace("name_replace", "com.supabase.unity"));

string asmdefTemplate = @"{
""name"": ""name_replace"",
""rootNamespace"": """",
""references"": [],
""includePlatforms"": [],
""excludePlatforms"": [],
""allowUnsafeCode"": false,
""overrideReferences"": false,
""precompiledReferences"": [],
""autoReferenced"": true,
""defineConstraints"": [],
""noEngineReferences"": false
}";
await File.WriteAllTextAsync(Path.Combine(unityDirPath, "Supabase.asmdef"), asmdefTemplate.Replace("name_replace", "Supabase"));


// create dlls packages
var buildDir = new DirectoryInfo(Path.Combine(repoDir.FullName, ".build"));


var unityDllsPath = Path.Combine(repoDir.FullName, ".UnityDlls");
if(Directory.Exists(unityDllsPath))
	Directory.Delete(unityDllsPath, true);

foreach(var file in buildDir.EnumerateFiles("*.dll"))
{
	var nameWithoutExt = Path.GetFileNameWithoutExtension(file.Name);
	var destPath = Path.Combine(repoDir.FullName, ".UnityDlls", nameWithoutExt);
	Directory.CreateDirectory(destPath);
	file.CopyTo(Path.Combine(destPath, file.Name));
	var packageJsonPath = Path.Combine(destPath, "package.json");
	await File.WriteAllTextAsync(packageJsonPath, packageJsonTemplate.Replace("name_replace", $"com.supabase.dll.{nameWithoutExt}"));
}