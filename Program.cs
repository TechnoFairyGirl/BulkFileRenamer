using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BulkFileRenamer
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: BulkFileRenamer [root directory path] [new name] [edit extensions]");
				Console.WriteLine("Edit extensions are pipe separated. Default is '.sln|.csproj|.cs'. Empty string disables editing.");
				Console.WriteLine("Making a backup prior to using this tool is highly recommended!");
				return;
			}

			var fileTypesToEdit = args.Length >= 3 ? 
				args[2].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) :
				new string[] { ".sln", ".csproj", ".cs" };

			var inputPath = Path.GetFullPath(args[0]);

			if (!Directory.Exists(inputPath))
			{
				Console.WriteLine("Root directory does not exist.");
				return;
			}

			var oldName = Path.GetFileName(inputPath);
			var newName = args[1];

			var allItems = new List<string>();

			void AddAllFiles(string path)
			{
				allItems.Add(path);
				allItems.AddRange(Directory.EnumerateFiles(path));
				var directories = Directory.EnumerateDirectories(path);
				foreach (var dir in directories) AddAllFiles(dir);
			}

			AddAllFiles(inputPath);

			for (var i = 0; i < allItems.Count; i++)
			{
				var path = allItems[i];
				var name = Path.GetFileName(path);

				if (name.Contains(oldName))
				{
					var parent = Path.GetDirectoryName(path);
					var newPath = Path.Combine(parent, name.Replace(oldName, newName));

					Console.WriteLine($"Renaming '{path}' to '{newPath}'.");

					if (Directory.Exists(path))
					{
						Directory.Move(path, newPath);

						for (var j = 0; j < allItems.Count; j++)
						{
							var item = allItems[j];
							if (item.StartsWith(path + Path.DirectorySeparatorChar))
								allItems[j] = newPath + item.Substring(path.Length);
						}
					}
					else if (File.Exists(path))
					{
						File.Move(path, newPath);
					}
					else
					{
						Console.WriteLine("Failed.");
						continue;
					}

					path = newPath;
					allItems[i] = newPath;
				}

				var extension = Path.GetExtension(path);

				if (fileTypesToEdit.Contains(extension))
				{
					Console.WriteLine($"Editing '{path}'.");

					var contents = File.ReadAllText(path);
					var newContents = contents.Replace(oldName, newName);
					File.WriteAllText(path, newContents);
				}
			}
		}
	}
}
