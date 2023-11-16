using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Esper : BaseMutation
{
	public Esper()
	{
		DisplayName = "Esper";
		Type = "Esper";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You only manifest mental mutations, and all of your mutation choices when manifesting a new mutation are mental.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}
}
