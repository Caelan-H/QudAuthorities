using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ElectromagneticPulsed : Effect
{
	public ElectromagneticPulsed()
	{
		base.DisplayName = "pulsed";
	}

	public ElectromagneticPulsed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440576;
	}

	public override string GetDescription()
	{
		return "{{B|pulsed}}";
	}

	public override string GetDetails()
	{
		return "Deactivated (if robotic).\nCan't be activated (if artifact).";
	}

	private bool ShouldPulse(GameObject obj)
	{
		if (base.Object.HasPart("Body"))
		{
			return true;
		}
		Inventory inventory = base.Object.Inventory;
		if (inventory != null && inventory.GetObjectCountDirect() > 0)
		{
			return true;
		}
		if (!TransparentToEMPEvent.Check(base.Object))
		{
			return true;
		}
		if (base.Object.HasRegisteredEvent("ApplyEMP"))
		{
			return true;
		}
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("ElectromagneticPulsed"))
		{
			ElectromagneticPulsed electromagneticPulsed = Object.GetEffect("ElectromagneticPulsed") as ElectromagneticPulsed;
			if (electromagneticPulsed.Duration < base.Duration)
			{
				electromagneticPulsed.Duration = base.Duration;
			}
			return false;
		}
		if (!Object.HasRegisteredEvent("ApplyEMP") || Object.FireEvent(Event.New("ApplyEMP", "Duration", base.Duration)))
		{
			Inventory inventory = Object.Inventory;
			if (inventory != null)
			{
				bool flag = false;
				foreach (GameObject item in inventory.GetObjectsDirect())
				{
					if (ShouldPulse(item))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					foreach (GameObject item2 in new List<GameObject>(inventory.GetObjectsDirect()))
					{
						if (ShouldPulse(item2))
						{
							item2.ApplyEffect(new ElectromagneticPulsed(base.Duration));
						}
					}
				}
			}
			Body body = Object.Body;
			if (body != null)
			{
				foreach (BodyPart part in body.GetParts())
				{
					if (part.DefaultBehavior != null && ShouldPulse(part.DefaultBehavior))
					{
						part.DefaultBehavior.ApplyEffect(new ElectromagneticPulsed(base.Duration));
					}
					if (part.Equipped != null && ShouldPulse(part.Equipped))
					{
						part.Equipped.ApplyEffect(new ElectromagneticPulsed(base.Duration));
					}
					if (part.Cybernetics != null && ShouldPulse(part.Cybernetics))
					{
						part.Cybernetics.ApplyEffect(new ElectromagneticPulsed(base.Duration));
					}
				}
			}
			if (TransparentToEMPEvent.Check(Object))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Object != null && Stat.RandomCosmetic(1, 120) <= 2)
		{
			for (int i = 0; i < 2; i++)
			{
				base.Object.ParticleText("&Y" + (char)Stat.RandomCosmetic(191, 198), 0.2f, 20);
			}
			for (int j = 0; j < 2; j++)
			{
				base.Object.ParticleText("&W\u000f", 0.02f, 10);
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.AddTag("[{{W|EMP}}]", 40);
		return base.HandleEvent(E);
	}
}
