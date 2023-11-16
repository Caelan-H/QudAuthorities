using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.Liquids;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class ILiquidEvent : MinEvent
{
	public GameObject Actor;

	public string Liquid;

	public LiquidVolume LiquidVolume;

	public int Drams;

	public GameObject Skip;

	public List<GameObject> SkipList;

	public Predicate<GameObject> Filter;

	public bool Auto;

	public bool ImpureOkay;

	public bool SafeOnly;

	public new static int CascadeLevel => 7;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Actor = null;
		Liquid = null;
		LiquidVolume = null;
		Drams = 0;
		Skip = null;
		SkipList = null;
		Filter = null;
		Auto = false;
		ImpureOkay = false;
		SafeOnly = false;
		base.Reset();
	}

	public bool ApplyTo(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj == Skip)
		{
			return false;
		}
		if (SkipList != null && SkipList.Contains(obj))
		{
			return false;
		}
		if (Filter != null && !Filter(obj))
		{
			return false;
		}
		if (SafeOnly)
		{
			if (LiquidVolume == null)
			{
				BaseLiquid liquid = LiquidVolume.getLiquid(Liquid);
				if (liquid != null)
				{
					if (!liquid.SafeContainer(obj))
					{
						return false;
					}
				}
				else
				{
					LiquidVolume = new LiquidVolume(Liquid, 0);
				}
			}
			if (LiquidVolume != null && !LiquidVolume.SafeContainer(obj))
			{
				return false;
			}
		}
		return true;
	}
}
