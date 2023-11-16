using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, light radius is increased by the standard
///             power load bonus, i.e. 2 for the standard overload power load of 400.
///             </remarks>
[Serializable]
public class ActiveLightSource : IActivePart
{
	public bool Darkvision;

	public int Radius = 5;

	public bool ShowInShortDescription = true;

	public ActiveLightSource()
	{
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		ActiveLightSource activeLightSource = p as ActiveLightSource;
		if (activeLightSource.Darkvision != Darkvision)
		{
			return false;
		}
		if (activeLightSource.Radius != Radius)
		{
			return false;
		}
		if (activeLightSource.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell != null && WasReady())
		{
			int effectiveRadius = GetEffectiveRadius();
			if (Darkvision)
			{
				if (ParentObject.Equipped != null && ParentObject.Equipped.IsPlayer())
				{
					if (effectiveRadius == 999)
					{
						cell.ParentZone.LightAll();
					}
					else
					{
						cell.ParentZone.AddLight(cell.X, cell.Y, effectiveRadius, LightLevel.Darkvision);
					}
				}
			}
			else if (effectiveRadius == 999)
			{
				cell.ParentZone.LightAll();
			}
			else
			{
				cell.ParentZone.AddLight(cell.X, cell.Y, effectiveRadius, LightLevel.Light);
				if (IsNight() && cell.ParentZone.Z <= 10 && ParentObject.Equipped != null && ParentObject.Equipped.IsPlayer())
				{
					cell.ParentZone.AddExplored(cell.X, cell.Y, effectiveRadius * 2);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			List<string> list = new List<string>();
			bool flag = false;
			if (!WorksOnSelf)
			{
				if (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnImplantee)
				{
					list.Add("equipped");
				}
				else
				{
					flag = true;
				}
			}
			if (ChargeUse > 0 || ChargeMinimum > 0)
			{
				list.Add("powered");
			}
			if (flag || !string.IsNullOrEmpty(NeedsOtherActivePartOperational) || !string.IsNullOrEmpty(NeedsOtherActivePartEngaged))
			{
				list.Add("in use");
			}
			stringBuilder.Append((list.Count > 0) ? "When" : "While").Append(' ').Append((list.Count > 0) ? Grammar.MakeAndList(list) : "operational")
				.Append(", provides ")
				.Append(Darkvision ? "night vision" : "light");
			if (GetEffectiveRadius() != 999)
			{
				stringBuilder.Append(" in radius ").Append(GetEffectiveRadius()).Append('.');
			}
			E.Postfix.AppendRules(stringBuilder.ToString());
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: true);
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: true);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: true);
		}
	}

	public int GetEffectiveRadius()
	{
		if (Radius == 999)
		{
			return Radius;
		}
		return Radius + MyPowerLoadBonus();
	}
}
