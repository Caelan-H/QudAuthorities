using NUnit.Framework;

namespace Qud.UI;

public class RTFSmokeTest
{
	[TestCase("{{y|want grey {{Y|white}} grey &Kblack}}", "<color=#B1C9C3FF>want grey </color><color=#FFFFFFFF>white</color><color=#B1C9C3FF> grey </color><color=#155352FF>black</color>")]
	public void FormatRTF(string input, string expected)
	{
		Assert.AreEqual(expected, RTF.FormatToRTF(input));
	}
}
