using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsFireSuppressionSystem : IPart
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public string Amount = "2-4";

	public string Sound = "SplashStep1";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && implantee.IsAflame() && implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			int num = Amount.RollCached();
			string name = LiquidVolume.getLiquid("gel").GetName(null);
			if (implantee.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your " + ParentObject.DisplayNameOnlyDirect + " discharges " + num + " " + ((num == 1) ? "dram" : "drams") + " of " + name + " all over you.");
			}
			else if (IComponent<GameObject>.Visible(implantee))
			{
				IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(implantee.DisplayNameOnlyDirect) + " " + ParentObject.DisplayNameOnlyDirect + " discharges " + num + " " + ((num == 1) ? "dram" : "drams") + " of " + name + " all over " + implantee.them + ".");
			}
			implantee.ApplyEffect(new LiquidCovered("gel", num));
			implantee.LiquidSplash(LiquidVolume.getLiquid("gel"));
			PlayWorldSound(Sound);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Fire Suppression", "CommandToggleFireSuppressionSystem", "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleFireSuppressionSystem" && E.Actor == ParentObject.Implantee)
		{
			ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}
}
