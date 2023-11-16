using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Scintillating : Effect
{
	public int Level;

	public string Affected;

	public Scintillating()
	{
		base.DisplayName = "{{rainbow|scintillating}}";
	}

	public Scintillating(int Duration, int Level = 1)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{rainbow|scintillating}}";
	}

	public override string GetDetails()
	{
		return "Cannot take actions.\nConfuses nearby hostile creatures per Confusion 7.";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Scintillating"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyScintillating"))
		{
			return false;
		}
		DidX("start", "scintillating in {{rainbow|prismatic hues}}", "!");
		Object.ParticleText("*scintillating*", IComponent<GameObject>.ConsequentialColorChar(Object));
		Object.ForfeitTurn();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		DidX("stop", "scintillating");
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeBeginTakeActionEvent.ID)
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		ConfuseHostiles();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "IsMobile");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile" && base.Duration > 0)
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int currentFrame = XRLCore.CurrentFrame;
			if (currentFrame < 5)
			{
				E.ColorString = "&r";
				E.DetailColor = "R";
			}
			else if (currentFrame <= 10)
			{
				E.ColorString = "&W";
				E.DetailColor = "w";
			}
			else if (currentFrame <= 15)
			{
				E.ColorString = "&G";
				E.DetailColor = "g";
			}
			else if (currentFrame <= 20)
			{
				E.ColorString = "&C";
				E.DetailColor = "c";
			}
			else if (currentFrame <= 25)
			{
				E.ColorString = "&B";
				E.DetailColor = "b";
			}
			else if (currentFrame <= 35)
			{
				E.ColorString = "&M";
				E.DetailColor = "m";
			}
			else if (currentFrame <= 40)
			{
				E.ColorString = "&B";
				E.DetailColor = "b";
			}
			else if (currentFrame <= 45)
			{
				E.ColorString = "&C";
				E.DetailColor = "c";
			}
			else if (currentFrame <= 50)
			{
				E.ColorString = "&G";
				E.DetailColor = "g";
			}
			else if (currentFrame < 55)
			{
				E.ColorString = "&W";
				E.DetailColor = "w";
			}
			else
			{
				E.ColorString = "&r";
				E.DetailColor = "R";
			}
		}
		return true;
	}

	public void ConfuseHostiles()
	{
		List<GameObject> list = base.Object.CurrentZone?.FindObjectsWithPart("Brain");
		if (list == null || list.Count == 0)
		{
			return;
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if ((gameObject.IsHostileTowards(base.Object) || gameObject.IsHostileTowards(base.Object.PartyLeader)) && !HasAffected(gameObject) && gameObject.pBrain.CheckVisibilityOf(base.Object))
			{
				int level = Confusion.GetConfusionLevel(Level);
				int penalty = Confusion.GetMentalPenalty(Level);
				if (PerformMentalAttack((MentalAttackEvent E) => Confusion.Confuse(E, Attack: false, level, penalty), base.Object, gameObject, null, "Confuse", "1d8", 4, Confusion.GetDuration(Level).RollCached(), int.MinValue, Level))
				{
					MarkAffected(gameObject);
				}
			}
		}
	}

	public bool HasAffected(GameObject GO)
	{
		if (Affected == null)
		{
			return false;
		}
		if (!GO.hasid)
		{
			return false;
		}
		return Affected.CachedCommaExpansion().Contains(GO.id);
	}

	public void MarkAffected(GameObject GO)
	{
		if (Affected == null)
		{
			Affected = GO.id;
		}
		else
		{
			Affected = Affected + "," + GO.id;
		}
	}
}
