using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DeathGate : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanAttemptDoorUnlock");
		Object.RegisterPartEvent(this, "AttemptDoorUnlock");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanAttemptDoorUnlock")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Opener");
			E.GetParameter("Door");
			if (ParentObject.pBrain != null)
			{
				return true;
			}
			if (gameObjectParameter == null)
			{
				return false;
			}
			if (!gameObjectParameter.IsPlayer())
			{
				return false;
			}
			if (!gameObjectParameter.HasMarkOfDeath())
			{
				return false;
			}
		}
		else if (E.ID == "AttemptDoorUnlock")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Opener");
			Door door = E.GetParameter("Door") as Door;
			if (ParentObject.pBrain != null)
			{
				return true;
			}
			if (gameObjectParameter2 == null)
			{
				return false;
			}
			if (!gameObjectParameter2.IsPlayer())
			{
				return false;
			}
			if (!gameObjectParameter2.HasMarkOfDeath())
			{
				Popup.ShowFail("The gates are sealed for eternity.");
				return false;
			}
			if (door.bLocked)
			{
				door.ParentObject.SetIntProperty("NoClose", 1);
				door.bLocked = false;
				Popup.Show("The gates swing wide.");
				JournalAPI.RevealObservation("MarkOfDeathSecret", onlyIfNotRevealed: true);
				The.Game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
				door.Open();
			}
		}
		return base.FireEvent(E);
	}

	public static void AddMarkOfDeath()
	{
		ReshephsCrypt.AddMarkOfDeath();
		The.Game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
	}
}
