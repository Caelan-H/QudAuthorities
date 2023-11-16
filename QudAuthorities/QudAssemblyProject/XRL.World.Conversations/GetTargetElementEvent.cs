using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired after leaving the current node and can control the navigation target.</summary>
[ConversationEvent(Action = Action.Send)]
public class GetTargetElementEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<GetTargetElementEvent> Pool;

	private static int PoolCounter;

	[Parameter(Reference = true)]
	public string Target;

	static GetTargetElementEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public GetTargetElementEvent()
		: base(ID)
	{
	}

	public static GetTargetElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetTargetElementEvent FromPool(IConversationElement Element, string Target)
	{
		GetTargetElementEvent getTargetElementEvent = FromPool();
		getTargetElementEvent.Element = Element;
		getTargetElementEvent.Target = Target;
		return getTargetElementEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Target = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, ref string Target)
	{
		if (Element.WantEvent(ID))
		{
			GetTargetElementEvent getTargetElementEvent = FromPool(Element, Target);
			Element.HandleEvent(getTargetElementEvent);
			Target = getTargetElementEvent.Target;
		}
	}
}
