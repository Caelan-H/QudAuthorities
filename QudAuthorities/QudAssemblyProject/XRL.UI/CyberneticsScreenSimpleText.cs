namespace XRL.UI;

public class CyberneticsScreenSimpleText : CyberneticsScreen
{
	private TerminalScreen backTo;

	private int securityChance;

	public CyberneticsScreenSimpleText(string text, TerminalScreen backTo, int securityChance = 0)
	{
		this.backTo = backTo;
		this.securityChance = securityChance;
		mainText = text;
		Options.Add("<back>");
	}

	public override void Back()
	{
		base.terminal.checkSecurity(securityChance, backTo);
	}

	public override void Activate()
	{
		Back();
	}
}
