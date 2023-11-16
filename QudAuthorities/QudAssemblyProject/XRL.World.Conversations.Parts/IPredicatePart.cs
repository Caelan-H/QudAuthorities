using System.Collections.Generic;

namespace XRL.World.Conversations.Parts;

/// <summary>Base consumer of predicates to satisfy condition for a derived action.</summary><seealso cref="T:XRL.World.Conversations.Parts.ChangeTarget" />
public abstract class IPredicatePart : IConversationPart
{
	public Dictionary<string, string> Predicates;

	public override void LoadAttributes(Dictionary<string, string> Attributes)
	{
		foreach (KeyValuePair<string, string> Attribute in Attributes)
		{
			if (ConversationDelegates.Predicates.ContainsKey(Attribute.Key))
			{
				if (Predicates == null)
				{
					Predicates = new Dictionary<string, string>();
				}
				Predicates[Attribute.Key] = Attribute.Value;
			}
		}
		base.LoadAttributes(Attributes);
	}

	public bool All(bool Default = true)
	{
		if (Predicates.IsNullOrEmpty())
		{
			return Default;
		}
		foreach (KeyValuePair<string, string> predicate in Predicates)
		{
			if (ConversationDelegates.Predicates.TryGetValue(predicate.Key, out var value) && !value(ParentElement, predicate.Value))
			{
				return false;
			}
		}
		return Default;
	}

	public bool Any(bool Default = false)
	{
		if (Predicates.IsNullOrEmpty())
		{
			return Default;
		}
		foreach (KeyValuePair<string, string> predicate in Predicates)
		{
			if (ConversationDelegates.Predicates.TryGetValue(predicate.Key, out var value) && value(ParentElement, predicate.Value))
			{
				return true;
			}
		}
		return Default;
	}
}
