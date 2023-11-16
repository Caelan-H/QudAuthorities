using System.Collections.Generic;

namespace XRL.World.Conversations;

[ConversationEvent(Action = Action.Send)]
public class GetTextElementEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<GetTextElementEvent> Pool;

	private static int PoolCounter;

	[Parameter(Required = true)]
	public List<ConversationText> Texts;

	[Parameter(Required = true)]
	public List<ConversationText> Visible;

	[Parameter(Required = true)]
	public List<ConversationText> Group;

	[Parameter(Reference = true)]
	public ConversationText Selected;

	static GetTextElementEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public GetTextElementEvent()
		: base(ID)
	{
	}

	public static GetTextElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetTextElementEvent FromPool(IConversationElement Element, List<ConversationText> Texts, List<ConversationText> Visible, List<ConversationText> Group, ConversationText Selected)
	{
		GetTextElementEvent getTextElementEvent = FromPool();
		getTextElementEvent.Element = Element;
		getTextElementEvent.Texts = Texts;
		getTextElementEvent.Visible = Visible;
		getTextElementEvent.Group = Group;
		getTextElementEvent.Selected = Selected;
		return getTextElementEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Texts = null;
		Visible = null;
		Group = null;
		Selected = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, List<ConversationText> Texts, List<ConversationText> Visible, List<ConversationText> Group, ref ConversationText Selected)
	{
		if (Element.WantEvent(ID))
		{
			GetTextElementEvent getTextElementEvent = FromPool(Element, Texts, Visible, Group, Selected);
			Element.HandleEvent(getTextElementEvent);
			Selected = getTextElementEvent.Selected;
		}
	}
}
