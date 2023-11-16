using System.Collections.Generic;

namespace XRL.World.Conversations;

[ConversationEvent(Action = Action.Send)]
public class ColorTextEvent : ConversationEvent
{
	public new static readonly int ID;

	private static List<ColorTextEvent> Pool;

	private static int PoolCounter;

	[Parameter(Reference = true)]
	public string Color;

	static ColorTextEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public ColorTextEvent()
		: base(ID)
	{
	}

	public static ColorTextEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ColorTextEvent FromPool(IConversationElement Element, string Color)
	{
		ColorTextEvent colorTextEvent = FromPool();
		colorTextEvent.Element = Element;
		colorTextEvent.Color = Color;
		return colorTextEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Color = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, ref string Color)
	{
		if (Element.WantEvent(ID))
		{
			ColorTextEvent colorTextEvent = FromPool(Element, Color);
			Element.HandleEvent(colorTextEvent);
			Color = colorTextEvent.Color;
		}
	}
}
