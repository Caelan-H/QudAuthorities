using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Raycat : IPart
{
	public Raycat()
	{
	}

	public Raycat(string _Says)
		: this()
	{
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectPetted");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectPetted" && E.GetGameObjectParameter("Petter") == IComponent<GameObject>.ThePlayer && !IComponent<GameObject>.ThePlayer.HasEffect("Luminous"))
		{
			IComponent<GameObject>.ThePlayer.ApplyEffect(new Luminous(100 + Stat.Random(0, 100)));
			Popup.Show("You start to glow.");
		}
		return base.FireEvent(E);
	}
}
