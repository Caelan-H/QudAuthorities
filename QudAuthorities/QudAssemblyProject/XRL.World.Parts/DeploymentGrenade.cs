using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class DeploymentGrenade : IGrenade
{
	public int Radius = 1;

	public int Chance = 100;

	public int AtLeast;

	public string Count;

	public string Duration;

	public string Blueprint = "Forcefield";

	public string UsabilityEvent;

	public string AccessibilityEvent;

	public string ActivationVerb = "detonates";

	public string PhaseDuration;

	public string OmniphaseDuration;

	public bool RealRadius;

	public bool BlockedBySolid = true;

	public bool BlockedByNonEmpty = true;

	public bool Seeping;

	public bool DustPuff = true;

	public bool DustPuffEach;

	public bool NoXPValue = true;

	public bool LoyalToThrower;

	public bool TriflingCompanion = true;

	public override bool SameAs(IPart p)
	{
		DeploymentGrenade deploymentGrenade = p as DeploymentGrenade;
		if (deploymentGrenade.Radius != Radius)
		{
			return false;
		}
		if (deploymentGrenade.Chance != Chance)
		{
			return false;
		}
		if (deploymentGrenade.AtLeast != AtLeast)
		{
			return false;
		}
		if (deploymentGrenade.Count != Count)
		{
			return false;
		}
		if (deploymentGrenade.Duration != Duration)
		{
			return false;
		}
		if (deploymentGrenade.Blueprint != Blueprint)
		{
			return false;
		}
		if (deploymentGrenade.UsabilityEvent != UsabilityEvent)
		{
			return false;
		}
		if (deploymentGrenade.AccessibilityEvent != AccessibilityEvent)
		{
			return false;
		}
		if (deploymentGrenade.ActivationVerb != ActivationVerb)
		{
			return false;
		}
		if (deploymentGrenade.PhaseDuration != PhaseDuration)
		{
			return false;
		}
		if (deploymentGrenade.OmniphaseDuration != OmniphaseDuration)
		{
			return false;
		}
		if (deploymentGrenade.RealRadius != RealRadius)
		{
			return false;
		}
		if (deploymentGrenade.BlockedBySolid != BlockedBySolid)
		{
			return false;
		}
		if (deploymentGrenade.BlockedByNonEmpty != BlockedByNonEmpty)
		{
			return false;
		}
		if (deploymentGrenade.Seeping != Seeping)
		{
			return false;
		}
		if (deploymentGrenade.DustPuff != DustPuff)
		{
			return false;
		}
		if (deploymentGrenade.DustPuffEach != DustPuffEach)
		{
			return false;
		}
		if (deploymentGrenade.NoXPValue != NoXPValue)
		{
			return false;
		}
		if (deploymentGrenade.LoyalToThrower != LoyalToThrower)
		{
			return false;
		}
		if (deploymentGrenade.TriflingCompanion != TriflingCompanion)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetComponentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	private bool CanDeploy(Cell C, Cell GC, Event Check, Dictionary<int, bool> Track, GameObject Actor, int Phase)
	{
		if (!Chance.in100())
		{
			return false;
		}
		if (RealRadius && C != GC && C.RealDistanceTo(GC) > (double)Radius)
		{
			return false;
		}
		if (BlockedBySolid && C.IsSolid(Seeping, Phase))
		{
			return false;
		}
		if (BlockedByNonEmpty && !C.IsEmpty())
		{
			return false;
		}
		if (Track != null && Track.ContainsKey(C.LocalCoordKey))
		{
			return false;
		}
		if (Check != null && !C.FireEvent(Check))
		{
			return false;
		}
		return true;
	}

	private void Deploy(Cell C, Dictionary<int, bool> Track, GameObject Actor, int Phase)
	{
		GameObject gameObject = GameObject.create(Blueprint);
		if (!string.IsNullOrEmpty(Duration))
		{
			gameObject.AddPart(new Temporary(Stat.Roll(Duration)));
		}
		switch (Phase)
		{
		case 2:
			gameObject.ForceApplyEffect(new Phased(string.IsNullOrEmpty(PhaseDuration) ? 9999 : PhaseDuration.RollCached()));
			break;
		case 3:
			gameObject.ForceApplyEffect(new Omniphase(string.IsNullOrEmpty(OmniphaseDuration) ? 9999 : OmniphaseDuration.RollCached()));
			break;
		}
		C.AddObject(gameObject);
		gameObject.MakeActive();
		if (NoXPValue && gameObject.HasStat("XPValue"))
		{
			gameObject.GetStat("XPValue").BaseValue = 0;
		}
		if (LoyalToThrower && Actor != null)
		{
			gameObject.BecomeCompanionOf(Actor, TriflingCompanion);
		}
		if (DustPuffEach)
		{
			gameObject.DustPuff();
		}
		if (Track != null)
		{
			Track[C.LocalCoordKey] = true;
		}
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		if (!string.IsNullOrEmpty(UsabilityEvent) && !ParentObject.FireEvent(UsabilityEvent))
		{
			return false;
		}
		PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, 0f, combat: true);
		DidX(ActivationVerb, null, "!");
		int phase = ParentObject.GetPhase();
		Event check = ((!string.IsNullOrEmpty(AccessibilityEvent)) ? Event.New(AccessibilityEvent) : null);
		Dictionary<int, bool> dictionary = ((AtLeast > 0 || !string.IsNullOrEmpty(Count)) ? new Dictionary<int, bool>() : null);
		List<Cell> localAdjacentCells = C.GetLocalAdjacentCells(Radius);
		if (string.IsNullOrEmpty(Count))
		{
			int num = 0;
			do
			{
				if (num == 1)
				{
					localAdjacentCells.ShuffleInPlace();
				}
				if (CanDeploy(C, C, check, dictionary, Actor, phase))
				{
					Deploy(C, dictionary, Actor, phase);
				}
				foreach (Cell item in localAdjacentCells)
				{
					if (num > 0 && dictionary.Count >= AtLeast)
					{
						break;
					}
					if (CanDeploy(item, C, check, dictionary, Actor, phase))
					{
						Deploy(item, dictionary, Actor, phase);
					}
				}
			}
			while (dictionary != null && dictionary.Count < AtLeast && ++num < 10);
		}
		else
		{
			localAdjacentCells.ShuffleInPlace();
			int num2 = Count.RollCached();
			int num3 = 0;
			while (num3 < num2)
			{
				Cell randomElement = localAdjacentCells.GetRandomElement();
				if (randomElement == null)
				{
					break;
				}
				if (CanDeploy(randomElement, C, check, dictionary, Actor, phase))
				{
					Deploy(randomElement, dictionary, Actor, phase);
					num3++;
				}
				localAdjacentCells.Remove(randomElement);
			}
		}
		if (DustPuff)
		{
			ParentObject.DustPuff();
		}
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
