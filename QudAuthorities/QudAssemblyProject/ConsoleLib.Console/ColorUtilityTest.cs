using NUnit.Framework;

namespace ConsoleLib.Console;

public class ColorUtilityTest
{
	[TestCase("{{B|wet &MScorwofagoofouz, the nimble Snapjaw Gutspiller}}", 20, "{{B|wet &MScorwofagoofouz,}}")]
	[TestCase("{{red|12345{{green|67890}}}}", 7, "{{red|12345{{green|67}}}}")]
	[TestCase("&M1&G2&B3&Y4&W5", 3, "&M1&G2&B3")]
	public void TestClipExceptFormatting(string input, int length, string expected)
	{
		Assert.AreEqual(expected, ColorUtility.ClipExceptFormatting(input, length));
	}
}
