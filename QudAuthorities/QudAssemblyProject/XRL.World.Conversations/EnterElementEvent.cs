using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired as an element is being entered and can prevent navigation.</summary>
[ConversationEvent(Action = Action.Check)]
public class EnterElementEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<EnterElementEvent> Pool;

	private static int PoolCounter;

	static EnterElementEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public EnterElementEvent()
		: base(ID)
	{
	}

	public static EnterElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EnterElementEvent FromPool(IConversationElement Element)
	{
		EnterElementEvent enterElementEvent = FromPool();
		enterElementEvent.Element = Element;
		return enterElementEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static bool Check(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			EnterElementEvent e = FromPool(Element);
			return Element.HandleEvent(e);
		}
		return true;
	}
}
