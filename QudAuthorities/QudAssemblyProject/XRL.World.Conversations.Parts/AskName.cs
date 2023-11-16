using XRL.Names;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class AskName : IConversationPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != EnteredElementEvent.ID)
		{
			return ID == IsElementVisibleEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		GameObject speaker = The.Speaker;
		if (!speaker.HasProperName)
		{
			speaker.DisplayName = NameMaker.MakeName(speaker);
			speaker.HasProperName = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		GameObject speaker = The.Speaker;
		if (!ConversationUI.StartNode.AllowEscape)
		{
			return false;
		}
		if (!GlobalConfig.GetBoolSetting("GeneralAskName"))
		{
			return false;
		}
		if (!speaker.IsCreature)
		{
			return false;
		}
		if (speaker.HasProperName)
		{
			return false;
		}
		if (speaker.HasPropertyOrTag("NoAskName"))
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
