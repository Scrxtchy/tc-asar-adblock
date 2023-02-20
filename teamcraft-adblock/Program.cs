using System.IO;
using System.Text.RegularExpressions;
using AsarSharp;

Console.Write(@"  _                                                  _ _     _            _    
 | |                                                | | |   | |          | |   
 | |_ ___ ______ __ _ ___  __ _ _ __ ______ __ _  __| | |__ | | ___   ___| | __
 | __/ __|______/ _` / __|/ _` | '__|______/ _` |/ _` | '_ \| |/ _ \ / __| |/ /
 | || (__      | (_| \__ \ (_| | |        | (_| | (_| | |_) | | (_) | (__|   < 
  \__\___|      \__,_|___/\__,_|_|         \__,_|\__,_|_.__/|_|\___/ \___|_|\_\
");

FileInfo fileout = new(".\\app\\dist\\apps\\electron\\src\\window\\main-window.js");

FileInfo version = new($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\ffxiv-teamcraft\\packages\\RELEASES");
Console.WriteLine("Finding Teamcraft Version");
var releaseinfo = File.ReadAllText(version.FullName);
string pattern = @"ffxiv-teamcraft-(.+)-\w+";
Match VersionID = Regex.Match(releaseinfo, pattern);
if (VersionID.Success)
{
    FileInfo tcasar = new($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\ffxiv-teamcraft\\app-{VersionID.Groups[1].Value}\\resources\\app.asar");

    Console.WriteLine($"Found verion {VersionID.Groups[1].Value}\nUnpacking...");

    using (AsarExtractor ae = new(tcasar.FullName, ".\\app"))
    {
        try
        {
            ae.Extract();
        } catch (IOException e)
        {
            Console.WriteLine("Unable to extract, is teamcraft still open?\n" + e.StackTrace);
        }
        
    }

    Console.WriteLine("Patching & Repacking...");
    string newmainwindow = File.ReadAllText(fileout.FullName);
    File.WriteAllText(fileout.FullName, newmainwindow.Replace("filter,", "{},"));

    string[] jsfiles = Directory.GetFiles(".\\app\\dist\\apps\\client\\", "main.*.js");
    try
    {
        FileInfo mainjs = new(jsfiles[0]);
        Console.WriteLine($"Patching {mainjs.Name}");
        string newmainjs = File.ReadAllText(mainjs.FullName);
        Match jscdn = Regex.Match(newmainjs, @"\w+\.setAttribute\(""src"",.https:/.+?\.js.\),");
        if (jscdn.Success)
        {
            //newmainjs = newmainjs
            File.WriteAllText(mainjs.FullName, newmainjs.Remove(jscdn.Index, jscdn.Length));

        } else
        {
            Console.WriteLine("Could not locate cdn attribute");
        }
    } catch(ArgumentNullException e)
    {
        Console.WriteLine("Could not patch out ad provider directly");
        Console.WriteLine("Trace: " + e.StackTrace);
        Console.ReadKey();
    }
    


    using (AsarArchiver aa = new(".\\app", ".\\app.asar"))
    {
        aa.Archive();
    }

    Console.WriteLine("Cleaning Up");

    Directory.Delete(".\\app", true);

    File.Copy(".\\app.asar", tcasar.FullName, true);
    File.Delete(".\\app.asar");

    Console.WriteLine("Done.\nPress anything to exit");
    Console.ReadKey();
}
else
{
    Console.WriteLine($"Error: Could not locate latest release from {version.FullName}\nPress anything to exit");
    Console.ReadKey();
}

