using QupKit;
using XRL.UI;

[UIView("Popup:TradeAmount", false, false, false, "Trade,Menu", "Popup:TradeAmount", false, 0, false)]
public class TradeAmountView : BaseView
{
	public override void OnCommand(string Command)
	{
		if (Command.StartsWith("Accept"))
		{
			TradeAmountViewBehavior.instance.Accept();
		}
		if (Command.StartsWith("Back"))
		{
			LegacyViewManager.Instance.SetActiveView("Trade");
		}
		base.OnCommand(Command);
	}
}
