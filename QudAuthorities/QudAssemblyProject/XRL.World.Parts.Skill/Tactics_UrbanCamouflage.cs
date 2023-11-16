using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_UrbanCamouflage : BaseSkill
{
	public int Level = 4;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	private void CheckCamouflage(bool ForceRemove = false)
	{
		UrbanCamouflaged urbanCamouflaged = ParentObject.GetEffect("UrbanCamouflaged") as UrbanCamouflaged;
		bool flag = urbanCamouflaged != null;
		if (!flag)
		{
			urbanCamouflaged = new UrbanCamouflaged();
		}
		if (!ForceRemove && ParentObject.CurrentCell != null && ParentObject.CurrentCell.HasObjectOtherThan(urbanCamouflaged.EnablesCamouflage, ParentObject))
		{
			if (!flag)
			{
				ParentObject.ApplyEffect(urbanCamouflaged);
			}
			urbanCamouflaged.SetContribution(ParentObject, Level);
		}
		else if (flag)
		{
			urbanCamouflaged.RemoveContribution(ParentObject);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" || E.ID == "EnteredCell")
		{
			CheckCamouflage();
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		CheckCamouflage();
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		CheckCamouflage(ForceRemove: true);
		return true;
	}
}
