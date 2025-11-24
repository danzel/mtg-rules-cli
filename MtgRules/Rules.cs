namespace MtgRulesCli;
internal class Rules
{
	private const string Uri = "https://media.wizards.com/2025/downloads/MagicCompRules%2020251114.txt";
	private static string FileName => new Uri(Uri).Segments.Last();

	private readonly string[] _lines;
	private readonly int _sectionsStartLine;
	private readonly int _glossaryStartLine;

	private Rules(string[] lines)
	{
		_lines = lines;

		_sectionsStartLine = Array.FindIndex(_lines, line => line == "Credits") + 1;
		_glossaryStartLine = Array.FindIndex(_lines, _sectionsStartLine, line => line == "Glossary") + 1;
	}

	public string[]? GetRulesForSectionOrGlossary(string item)
	{
		//if item starts with a number, it's a section
		if (char.IsDigit(item[0]))
		{
			//Make it in to a searchable string
			//If it looks like "100" then add a dot since those end with a dot in the rules txt
			var searchItem = item;
			if (char.IsDigit(item[^1]))
				searchItem += '.';
			//Other ones should end with a letter "100.1a", they don't need extra anything added
			else if (char.IsLetter(item[^1]))
			{ }
			else
				throw new NotImplementedException($"Not sure what to do with section '{item}'");

			//Find the start of the section
			var sectionStartLine = Array.FindIndex(_lines, _sectionsStartLine, line => line.StartsWith(searchItem + " ", StringComparison.InvariantCultureIgnoreCase));
			if (sectionStartLine == -1)
				return null;

			//Scan forwards while lines start with this string, or are blank, or don't start with a digit. This should find all lines in the section.
			var sectionEndLine = sectionStartLine + 1;
			while (_lines[sectionEndLine].StartsWith(item, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrWhiteSpace(_lines[sectionEndLine]) || !char.IsDigit(_lines[sectionEndLine].FirstOrDefault()))
			{
				sectionEndLine++;
			}

			return _lines[sectionStartLine..sectionEndLine];
		}
		else
		{
			//glossary
			var glossaryStartLine = Array.FindIndex(_lines, _glossaryStartLine, line => line.Equals(item, StringComparison.InvariantCultureIgnoreCase));
			if (glossaryStartLine == -1)
				return null;

			//Scan forwards until we hit a blank line
			var glossaryEndLine = glossaryStartLine + 1;
			while (!string.IsNullOrWhiteSpace(_lines[glossaryEndLine]))
			{
				glossaryEndLine++;
			}

			return _lines[glossaryStartLine..glossaryEndLine];
		}
	}

	public async static Task<Rules> Load()
	{
		var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MtgRulesCli", FileName);

		if (!File.Exists(path))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			using var client = new HttpClient();
			var data = await client.GetByteArrayAsync(Uri);
			await File.WriteAllBytesAsync(path, data);
		}

		var lines = await File.ReadAllLinesAsync(path);

		return new Rules(lines);
	}
}
