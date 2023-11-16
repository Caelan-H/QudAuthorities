using System;
using Qud.API;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Monochrome : Effect
{
	public bool monochromeApplied;

	public int DrankCure;

	public Monochrome()
	{
		base.Duration = 1;
	}

	public override string GetDetails()
	{
		return "Sees shades of only a single color.";
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent("ApplyDisease") && ApplyEffectEvent.Check(Object, "Disease", this) && Object.FireEvent("ApplyMonochrome") && ApplyEffectEvent.Check(Object, "Monochrome", this) && Object.IsPlayer())
		{
			if (Object.GetEffect("Monochrome") is Monochrome monochrome)
			{
				monochrome.Duration += base.Duration;
				return false;
			}
			AchievementManager.SetAchievement("ACH_GET_MONOCHROME");
			Popup.Show("You have contracted monochrome! Color starts to seep out of the world.");
			JournalAPI.AddAccomplishment("You contracted monochrome.", "Woe to the scroundrels and dastards who conspired to have =name= contract univision!", "general", JournalAccomplishment.MuralCategory.BodyExperienceBad, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			monochromeApplied = true;
			GameManager.Instance.GreyscaleLevel++;
			The.Game.SetIntGameState("HasMonochrome", 1);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (monochromeApplied)
		{
			GameManager.Instance.GreyscaleLevel--;
			The.Game.RemoveIntGameState("HasMonochrome");
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "FlashbangHit");
		Object.RegisterEffectEvent(this, "DrinkingFrom");
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "FlashbangHit");
		Object.UnregisterEffectEvent(this, "DrinkingFrom");
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (DrankCure > 0)
			{
				DrankCure--;
			}
		}
		else if (E.ID == "FlashbangHit")
		{
			if (DrankCure > 0)
			{
				base.Duration = 0;
				DrankCure = 0;
				if (monochromeApplied && base.Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Color starts to seep into the world.");
					AchievementManager.SetAchievement("ACH_CURE_MONOCHROME");
					Popup.Show("You are cured of monochrome.");
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			LiquidVolume liquidVolume = E.GetGameObjectParameter("Container").LiquidVolume;
			string stringGameState = XRLCore.Core.Game.GetStringGameState("MonochromeCure");
			if (!liquidVolume.IsPureLiquid(stringGameState))
			{
				return true;
			}
			DrankCure = 51;
		}
		return base.FireEvent(E);
	}
}
