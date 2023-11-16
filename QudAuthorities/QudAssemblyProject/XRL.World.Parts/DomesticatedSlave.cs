using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DomesticatedSlave : IBondedCompanion
{
	public DomesticatedSlave()
	{
	}

	public DomesticatedSlave(GameObject EnslavedBy = null, string Faction = "Templar", string NameAdjective = "domesticated", string NameClause = null, string ConversationID = "TemplarDomesticant", bool StripGear = false)
		: base(EnslavedBy, Faction, NameAdjective, NameClause, ConversationID, StripGear)
	{
	}

	public override void InitializeBondedCompanion()
	{
		string filterExtras = ParentObject.GetxTag("TextFragments", "YounglingNoise");
		ParentObject.RemovePart("Preacher");
		Preacher preacher = ParentObject.AddPart<Preacher>();
		preacher.Book = "TemplarDomesticant";
		preacher.Filter = "Lallated";
		preacher.FilterExtras = filterExtras;
		preacher.Prefix = ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("say") + ", &W'";
		preacher.ChatWait = Stat.Random(20, 30);
		preacher.Volume = 10;
		preacher.SmartUse = false;
		ConversationScript conversationScript = ParentObject.RequirePart<ConversationScript>();
		conversationScript.Filter = "Lallated";
		conversationScript.FilterExtras = filterExtras;
		GameObjectBlueprint blueprint = ParentObject.GetBlueprint();
		GameObject gameObject = null;
		string text = null;
		if (!blueprint.IsBodyPartOccupied("Face"))
		{
			gameObject = GameObject.create("Gentling Mask");
			text = "Face";
		}
		else
		{
			gameObject = GameObject.create("Gentling Collar");
			text = "Head";
		}
		if (gameObject != null)
		{
			gameObject.RequirePart<SlaveMask>();
			ParentObject.ForceEquipObject(gameObject, text);
		}
	}
}
