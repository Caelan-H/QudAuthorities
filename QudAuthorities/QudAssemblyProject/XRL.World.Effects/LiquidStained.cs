using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class LiquidStained : Effect
{
	public LiquidVolume Liquid;

	[NonSerialized]
	private static Dictionary<string, int> CleanseAmounts = new Dictionary<string, int>();

	public LiquidStained()
	{
		base.DisplayName = "stained by liquid";
		base.Duration = 1;
	}

	public LiquidStained(LiquidVolume Liquid)
	{
		this.Liquid = Liquid;
		base.Duration = 9999;
	}

	public LiquidStained(LiquidVolume From, int Drams, int Duration = 9999)
	{
		Liquid = From.Split(Drams);
		base.Duration = Duration;
	}

	public LiquidStained(string LiquidSpec, int Drams, int Duration = 9999)
	{
		Liquid = new LiquidVolume(LiquidSpec, Drams);
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 67109888;
	}

	public override bool SameAs(Effect e)
	{
		LiquidStained liquidStained = e as LiquidStained;
		if (Liquid == null != (liquidStained.Liquid == null))
		{
			return false;
		}
		if (Liquid != null && !liquidStained.Liquid.SameAs(Liquid))
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool CanApplyToStack()
	{
		return true;
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		if (Reader.FileVersion >= 150)
		{
			Dictionary<string, int> componentLiquids = Reader.ReadDictionary<string, int>();
			if (Liquid != null)
			{
				Liquid.ComponentLiquids = componentLiquids;
			}
		}
		else if (Liquid != null && Liquid.Primary != null)
		{
			if (Liquid.Secondary != null)
			{
				Liquid.InitialLiquid = Liquid.Primary + "-750," + Liquid.Secondary + "-250";
			}
			else
			{
				Liquid.InitialLiquid = Liquid.Primary;
			}
		}
	}

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		Writer.Write(Liquid?.ComponentLiquids);
	}

	public override string GetDescription()
	{
		if (IsConcealedByLiquid())
		{
			return null;
		}
		if (Liquid == null)
		{
			return "stained by liquid";
		}
		string stainedName = Liquid.StainedName;
		string stainedColor = Liquid.StainedColor;
		if (!string.IsNullOrEmpty(stainedColor))
		{
			return "{{" + stainedColor + "|" + stainedName + "}}";
		}
		return stainedName;
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Stained by ").Append((Liquid == null) ? "liquid" : Liquid.GetLiquidName()).Append('.');
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (Liquid == null)
		{
			return false;
		}
		Liquid.Update();
		if (Object.GetEffect("LiquidStained") is LiquidStained liquidStained && liquidStained.Liquid != null)
		{
			liquidStained.Liquid.MixWith(Liquid);
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyLiquidStained", "Liquid", Liquid)))
		{
			return false;
		}
		Object.FireEvent(Event.New("AppliedLiquidStained", "Liquid", Liquid));
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CheckAnythingToCleanEvent.ID && ID != CleanItemsEvent.ID && ID != EndTurnEvent.ID && ID != GetDisplayNameEvent.ID)
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckAnythingToCleanEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CleanItemsEvent E)
	{
		E.RegisterObject(base.Object);
		E.RegisterType("stains");
		if (Liquid != null)
		{
			Liquid.Volume = 1;
			Liquid.FlowIntoCell(-1, base.Object.GetCurrentCell(), E.Actor);
		}
		base.Object.RemoveEffect(this, NeedStackCheck: false);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (Liquid != null && !IsConcealedByLiquid())
		{
			Liquid.ProcessStain(E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (Liquid != null)
		{
			foreach (string key in Liquid.ComponentLiquids.Keys)
			{
				LiquidVolume.getLiquid(key).StainElements(Liquid, E);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && base.Object != null && base.Object.pRender != null && Liquid != null && !IsConcealedByLiquid())
		{
			Liquid.RenderStain(E, base.Object);
		}
		return true;
	}

	public bool IsConcealedByLiquid()
	{
		if (Liquid == null)
		{
			return false;
		}
		if (!(base.Object.GetEffect("LiquidCovered") is LiquidCovered liquidCovered) || liquidCovered.Liquid == null)
		{
			return false;
		}
		if (Liquid.Primary == null)
		{
			return true;
		}
		if (liquidCovered.Liquid.ComponentLiquids.ContainsKey(Liquid.Primary))
		{
			if (Liquid.Secondary == null)
			{
				return true;
			}
			if (liquidCovered.Liquid.ComponentLiquids.ContainsKey(Liquid.Secondary))
			{
				return true;
			}
		}
		return false;
	}

	public bool Cleanse(int Amount)
	{
		if (Liquid == null)
		{
			return false;
		}
		CleanseAmounts.Clear();
		int total = 0;
		for (int i = 0; i < Amount; i++)
		{
			string randomElement = Liquid.ComponentLiquids.GetRandomElement(ref total);
			if (randomElement == null)
			{
				MetricsManager.LogError("LiquidStained on " + base.Object.DebugName + " had null selected liquid from " + Liquid.GetLiquidDebugDesignation() + " on cleanse dram " + (i + 1) + " of " + Amount);
				return false;
			}
			if (CleanseAmounts.ContainsKey(randomElement))
			{
				CleanseAmounts[randomElement]++;
			}
			else
			{
				CleanseAmounts.Add(randomElement, 1);
			}
		}
		Liquid.UseDrams(CleanseAmounts);
		if (Liquid.Volume <= 0)
		{
			base.Object.RemoveEffect(this);
		}
		return true;
	}

	public override void WasUnstackedFrom(GameObject obj)
	{
		base.WasUnstackedFrom(obj);
		if (Liquid == null || !(obj.GetEffect("LiquidStained") is LiquidStained liquidStained) || liquidStained.Liquid == null || liquidStained.Liquid.Volume != Liquid.Volume)
		{
			return;
		}
		int count = base.Object.Count;
		int count2 = obj.Count;
		int num = Liquid.Volume * count / (count + count2);
		int num2 = Liquid.Volume * count2 / (count + count2);
		if (num + num2 < Liquid.Volume)
		{
			if (count > count2)
			{
				num++;
				if (num + num2 < Liquid.Volume)
				{
					num2++;
				}
			}
			else
			{
				num2++;
				if (num + num2 < Liquid.Volume)
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			Liquid.Volume = num;
		}
		else
		{
			Liquid.Empty();
			base.Duration = 0;
		}
		if (num2 > 0)
		{
			Liquid.Volume = num2;
			return;
		}
		liquidStained.Liquid.Empty();
		liquidStained.Duration = 0;
	}
}
