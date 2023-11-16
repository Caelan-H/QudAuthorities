using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Gas : IPart
{
	public int _Density = 100;

	public int Level = 1;

	public bool Seeping;

	public bool Stable;

	public string GasType = "BaseGas";

	public string ColorString;

	[NonSerialized]
	private int FrameOffset;

	public GameObject _Creator;

	public GameObject Creator
	{
		get
		{
			GameObject.validate(ref _Creator);
			return _Creator;
		}
		set
		{
			_Creator = value;
		}
	}

	public int Density
	{
		get
		{
			return _Density;
		}
		set
		{
			if (_Density != value && ParentObject != null && ParentObject.HasRegisteredEvent("DensityChange"))
			{
				ParentObject.FireEvent(Event.New("DensityChange", "OldValue", _Density, "NewValue", value));
			}
			_Density = value;
		}
	}

	public Gas()
	{
		FrameOffset = Stat.Random(0, 60);
	}

	public override void Initialize()
	{
		if (ParentObject.pRender != null)
		{
			ParentObject.pRender.RenderString = "°";
			if (!string.IsNullOrEmpty(ColorString))
			{
				ParentObject.pRender.ColorString = ColorString;
			}
		}
	}

	public override bool SameAs(IPart p)
	{
		Gas gas = p as Gas;
		if (gas._Density != _Density)
		{
			return false;
		}
		if (gas.Level != Level)
		{
			return false;
		}
		if (gas.Seeping != Seeping)
		{
			return false;
		}
		if (gas.Stable != Stable)
		{
			return false;
		}
		if (gas.GasType != GasType)
		{
			return false;
		}
		if (gas.ColorString != ColorString)
		{
			return false;
		}
		if (gas._Creator != _Creator)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GeneralAmnestyEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetMatterPhaseEvent.ID && ID != GetMaximumLiquidExposureEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Density", Density);
		E.AddEntry(this, "Level", Level);
		E.AddEntry(this, "Seeping", Seeping);
		E.AddEntry(this, "Stable", Stable);
		E.AddEntry(this, "GasType", GasType);
		E.AddEntry(this, "ColorString", ColorString);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Creator = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMatterPhaseEvent E)
	{
		E.MinMatterPhase(3);
		return false;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		GameObject.validate(ref _Creator);
		if (CheckMergeGas(E.Object))
		{
			return false;
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
		ProcessGasBehavior();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (ProcessGasBehavior())
		{
			Disperse(10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (ProcessGasBehavior())
		{
			Disperse(100);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (ColorString != null)
		{
			E.ColorString = ColorString;
		}
		int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
		if (num < 15)
		{
			E.RenderString = "°";
		}
		else if (num < 30)
		{
			E.RenderString = "±";
		}
		else if (num < 45)
		{
			E.RenderString = "²";
		}
		else
		{
			E.RenderString = "Û";
		}
		if (Density < 50 && !GasType.Contains("Cryo"))
		{
			E.BackgroundString = "^k";
		}
		return true;
	}

	public bool ProcessGasBehavior()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		GameObject.validate(ref _Creator);
		if (Density > 10)
		{
			if (!Stable)
			{
				Density -= GetDispersalRate();
			}
			if (25.in100())
			{
				int i = 0;
				for (int num = Stat.Random(1, 4); i < num; i++)
				{
					Cell localCellFromDirection = cell.GetLocalCellFromDirection(Directions.GetRandomDirection());
					if (localCellFromDirection == null)
					{
						continue;
					}
					bool flag = false;
					List<GameObject> list = null;
					if (Seeping || !localCellFromDirection.IsSolidFor(ParentObject))
					{
						if (Stable)
						{
							flag = localCellFromDirection.IsEmpty();
							if (localCellFromDirection.GetObjectCountWithPart("Gas") > 0)
							{
								if (list == null)
								{
									list = Event.NewGameObjectList();
									localCellFromDirection.GetObjectsWithPart("Gas", list);
								}
								int j = 0;
								for (int count = list.Count; j < count; j++)
								{
									GameObject gameObject = list[j];
									if (gameObject.PhaseMatches(ParentObject))
									{
										ParentObject.FireEvent(Event.New("GasPressureOut", "Object", gameObject));
										gameObject.FireEvent(Event.New("GasPressureIn", "Object", ParentObject));
										flag = false;
									}
								}
							}
						}
						else
						{
							flag = true;
						}
					}
					if (!flag)
					{
						continue;
					}
					int num2 = Stat.Random(1, Math.Min(Density, 30));
					if (list == null)
					{
						list = Event.NewGameObjectList();
						localCellFromDirection.GetObjectsWithPart("Gas", list);
					}
					bool flag2 = false;
					int k = 0;
					for (int count2 = list.Count; k < count2; k++)
					{
						if (CheckMergeToGas(list[k], num2))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						GameObject gameObject2 = GameObject.create(ParentObject.Blueprint);
						Gas obj = gameObject2.GetPart("Gas") as Gas;
						obj.Creator = Creator;
						obj.Density = num2;
						obj.ColorString = ColorString;
						obj.Level = Level;
						obj.Seeping = Seeping;
						Density -= num2;
						gameObject2.FireEvent(Event.New("GasSpawned", "Parent", ParentObject));
						Temporary.CarryOver(ParentObject, gameObject2);
						Phase.carryOver(ParentObject, gameObject2);
						localCellFromDirection.AddObject(gameObject2);
					}
					if (Density <= 0)
					{
						break;
					}
				}
			}
		}
		if (Density <= 0 || (Density <= 10 && 50.in100()))
		{
			Dissipate();
			return false;
		}
		return true;
	}

	public void Dissipate()
	{
		if (ParentObject.IsPlayer() || ParentObject.HasTag("Creature"))
		{
			ParentObject.Die(null, "dissipation", "You dissipated.", ParentObject.It + " @@dissipated.", Accidental: true);
		}
		else
		{
			ParentObject.Obliterate(null, Silent: true);
		}
	}

	public void Disperse(int Factor = 1)
	{
		Density -= GetDispersalRate(Factor);
		if (Density < 0 || (Density <= 10 && 50.in100()))
		{
			Dissipate();
		}
	}

	public int GetDispersalRate(int Factor = 1)
	{
		int num = Stat.Random(Factor, Factor * 3);
		if (Creator != null && Creator.DistanceTo(ParentObject) <= 1 && Creator.HasRegisteredEvent("CreatorModifyGasDispersal"))
		{
			Event @event = new Event("CreatorModifyGasDispersal", "Rate", num);
			Creator.FireEvent(@event);
			num = @event.GetIntParameter("Rate");
		}
		return num;
	}

	private bool IsGasMergeable(Gas pGas)
	{
		if (pGas != null && pGas.GasType == GasType)
		{
			return pGas.ColorString == ColorString;
		}
		return false;
	}

	private void MergeGas(Gas pGas)
	{
		Density += pGas.Density;
		if (pGas.Level > Level)
		{
			Level = pGas.Level;
		}
		if (pGas.Seeping && !Seeping)
		{
			Seeping = true;
		}
		if (pGas.Creator != null && Creator == null)
		{
			Creator = pGas.Creator;
		}
	}

	private void MergeToGas(Gas pGas, int Contribution)
	{
		if (Contribution > Density)
		{
			Contribution = Density;
		}
		pGas.Density += Contribution;
		if (Level > pGas.Level)
		{
			pGas.Level = Level;
		}
		if (Seeping && !pGas.Seeping)
		{
			pGas.Seeping = true;
		}
		if (Creator != null && pGas.Creator == null)
		{
			pGas.Creator = Creator;
		}
		Density -= Contribution;
	}

	private bool CheckMergeGas(GameObject obj)
	{
		if (obj != ParentObject)
		{
			Gas gas = obj.GetPart("Gas") as Gas;
			if (IsGasMergeable(gas) && obj.PhaseMatches(ParentObject))
			{
				MergeGas(gas);
				gas.Dissipate();
				return true;
			}
		}
		return false;
	}

	private bool CheckMergeToGas(GameObject obj, int Contribution)
	{
		if (obj != ParentObject)
		{
			Gas pGas = obj.GetPart("Gas") as Gas;
			if (IsGasMergeable(pGas) && obj.PhaseMatches(ParentObject))
			{
				MergeToGas(pGas, Contribution);
				return true;
			}
		}
		return false;
	}
}
