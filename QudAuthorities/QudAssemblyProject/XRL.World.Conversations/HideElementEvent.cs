using System.Collections.Generic;

namespace XRL.World.Conversations;

[ConversationEvent(Action = Action.Check)]
public class HideElementEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<HideElementEvent> Pool;

	private static int PoolCounter;

	public string Context;

	static HideElementEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public HideElementEvent()
		: base(ID)
	{
	}

	public static HideElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static HideElementEvent FromPool(IConversationElement Element, string Context = null)
	{
		HideElementEvent hideElementEvent = FromPool();
		hideElementEvent.Element = Element;
		hideElementEvent.Context = Context;
		return hideElementEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Context = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static bool Check(IConversationElement Element, string Context = null)
	{
		if (Element.WantEvent(ID))
		{
			HideElementEvent e = FromPool(Element, Context);
			return Element.HandleEvent(e);
		}
		return true;
	}
}
