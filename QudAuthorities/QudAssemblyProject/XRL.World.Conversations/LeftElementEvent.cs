using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired after an element has successfully been exited.</summary>
[ConversationEvent(Action = Action.Send)]
public class LeftElementEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<LeftElementEvent> Pool;

	private static int PoolCounter;

	static LeftElementEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public LeftElementEvent()
		: base(ID)
	{
	}

	public static LeftElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static LeftElementEvent FromPool(IConversationElement Element)
	{
		LeftElementEvent leftElementEvent = FromPool();
		leftElementEvent.Element = Element;
		return leftElementEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			LeftElementEvent e = FromPool(Element);
			Element.HandleEvent(e);
		}
	}
}
