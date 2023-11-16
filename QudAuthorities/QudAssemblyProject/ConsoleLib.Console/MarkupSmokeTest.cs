using NUnit.Framework;

namespace ConsoleLib.Console;

public class MarkupSmokeTest
{
	[TestCase("no markup", "no markup")]
	[TestCase("&ykeeps starting y", "&ykeeps starting y")]
	[TestCase("{{y|keeps starting y}}", "&ykeeps starting y")]
	[TestCase("small {{g|mossy}} tube", "&ysmall &gmossy&y tube")]
	[TestCase("{{|small {{g|mossy}} tube}}", "small &gmossy&y tube")]
	[TestCase("{{c|&Kwant black}}", "&Kwant black")]
	[TestCase("{{y|want grey {{Y|white}} grey &Kblack}}", "&ywant grey &Ywhite&y grey &Kblack")]
	[TestCase("{{zetachrome|&Kwant bl&Kack zeta", "&Kwa&Mn&Yt &Cb&cl&Kac&Ck &Yz&Met&ma")]
	public void Transform(string input, string expected)
	{
		Assert.AreEqual(expected, Markup.Transform(input));
	}
}
