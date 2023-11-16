using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MoteProperties : IPart
{
	public int Fuel = 5000;

	public int nFrameOffset;

	[NonSerialized]
	private LightSource _pLight;

	public LightSource pLight
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

	public override bool SameAs(IPart p)
	{
		if ((p as MoteProperties).Fuel != Fuel)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != AfterThrownEvent.ID && ID != EndTurnEvent.ID && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (pLight != null && pLight.Lit)
		{
			if (Fuel > 3500)
			{
				pLight.Radius = 6;
			}
			else if (Fuel > 2000)
			{
				pLight.Radius = 5;
			}
			else if (Fuel > 500)
			{
				pLight.Radius = 4;
			}
			else
			{
				pLight.Radius = 3;
			}
			if (Fuel > 0)
			{
				Fuel--;
			}
			if (Fuel <= 0)
			{
				if (ParentObject.Equipped != null && ParentObject.Equipped.IsPlayer())
				{
					pLight.Lit = false;
					IComponent<GameObject>.AddPlayerMessage((ParentObject.HasProperName ? ColorUtility.CapitalizeExceptFormatting(ParentObject.ShortDisplayName) : ("Your " + ParentObject.ShortDisplayName)) + ParentObject.GetVerb("dissipate") + ".");
				}
				ParentObject.Destroy();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (pLight != null && pLight.Lit && ParentObject.IsReal && ParentObject.IsTakeable())
		{
			E.AddTag("(" + getLitDescription() + ")");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void Light()
	{
		if (Fuel > 0 && pLight != null)
		{
			pLight.Lit = true;
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
			return "dim";
		}
		return "{{K|faint}}";
	}

	public override bool Render(RenderEvent E)
	{
		if (pLight != null && pLight.Lit)
		{
			int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				nFrameOffset += Stat.Random(1, 5);
			}
			if (num < 15)
			{
				E.ColorString = "&Y";
			}
			else if (num < 30)
			{
				E.ColorString = "&W";
			}
			else if (num < 45)
			{
				E.ColorString = "&C";
			}
			else
			{
				E.ColorString = "&W";
			}
		}
		return true;
	}
}
