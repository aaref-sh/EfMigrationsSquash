using System.Text.RegularExpressions;

var def = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Yellow;
Console.Write("\nWARNING:");
Console.ForegroundColor = def;
Console.WriteLine(" this tool will delete the squashed migration files, if something happened you may lose your migrations, make sure to use source control (like git) or to backup your migrations\n" +
    "use it at your own risk\n\npress any key to continue or (Q) to quit");

if (Console.ReadKey().Key == ConsoleKey.Q) return;
Console.WriteLine();

var migrationsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Migrations");

if (!Directory.Exists(migrationsFolder))
{
    Console.Write("Migrations folder not found!");
    return;
}

var files = Directory.EnumerateFiles(migrationsFolder).Where(x => !x.EndsWith(".Designer.cs")).ToList();

if (files.Count == 0)
{
    Console.Write("No migration files found!");
    return;
}

files = files.Take(files.Count - 1).ToList();

var lst = string.Join('\n', files.Select((x, i) => $"{i + 1} - {Path.GetFileName(x)}"));
Console.WriteLine("Your migrations:\n" + lst);

int start, end;

Console.Write("Enter starting file number: ");
while (!int.TryParse(Console.ReadLine(), out start)) Console.Write("Invalid input. Try again: ");

Console.Write("Enter end file number: ");
while (!int.TryParse(Console.ReadLine(), out end)) Console.Write("Invalid input. Try again: ");

var ups = new List<string>();
var downs = new List<string>();
var usings = new List<string>();

Console.WriteLine("Collecting data");
for (int i = 0; i <= end - start; i++)
{
    // Convert user provided numbers (1-based) into 0-based indexes.
    var path = files[start + i - 1];
    var fileName = $"\n// content of {Path.GetFileName(path)}\n";
    var fileContent = File.ReadAllText(path);

    usings.AddRange(Regexes.UsingsRegex.Match(fileContent).Value.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    ups.Add(fileName + Regexes.UpMigrationRegex.Match(fileContent).Groups[1].Value);
    downs.Add(fileName + Regexes.DownMigrationRegex.Match(fileContent).Groups[1].Value);
}

Console.WriteLine("Combining");
var squashedUp = string.Join('\n', ups);
downs.Reverse();
var squashedDown = string.Join('\n', downs);
var combinedUsing = string.Join(";\n", usings.Select(x => x.Trim()).ToHashSet()) + ";";

Console.WriteLine("Writing result");
var destFile = File.ReadAllText(files[end - 1]);

var resultContent = Regexes.CompleteMigrationFileRegex.Replace(destFile, match =>
    $"{combinedUsing}\n\r\n#nullable disable\n\r\n{match.Groups[3].Value}\n{squashedUp}{match.Groups[5].Value}{squashedDown}\n\t\t}}\n\t}}\n}}"
);

try
{
    File.WriteAllText(files[end - 1], resultContent);
    Console.WriteLine("Deleting parsed files");
    for (int i = 0; i < end - start; i++)
    {
        var filename = files[start + i - 1];
        try
        {
            // Replace range slicing with Substring for .NET 6 compatibility.
            var designerFilename = string.Concat(filename.AsSpan(0, filename.Length - 3), ".Designer.cs");
            File.Delete(filename);
            File.Delete(designerFilename);
        }
        catch
        {
            Console.WriteLine($"Failed deleting file: {filename}");
        }
    }
}
catch
{
    Console.WriteLine("Failed writing file");
}
Console.WriteLine("Done! double-check your result");

static class Regexes
{
    public static readonly Regex CompleteMigrationFileRegex = new(@"((using .+;\s*)*)[\s\S]+(namespace.*\s{[\s\S]+protected\s+override\s+void\s+Up\s*\(\s*MigrationBuilder\s+\w+\)\s*{)([\s\S]*)(}[\s\S]+protected\s+override\s+void\s+Down\(MigrationBuilder\s+\w+\)\s+{)([\s\S]*)}\s+}\s*}", RegexOptions.Compiled);

    public static readonly Regex UsingsRegex = new(@"((using .+;\s*)*)", RegexOptions.Compiled);

    public static readonly Regex UpMigrationRegex = new(@"protected\s+override\s+void\s+Up\s*\(\s*MigrationBuilder\s+\w+\)\s*{([\s\S]*)}[\s\S]+protected\s+override\s+void\s+Down", RegexOptions.Compiled);

    public static readonly Regex DownMigrationRegex = new(@"}[\s\S]+protected\s+override\s+void\s+Down\s*\(\s*MigrationBuilder\s+\w+\)\s*{([\s\S]*)}\s+}\s*}", RegexOptions.Compiled);
}