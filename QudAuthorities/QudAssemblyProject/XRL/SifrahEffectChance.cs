using System;
using XRL.World;

namespace XRL;

[Serializable]
public class SifrahEffectChance
{
	public string EffectName;

	public int Chance;

	public string Duration;

	public string DisplayName;

	public bool Stack;

	public bool Force;

	public Effect EffectInstance;

	public SifrahEffectChance()
	{
	}

	public SifrahEffectChance(string EffectName, int Chance, string Duration = null, string DisplayName = null, bool Stack = false, bool Force = false, Effect EffectInstance = null)
	{
		this.EffectName = EffectName;
		this.Chance = Chance;
		this.Duration = Duration;
		this.DisplayName = DisplayName;
		this.Stack = Stack;
		this.Force = Force;
		this.EffectInstance = EffectInstance;
	}

	public Effect GetEffectInstance()
	{
		if (EffectInstance == null)
		{
			EffectInstance = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + EffectName)) as Effect;
		}
		if (!string.IsNullOrEmpty(Duration))
		{
			EffectInstance.Duration = Duration.RollCached();
		}
		if (!string.IsNullOrEmpty(DisplayName))
		{
			EffectInstance.DisplayName = DisplayName;
		}
		return EffectInstance;
	}

	public bool Apply(GameObject Subject)
	{
		Effect effectInstance = GetEffectInstance();
		if (Force)
		{
			Subject.ForceApplyEffect(effectInstance);
		}
		else
		{
			Subject.ApplyEffect(effectInstance);
		}
		return true;
	}
}
