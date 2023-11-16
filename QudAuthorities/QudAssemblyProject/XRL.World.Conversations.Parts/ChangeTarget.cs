namespace XRL.World.Conversations.Parts;

/// <summary>Change target element if all/any predicates are satisfied.</summary>
public class ChangeTarget : IPredicatePart
{
	public string Target;

	/// <summary>Require one predicate to match rather than all.</summary>
	public new bool Any;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == GetTargetElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTargetElementEvent E)
	{
		if (Any)
		{
			if (Any())
			{
				E.Target = Target;
			}
		}
		else if (All())
		{
			E.Target = Target;
		}
		return base.HandleEvent(E);
	}
}
