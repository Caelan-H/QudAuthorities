using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SkrefCorpseLoot : IPart
{
	public bool bCreated;

	public bool bSeen;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (!bSeen)
		{
			bSeen = true;
			Popup.ShowSpace("You stumble upon some flattened remains.");
			JournalAPI.AddAccomplishment("You stumbled upon some flattened remains.", "On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", a fallen cherub gifted his broken wings to the traveler =name=.", "general", JournalAccomplishment.MuralCategory.FindsObject, JournalAccomplishment.MuralWeight.High, null, -1L);
			AchievementManager.SetAchievement("ACH_FIND_APPRENTICE");
			JournalAPI.GetMapNote("$skrefcorpse")?.Reveal();
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			Cell cell = ParentObject.CurrentCell;
			GameObject gameObject = GameObject.create("Mechanical Wings");
			gameObject.ApplyEffect(new Broken());
			cell.AddObject(gameObject);
			cell.AddObject(GameObject.create("Wire Strand 50"));
			if (25.in100())
			{
				cell.AddObject(GameObject.create("Wire Strand 50"));
			}
			if (25.in100())
			{
				cell.AddObject(GameObject.create("Wire Strand 20"));
			}
			if (25.in100())
			{
				cell.AddObject(GameObject.create("Wire Strand 10"));
			}
			int i = 0;
			for (int num = Stat.Random(1, 2); i < num; i++)
			{
				cell.AddObject(PopulationManager.CreateOneFrom("Junk 3"));
			}
			cell.AddObject(PopulationManager.CreateOneFrom("Armor 2"));
			cell.AddObject(PopulationManager.CreateOneFrom("Armor 1"));
			cell.AddObject(PopulationManager.CreateOneFrom("Melee Weapons 2"));
			ParentObject.UnregisterPartEvent(this, "EnteredCell");
			ParentObject.Bloodsplatter();
		}
		return base.FireEvent(E);
	}
}
