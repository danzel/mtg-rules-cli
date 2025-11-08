if (args.Length < 1)
{
	Console.WriteLine("Usage: MtgRulesCli <section|glossary term>");
	return;
}
var item = string.Join(' ', args);

var rules = await MtgRulesCli.Rules.Load();
var text = rules.GetRulesForSectionOrGlossary(item);

if (text == null)
{
	Console.WriteLine($"No rules found for '{item}'");
	return;
}
foreach (var line in text)
{
	Console.WriteLine(line);
}