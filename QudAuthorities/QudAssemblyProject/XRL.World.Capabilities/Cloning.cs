using System;
using Qud.API;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Capabilities;

public static class Cloning
{
	public static bool CanBeCloned(GameObject Object, GameObject Actor = null, string Context = null)
	{
		if (Object.HasPropertyOrTag("Noclone"))
		{
			return false;
		}
		if (!Object.IsAlive)
		{
			return false;
		}
		if (!Effect.CanEffectTypeBeAppliedTo(16, Object))
		{
			return false;
		}
		if (!CanBeReplicatedEvent.Check(Object, Actor, Context))
		{
			return false;
		}
		return true;
	}

	private static void PostprocessClone(GameObject original, GameObject clone, GameObject actor, bool DuplicateGear = false, bool BecomesCompanion = true, bool Budded = false, string Context = null)
	{
		if (!DuplicateGear)
		{
			clone.StripContents(KeepNatural: true, Silent: true);
		}
		clone.RestorePristineHealth();
		clone.HasProperName = false;
		clone.RemoveIntProperty("Renamed");
		clone.ModIntProperty("IsClone", 1);
		clone.SetStringProperty("CloneOf", original.id);
		if (Budded)
		{
			clone.ModIntProperty("IsBuddedClone", 1);
		}
		if (clone.pRender != null && !original.HasPropertyOrTag("CloneNoNameChange") && !original.BaseDisplayName.Contains("clone of"))
		{
			if (original.HasProperName)
			{
				clone.pRender.DisplayName = "clone of " + original.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: true, WithIndefiniteArticle: true);
			}
			else
			{
				clone.pRender.DisplayName = clone.GetBlueprint().DisplayName();
			}
		}
		if (BecomesCompanion)
		{
			clone.BecomeCompanionOf(original);
		}
		WasReplicatedEvent.Send(original, actor, clone, Context);
		ReplicaCreatedEvent.Send(clone, actor, original, Context);
	}

	public static GameObject GenerateClone(GameObject original, GameObject actor = null, Cell C = null, bool DuplicateGear = false, bool BecomesCompanion = true, bool Budded = false, string Context = null, Func<GameObject, GameObject> MapInv = null)
	{
		GameObject gameObject = null;
		try
		{
			original.FireEvent("BeforeBeingCloned");
			gameObject = original.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
		}
		finally
		{
			original.FireEvent("AfterBeingCloned");
		}
		if (gameObject == null)
		{
			return null;
		}
		PostprocessClone(original, gameObject, actor, DuplicateGear, BecomesCompanion, Budded, Context);
		if (C != null)
		{
			C.AddObject(gameObject);
			gameObject.MakeActive();
			if (original.IsPlayer() && !AchievementManager.GetAchievement("ACH_30_CLONES"))
			{
				QueueAchievementCheck();
			}
		}
		return gameObject;
	}

	public static GameObject GenerateClone(GameObject original, GameObject actor = null, bool DuplicateGear = false, bool BecomesCompanion = true, bool Budded = false, string Context = null, int MaxRadius = 1)
	{
		if (original.CurrentCell != null && !original.OnWorldMap())
		{
			Cell firstEmptyAdjacentCell = original.CurrentCell.GetFirstEmptyAdjacentCell(1, MaxRadius);
			if (firstEmptyAdjacentCell != null)
			{
				return GenerateClone(original, actor, firstEmptyAdjacentCell, DuplicateGear, BecomesCompanion, Budded, Context);
			}
		}
		return null;
	}

	public static GameObject GenerateBuddedClone(GameObject original, Cell C, GameObject actor = null, bool DuplicateGear = false, bool BecomesCompanion = true, string Context = "Budding")
	{
		return StartBuddedClone(original, GenerateClone(original, actor, C, DuplicateGear, BecomesCompanion, Budded: true, Context));
	}

	public static GameObject GenerateBuddedClone(GameObject original, GameObject actor = null, bool DuplicateGear = false, bool BecomesCompanion = true, int MaxRadius = 1, string Context = "Budding")
	{
		return StartBuddedClone(original, GenerateClone(original, actor, DuplicateGear, BecomesCompanion, Budded: true, Context, MaxRadius));
	}

	private static GameObject StartBuddedClone(GameObject original, GameObject clone)
	{
		original.Bloodsplatter();
		if (original.IsPlayer())
		{
			Popup.Show(clone.A + clone.ShortDisplayName + clone.GetVerb("detach") + " from you!");
			JournalAPI.AddAccomplishment("On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", you multiplied.", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + " AR, =name= immaculately birthed " + The.Player.GetPronounProvider().Reflexive + ".", "general", JournalAccomplishment.MuralCategory.WeirdThingHappens, JournalAccomplishment.MuralWeight.High, null, -1L);
		}
		else if (original.IsVisible() || clone.IsVisible())
		{
			MessageQueue.AddPlayerMessage(clone.A + clone.ShortDisplayName + clone.GetVerb("detach") + " from " + original.the + original.ShortDisplayName + "!");
		}
		return clone;
	}

	public static void QueueAchievementCheck()
	{
		GameManager.Instance.gameQueue.queueSingletonTask("CloningAchievements", CheckAchievements);
	}

	public static void CheckAchievements()
	{
		int num = 0;
		string id = The.Player.id;
		Zone currentZone = The.Player.CurrentZone;
		for (int i = 0; i < currentZone.Width; i++)
		{
			for (int j = 0; j < currentZone.Height; j++)
			{
				int k = 0;
				for (int count = currentZone.Map[i][j].Objects.Count; k < count; k++)
				{
					GameObject gameObject = currentZone.Map[i][j].Objects[k];
					if (gameObject._Property != null && gameObject._Property.TryGetValue("CloneOf", out var value) && value == id)
					{
						num++;
					}
				}
			}
		}
		if (num >= 10)
		{
			AchievementManager.SetAchievement("ACH_10_CLONES");
		}
		if (num >= 30)
		{
			AchievementManager.SetAchievement("ACH_30_CLONES");
		}
	}
}
