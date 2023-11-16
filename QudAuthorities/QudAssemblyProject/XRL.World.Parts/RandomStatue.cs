using System;
using Qud.API;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class RandomStatue : IPart
{
	public string Material = "Stone";

	public string BaseBlueprint;

	public void SetCreature(GameObject gameObject)
	{
		ParentObject.pRender.DisplayName = ParentObject.GetBlueprint().GetPartParameter("Render", "DisplayName");
		_SetCreature(gameObject);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectCreated");
		base.Register(Object);
	}

	private void _SetCreature(GameObject creatureObject)
	{
		BaseBlueprint = creatureObject.Blueprint;
		ParentObject.pRender.DisplayName = Grammar.InitLower(Material) + " statue of *creature.a**creature*";
		ParentObject.pRender.DisplayName = ParentObject.pRender.DisplayName.Replace("*creature*", creatureObject.DisplayNameOnlyDirectAndStripped).Replace("*creature.a*", creatureObject.a);
		ParentObject.pRender.RenderString = creatureObject.pRender.RenderString;
		ParentObject.pRender.Tile = creatureObject.pRender.Tile;
		Description description = creatureObject.GetPart("Description") as Description;
		Description obj = ParentObject.GetPart("Description") as Description;
		obj._Short = "This statue worked from *material* intricately depicts *creature.a**creature*:\n\n";
		obj._Short = obj._Short.Replace("*material*", Material).Replace("*creature.a*", creatureObject.a).Replace("*creature*", creatureObject.DisplayNameOnlyDirectAndStripped) + GameText.VariableReplace(description._Short, creatureObject);
		if (creatureObject.HasPart("Lovely") && !ParentObject.HasPart("Lovely"))
		{
			ParentObject.AddPart(new Lovely());
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated")
		{
			GameObject creatureObject = ((BaseBlueprint == null) ? EncountersAPI.GetACreature() : GameObjectFactory.Factory.CreateObject(BaseBlueprint));
			_SetCreature(creatureObject);
		}
		return base.FireEvent(E);
	}
}
