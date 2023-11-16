using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class TorchProperties : IPart
{
	public int Fuel = 5000;

	public bool InHand;

	public GameObject LastThrower;

	public int nFrameOffset;

	public bool ChangeColorString;

	public bool ChangeDetailColor;

	private LightSource _pLight;

	private LightSource pLight
	{
		get
		{
			if (_pLight == null)
			{
				_pLight = ParentObject.GetPart("LightSource") as LightSource;
			}
			return _pLight;
		}
	}

	public override void SaveData(SerializationWriter Writer)
	{
		GameObject.validate(ref LastThrower);
		base.SaveData(Writer);
	}

	public override bool SameAs(IPart p)
	{
		if ((p as TorchProperties).Fuel != Fuel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterThrownEvent.ID && ID != EndTurnEvent.ID && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != RadiatesHeatEvent.ID && ID != SuspendingEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		LastThrower = E.Actor;
		Light();
		InHand = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (pLight.Lit)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!ParentObject.HasPart("Flame"))
		{
			if (pLight.Lit)
			{
				E.AddAdjective("lit", -40);
				E.AddTag("(" + getLitDescription() + ")", -40);
			}
			else
			{
				E.AddTag("(" + getUnlitDescription() + ")", -40);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		LastThrower = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (E.Part != null && E.Part.Type == "Hand")
		{
			Light();
			InHand = true;
		}
		else
		{
			Extinguish();
			InHand = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		InHand = false;
		Extinguish();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.validate(ref LastThrower);
		GameObject equipped = ParentObject.Equipped;
		if (equipped != null && equipped.OnWorldMap())
		{
			return true;
		}
		if (Options.AutoTorch && InHand)
		{
			if (pLight.Lit)
			{
				if (equipped != null && equipped.IsPlayer() && equipped.IsUnderSky() && IsDay())
				{
					Extinguish();
				}
			}
			else if (equipped != null && equipped.IsPlayer())
			{
				if (equipped.IsUnderSky() && IsNight())
				{
					Light();
				}
				else if (equipped.CurrentZone != null && equipped.CurrentZone.Z > 10)
				{
					Light();
				}
			}
		}
		if (pLight.Lit)
		{
			if (Fuel > 0)
			{
				Fuel--;
			}
			if (Fuel <= 0)
			{
				Extinguish();
				if (equipped != null && equipped.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your torch burns out!");
					AutoAct.Interrupt();
				}
				ParentObject.Destroy(null, Silent: true);
			}
			else
			{
				if (ParentObject.pPhysics.CurrentCell != null)
				{
					int phase = ParentObject.GetPhase();
					foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPartReadonly("Physics"))
					{
						if (item == ParentObject)
						{
							continue;
						}
						if (ParentObject.pPhysics.IsReal)
						{
							if (item.pPhysics.Temperature < 400 && Stat.Random(1, 4) == 1)
							{
								item.TemperatureChange(400, LastThrower, Radiant: false, MinAmbient: false, MaxAmbient: false, phase);
							}
						}
						else if (item.pPhysics.Temperature < 400 && Stat.Random(1, 4) == 1)
						{
							item.TemperatureChange(100, LastThrower, Radiant: false, MinAmbient: false, MaxAmbient: false, phase);
						}
					}
				}
				if (ParentObject.pPhysics.IsReal && ParentObject.CurrentCell != null && 8.in100())
				{
					ParentObject.Smoke();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (InHand && ParentObject.Equipped != null)
		{
			if (pLight.Lit)
			{
				E.AddAction("Extinguish", "extinguish", "TorchExtinguish", null, 'x', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
			}
			else
			{
				E.AddAction("Light", "light", "TorchLight", null, 'i', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "TorchLight")
		{
			if (E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				E.Actor.UseEnergy(1000);
				Light();
			}
		}
		else if (E.Command == "TorchExtinguish" && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			E.Actor.UseEnergy(1000);
			Extinguish();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AddedToInventory");
		Object.RegisterPartEvent(this, "ApplyWet");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AddedToInventory")
		{
			Extinguish();
			InHand = false;
		}
		else if (E.ID == "ApplyWet")
		{
			Extinguish();
		}
		return base.FireEvent(E);
	}

	public void Light()
	{
		if (Fuel > 0)
		{
			pLight.Lit = true;
			ParentObject.pRender.Tile = "Items/sw_torch_lit.png";
			ParentObject.pRender.DetailColor = "W";
		}
	}

	private string getLitDescription()
	{
		if (Fuel > 3500)
		{
			return "{{Y|blazing}}";
		}
		if (Fuel > 2000)
		{
			return "{{W|bright}}";
		}
		if (Fuel > 500)
		{
			return "{{w|dim}}";
		}
		return "{{r|smouldering}}";
	}

	private string getUnlitDescription()
	{
		if (Fuel > 3500)
		{
			return "unburnt";
		}
		if (Fuel > 2000)
		{
			return "half-burnt";
		}
		if (Fuel > 500)
		{
			return "mostly burnt";
		}
		return "nearly extinguished";
	}

	public void Extinguish()
	{
		pLight.Lit = false;
		ParentObject.pRender.Tile = "Items/sw_torch.png";
		ParentObject.pRender.DetailColor = "r";
	}

	public override bool Render(RenderEvent E)
	{
		if ((ChangeColorString || ChangeDetailColor) && pLight.Lit)
		{
			int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				nFrameOffset += Stat.Random(1, 5);
			}
			char c = 'W';
			if (num < 15)
			{
				c = 'R';
			}
			else if (num >= 30 && num < 45)
			{
				c = 'r';
			}
			if (ChangeColorString || (ChangeDetailColor && !Options.UseTiles))
			{
				E.ColorString = "&" + c;
			}
			if (ChangeDetailColor)
			{
				E.DetailColor = c.ToString() ?? "";
			}
		}
		return true;
	}
}
