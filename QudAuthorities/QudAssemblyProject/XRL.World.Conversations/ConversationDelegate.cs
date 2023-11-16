using System;

namespace XRL.World.Conversations;

/// <summary>
///             Indicates that a method is a conversation delegate.
///             Depending on the return type it is either registered as a Predicate (bool), Action (void) or Generator (IConversationPart).
///             </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ConversationDelegate : Attribute
{
	public string Key;

	/// <summary>
	///             If this delegate is a conversation predicate, create another delegate returning the negated result of the first.
	///             </summary>
	public bool Inverse = true;

	public string InverseKey;

	/// <summary>
	///             If this delegate is a conversation predicate or action, create another delegate with the speaker as the target instead of the player.
	///             </summary>
	public bool Speaker;

	public string SpeakerKey;

	/// <summary>
	///             The key used to call the inverse speaker delegate, defaults to IfSpeakerNotXYZ should the original key follow the IfXYZ PascalCase pattern.
	///             E.g. IfHavePart -&gt; IfSpeakerNotHavePart.
	///             </summary>
	public string SpeakerInverseKey;

	public bool Require = true;

	public string RequireKey;
}
