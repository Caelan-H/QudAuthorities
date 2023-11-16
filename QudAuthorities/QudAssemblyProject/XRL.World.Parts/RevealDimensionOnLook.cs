using System;
using Qud.API;
using XRL.Rules;
using XRL.World.Encounters;

namespace XRL.World.Parts;

[Serializable]
public class RevealDimensionOnLook : IPart
{
	public string SecretID;

	public bool LookedAt;

	public RevealDimensionOnLook()
	{
	}

	public RevealDimensionOnLook(string SecretID)
	{
		this.SecretID = SecretID;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RevealDimensionOnLook).SecretID != SecretID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		CheckConfiguration();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLookedAt");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !LookedAt && CheckConfiguration() && ParentObject.Understood())
		{
			JournalAPI.RevealObservation(SecretID, onlyIfNotRevealed: true);
		}
		return base.FireEvent(E);
	}

	private bool CheckConfiguration()
	{
		if (string.IsNullOrEmpty(SecretID) && The.Game?.GetObjectGameState("PsychicManager") is PsychicManager psychicManager)
		{
			int num = Stat.Random(0, psychicManager.ExtraDimensions.Count + psychicManager.PsychicFactions.Count - 1);
			if (num < psychicManager.ExtraDimensions.Count)
			{
				ExtraDimension extraDimension = psychicManager.ExtraDimensions[num];
				SecretID = extraDimension.SecretID;
			}
			else
			{
				num -= psychicManager.ExtraDimensions.Count;
				PsychicFaction psychicFaction = psychicManager.PsychicFactions[num];
				SecretID = psychicFaction.dimensionSecretID;
			}
		}
		return !string.IsNullOrEmpty(SecretID);
	}
}
