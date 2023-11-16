using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class BurgeonOnHit : IPart
{
	public int Chance = 100;

	public string Level = "10";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" && Chance.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			UnwelcomeGermination.Germinate(gameObjectParameter, Stat.Roll(Level), 1, friendly: true, gameObjectParameter2.CurrentCell);
			IComponent<GameObject>.XDidY(gameObjectParameter, "cause", "several plants to germinate with the force of " + gameObjectParameter.its + ParentObject.DisplayNameOnlyDirect, "!", null, gameObjectParameter);
		}
		return base.FireEvent(E);
	}
}
