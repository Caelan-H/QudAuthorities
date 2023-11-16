using System.Collections.Generic;
using System.Text;

namespace XRL.World.Conversations;

/// <summary>Fired when preparing spoken text for display after a node has been entered.</summary><remarks>This precedes the standard variable replacements like =subject.name= and allows setting a new Subject and Object.</remarks>
[ConversationEvent(Action = Action.Send)]
public class PrepareTextEvent : ITemplateTextEvent
{
	public new static readonly int ID;

	private static List<PrepareTextEvent> Pool;

	private static int PoolCounter;

	static PrepareTextEvent()
	{
		ID = ConversationEvent.AllocateID();
	}

	public PrepareTextEvent()
		: base(ID)
	{
	}

	public static PrepareTextEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PrepareTextEvent FromPool(IConversationElement Element, StringBuilder Text, GameObject Subject, GameObject Object)
	{
		PrepareTextEvent prepareTextEvent = FromPool();
		prepareTextEvent.Element = Element;
		prepareTextEvent.Text = Text;
		prepareTextEvent.Subject = Subject;
		prepareTextEvent.Object = Object;
		return prepareTextEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, StringBuilder Text, ref GameObject Subject, ref GameObject Object, out string ExplicitSubject, out bool ExplicitSubjectPlural, out string ExplicitObject, out bool ExplicitObjectPlural)
	{
		if (Element.WantEvent(ID))
		{
			PrepareTextEvent prepareTextEvent = FromPool(Element, Text, Subject, Object);
			Element.HandleEvent(prepareTextEvent);
			Subject = prepareTextEvent.Subject;
			Object = prepareTextEvent.Object;
			ExplicitSubject = prepareTextEvent.ExplicitSubject;
			ExplicitSubjectPlural = prepareTextEvent.ExplicitSubjectPlural;
			ExplicitObject = prepareTextEvent.ExplicitObject;
			ExplicitObjectPlural = prepareTextEvent.ExplicitObjectPlural;
		}
		else
		{
			ExplicitSubject = null;
			ExplicitSubjectPlural = false;
			ExplicitObject = null;
			ExplicitObjectPlural = false;
		}
	}
}
