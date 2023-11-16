using System;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Mutating : Effect
{
	public string Population = "MutatingResults";

	public bool triggered;

	public int PermuteMutationsAt;

	public bool MutationsPermuted;

	public Mutating()
	{
		base.DisplayName = "{{M|mutating}}";
	}

	public Mutating(int Duration, string Population = "MutatingResults")
		: this()
	{
		base.Duration = Duration;
		this.Population = Population;
		PermuteMutationsAt = Duration / 2;
	}

	public Mutating(int Duration, int PermuteMutationsAt, string Population = "MutatingResults")
		: this(Duration, Population)
	{
		this.PermuteMutationsAt = PermuteMutationsAt;
	}

	public override int GetEffectType()
	{
		return 100663300;
	}

	public override bool SameAs(Effect e)
	{
		Mutating mutating = e as Mutating;
		if (mutating.Population != Population)
		{
			return false;
		}
		if (mutating.triggered != triggered)
		{
			return false;
		}
		if (mutating.PermuteMutationsAt != PermuteMutationsAt)
		{
			return false;
		}
		if (mutating.MutationsPermuted != MutationsPermuted)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "In the process of mutating.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.IsMutant() && !Object.IsTrueKin())
		{
			return false;
		}
		if (Object.HasEffect("Mutating"))
		{
			return false;
		}
		if (Object.FireEvent("ApplyMutating"))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You start to feel unstable.", 'M');
			}
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && base.Duration > 0)
		{
			base.Duration--;
			if (base.Duration < PermuteMutationsAt && !MutationsPermuted)
			{
				base.Object.PermuteRandomMutationBuys();
				if (base.Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You feel increasingly unstable.", 'M');
				}
			}
			if (base.Duration <= 0 && !triggered)
			{
				triggered = true;
				string blueprint = PopulationManager.RollOneFrom(Population).Blueprint;
				if (blueprint == "Mutation")
				{
					MutationEntry mutationEntry = MutationsAPI.FindRandomMutationFor(base.Object, (MutationEntry e) => !e.IsDefect());
					if (mutationEntry != null)
					{
						if (base.Object.IsPlayer())
						{
							Popup.Show("Your genome destabilizes and you gain a new mutation:\n\n{{W|" + mutationEntry.DisplayName + "}}");
							AchievementManager.SetAchievement("ACH_MUTATION_FROM_GAMMAMOTH");
						}
						MutationsAPI.ApplyMutationTo(base.Object, mutationEntry);
					}
				}
				else if (blueprint == "Defect")
				{
					MutationEntry mutationEntry2 = MutationsAPI.FindRandomMutationFor(base.Object, (MutationEntry e) => e.IsDefect(), null, allowMultipleDefects: true);
					if (mutationEntry2 != null)
					{
						if (base.Object.IsPlayer())
						{
							Popup.Show("You genome destabilizes and you gain a new defect:\n\n{{W|" + mutationEntry2.DisplayName + "}}");
						}
						MutationsAPI.ApplyMutationTo(base.Object, mutationEntry2);
					}
				}
				else if (blueprint.StartsWith("Points:"))
				{
					int num = Stat.Roll(blueprint.Split(':')[1]);
					if ((base.Object.IsMutant() || base.Object.IsTrueKin()) && base.Object.GainMP(num) && base.Object.IsPlayer())
					{
						Popup.Show("Your genome destabilizes and you gain " + Grammar.Cardinal(num) + " mutation " + ((num == 1) ? "point" : "points") + ".");
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
