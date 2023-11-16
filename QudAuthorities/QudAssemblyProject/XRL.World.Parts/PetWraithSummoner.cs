using System;

namespace XRL.World.Parts;

[Serializable]
public class PetWraithSummoner : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && IComponent<GameObject>.ThePlayer != null)
		{
			GameObject gameObject = GameObject.create("BrokenPhylactery");
			IComponent<GameObject>.ThePlayer.TakeObject(gameObject, Silent: false, 0);
			InventoryActionEvent.Check(gameObject, IComponent<GameObject>.ThePlayer, gameObject, "ActivateTemplarPhylactery");
			ParentObject.Obliterate();
		}
		return base.FireEvent(E);
	}
}
