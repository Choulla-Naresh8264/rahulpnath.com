<Query Kind="Program">
  <Namespace>System.Globalization</Namespace>
</Query>

void Main()
{
var groupByFormat = "yyy";
	var allYears = GetAllYearsTillDateFromPostStart().ToList();
	var files = Directory.GetFiles(@"C:\Users\rahulpnath\Documents\Work\Personal\rahulpnath.com\source\_posts").Select(a => Path.GetFileName(a));
	var dateFiles = files.Select(a => new { Title = a, Date = DateTime.Parse(a.Substring(0, 10)) }).OrderBy(a => a.Date);

	var filesGroupedByYear = dateFiles.ToLookup(a => a.Date.ToString(groupByFormat));

	var allDat = allYears.Select(a => new { YearMonth = a.ToString(groupByFormat), Data = filesGroupedByYear[a.ToString(groupByFormat)]});



	var list = allDat.Select(a => new { Year = a.YearMonth , Posts = a.Data.Aggregate(string.Empty, (p, b) => p = p + GetPostTitle(b.Title) + "\\n"), NumberOfPosts = a.Data.Count() });

	var template = "['{0}',{1},'{2}']";
	var output = string.Join(",", list.Select(b => string.Format(template, b.Year, b.NumberOfPosts, b.Posts)));
	Console.WriteLine(output);
}

private IEnumerable<DateTime> GetAllYearsTillDateFromPostStart()
{
	var postStartMonth = new DateTime(2009, 06, 01);
	var current = postStartMonth;
	while (current.Year <= DateTime.Now.Year)
	{
		yield return current;
		current = current.AddYears(1);
	}
}

private string GetPostTitle(string fileName)
{
	TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
	return textInfo.ToTitleCase(fileName.Remove(0, 11).Replace("-", " ").Replace(".markdown", ""));
}
// Define other methods and classes here