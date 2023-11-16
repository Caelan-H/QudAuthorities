using System.Collections.Generic;

namespace XRL.World.Conversations;

[ConversationEvent(Action = Action.Check)]
public class IsElementVisibleEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<IsElementVisibleEvent> Pool;

	private static int PoolCounter;

	static IsElementVisibleEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public IsElementVisibleEvent()
		: base(ID)
	{
	}

	public static IsElementVisibleEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static IsElementVisibleEvent FromPool(IConversationElement Element)
	{
		IsElementVisibleEvent isElementVisibleEvent = FromPool();
		isElementVisibleEvent.Element = Element;
		return isElementVisibleEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static bool Check(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			IsElementVisibleEvent e = FromPool(Element);
			return Element.HandleEvent(e);
		}
		return true;
	}
}
