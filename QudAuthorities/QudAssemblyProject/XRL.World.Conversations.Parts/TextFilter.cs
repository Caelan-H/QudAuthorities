using XRL.Language;

namespace XRL.World.Conversations.Parts;

public class TextFilter : IConversationPart
{
	public string FilterID;

	public string Extras;

	public bool FormattingProtect = true;

	public TextFilter()
	{
		Priority = -1000;
	}

	public TextFilter(string ID, string Extras = null)
		: this()
	{
		FilterID = ID;
		this.Extras = Extras;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (!FilterID.IsNullOrEmpty())
		{
			string value = TextFilters.Filter(E.Text.ToString(), FilterID, Extras, FormattingProtect);
			E.Text.Clear().Append(value);
		}
		return base.HandleEvent(E);
	}
}
