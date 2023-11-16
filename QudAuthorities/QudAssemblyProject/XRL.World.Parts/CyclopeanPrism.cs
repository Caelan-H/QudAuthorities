using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyclopeanPrism : IPart
{
	public int TurnCount;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != EnteredCellEvent.ID && ID != GenericDeepNotifyEvent.ID && ID != GetPrecognitionRestoreGameStateEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == RealityStabilizeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (ParentObject.Equipped != null && E.Check())
		{
			ParentObject.Equipped.ApplyEffect(new Dazed(1));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericDeepNotifyEvent E)
	{
		if (E.Notify == "PrecognitionGameRestored")
		{
			string precognitionTransferKey = GetPrecognitionTransferKey();
			if (The.Game.GetBooleanGameState(precognitionTransferKey))
			{
				The.Game.RemoveBooleanGameState(precognitionTransferKey);
				if (ParentObject.Equipped == null)
				{
					PtohAnnoyed();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			AchievementManager.SetAchievement("ACH_WIELD_PRISM");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		Reset();
		if (!E.Actor.IsDying)
		{
			PtohAnnoyed(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (E.Object == ParentObject.Equipped)
		{
			TurnCount++;
			if (E.Object == null || !E.Object.HasStat("Ego") || !E.Object.HasStat("Willpower"))
			{
				Reset();
			}
			else
			{
				int num = E.Object.Stat("Ego");
				if (num <= 15)
				{
					ParentObject.pRender.DisplayName = "{{K|amaranthine}} prism";
				}
				else if (num <= 19)
				{
					ParentObject.pRender.DisplayName = "{{K|amara{{y|n}}thine}} prism";
				}
				else if (num <= 23)
				{
					ParentObject.pRender.DisplayName = "{{K|amar{{y|a{{Y|n}}t}}hine}} prism";
				}
				else if (num <= 27)
				{
					ParentObject.pRender.DisplayName = "{{K|am{{y|ar{{Y|a{{R|n}}t}}hi}}ne}} prism";
				}
				else if (num <= 31)
				{
					ParentObject.pRender.DisplayName = "{{y|am{{Y|a{{y|r{{r|a{{R|n}}t}}h}}i}}ne}} prism";
				}
				else
				{
					ParentObject.pRender.DisplayName = "{{r|a{{R|m{{Y|a{{y|r{{r|a{{R|n}}t}}h}}i}}n}}e}} prism";
				}
				if (TurnCount >= 4800)
				{
					if (E.Object.Stat("Willpower") <= 1)
					{
						E.Object.Die(ParentObject, null, "You had a dream, which was not all a dream. The bright sun was extinguish'd, and the stars did wander darkling in the eternal space.", ParentObject.It + " had a dream, which was not all a dream. The bright sun was extinguish'd, and the stars did wander darkling in the eternal space.");
					}
					Armor obj = ParentObject.GetPart("Armor") as Armor;
					obj.Ego++;
					obj.Willpower--;
					obj.UpdateStatShifts();
					TurnCount = 0;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Reset();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPrecognitionRestoreGameStateEvent E)
	{
		if (ParentObject.Equipped == E.Object)
		{
			E.Set(GetPrecognitionTransferKey(), true);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void PtohAnnoyed(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped ?? ParentObject.InInventory ?? The.Player;
			if (who == null)
			{
				return;
			}
		}
		if (who.IsPlayer())
		{
			Popup.Show("From across the psychic sea, you feel the glare of unseen eyes. Someone is disappointed in you.");
			The.Game.PlayerReputation.modify("highly entropic beings", -100);
			AchievementManager.SetAchievement("ACH_DISAPPOINT_HEB");
		}
		Cell cell = who.CurrentCell;
		if (cell == null || cell.ParentZone == null)
		{
			return;
		}
		for (int num = Stat.Random(2, 5); num >= 0; num--)
		{
			Cell cell2 = null;
			int num2 = 0;
			while (++num2 < 100)
			{
				cell2 = cell.ParentZone.GetRandomCell();
				int num3 = cell2.PathDistanceTo(cell);
				if (num3 >= 4 && num3 <= 12 && !cell2.HasObjectWithPart("Brain") && !cell2.HasObjectWithPart("SpaceTimeVortex") && !cell2.HasObjectWithPart("SpaceTimeRift") && cell2.FireEvent("CheckRealityDistortionAccessibility"))
				{
					break;
				}
			}
			if (num2 < 100)
			{
				cell2?.AddObject("Space-Time Vortex");
			}
		}
	}

	private void Reset()
	{
		Armor obj = ParentObject.GetPart("Armor") as Armor;
		obj.Ego = 1;
		obj.Willpower = -1;
		ParentObject.pRender.DisplayName = "{{K|amaranthine}} prism";
	}

	private string GetPrecognitionTransferKey()
	{
		return "AmaranthinePrism" + ParentObject.id + "EquippedAtPrecognitionRestore";
	}
}
