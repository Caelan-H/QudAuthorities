using System;

namespace XRL.World.Parts;

[Serializable]
public class Yurtmat : IPart
{
	public int Bonus = 2;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "GetShortDescription");
		base.Register(Object);
	}

	private void CheckCamouflage()
	{
		GameObject equipped = ParentObject.pPhysics.Equipped;
		if (equipped != null && equipped.pPhysics.CurrentCell != null)
		{
			if (equipped.pPhysics.CurrentCell.HasObjectWithPart("PlantProperties"))
			{
				base.StatShifter.DefaultDisplayName = "camouflage";
				base.StatShifter.SetStatShift(equipped, "DV", Bonus);
			}
			else
			{
				base.StatShifter.RemoveStatShifts(equipped);
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			CheckCamouflage();
			return true;
		}
		if (E.ID == "Equipped")
		{
			E.GetParameter<GameObject>("EquippingObject").RegisterPartEvent(this, "EnteredCell");
			CheckCamouflage();
			return true;
		}
		if (E.ID == "Unequipped")
		{
			GameObject parameter = E.GetParameter<GameObject>("UnequippingObject");
			base.StatShifter.RemoveStatShifts(parameter);
			parameter.UnregisterPartEvent(this, "EnteredCell");
			return true;
		}
		if (E.ID == "GetShortDescription")
		{
			E.SetParameter("Postfix", E.GetStringParameter("Postfix") + "\n&C" + ((Bonus < 0) ? Bonus.ToString() : ("+" + Bonus)) + " DV while occupying the same tile as foliage");
		}
		return base.FireEvent(E);
	}
}
