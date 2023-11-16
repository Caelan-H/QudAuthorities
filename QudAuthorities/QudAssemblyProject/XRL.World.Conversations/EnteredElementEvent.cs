using System.Collections.Generic;

namespace XRL.World.Conversations;

[ConversationEvent(Action = Action.Send)]
public class EnteredElementEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<EnteredElementEvent> Pool;

	private static int PoolCounter;

	static EnteredElementEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public EnteredElementEvent()
		: base(ID)
	{
	}

	public static EnteredElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EnteredElementEvent FromPool(IConversationElement Element)
	{
		EnteredElementEvent enteredElementEvent = FromPool();
		enteredElementEvent.Element = Element;
		return enteredElementEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			EnteredElementEvent e = FromPool(Element);
			Element.HandleEvent(e);
		}
	}
}
