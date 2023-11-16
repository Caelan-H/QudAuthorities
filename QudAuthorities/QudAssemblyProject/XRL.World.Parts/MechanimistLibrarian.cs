using System;

namespace XRL.World.Parts;

[Serializable]
public class MechanimistLibrarian : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		try
		{
			ParentObject.DisplayName = "{{Y|Sheba Hagadias, " + ParentObject.DisplayNameOnlyUnconfused + " and librarian of the Stilt}}";
			ParentObject.HasProperName = true;
			ParentObject.pRender.ColorString = "&C";
			ParentObject.pRender.DetailColor = "W";
			ParentObject.GetStat("Intelligence").BaseValue = 27;
			ParentObject.RemovePart("Sitting");
			ParentObject.TakeObject("Cloth Robe", Silent: false, 0);
			ParentObject.TakeObject("Spectacles", Silent: false, 0);
			ParentObject.SetStringProperty("Mayor", "Mechanimists");
			(ParentObject.GetPart("Description") as Description).Short = "In the narthex of the Stilt, cloistered beneath a marble arch and close to =pronouns.possessive= Argent Fathers, =pronouns.subjective= =verb:muse:afterpronoun= over a tattered codex. =pronouns.Subjective==verb:'re:afterpronoun= safe here, but it wasn't always that way. As a youngling, =pronouns.possessive= own kind understood =pronouns.objective= little. Only when =pronouns.subjective= =verb:were:afterpronoun= gifted a copy of the Canticles Chromaic did =pronouns.subjective= learn comfort, or mirth, or reason. =pronouns.Possessive= journey to the Stilt took several years, but now that =pronouns.subjective==verb:'re:afterpronoun= here, Sheba =verb:seek= to consolidate all the learning of the ages tucked away in Qud's innumerable chrome nooks. Here, =pronouns.subjective= =verb:prepare:afterpronoun= a residence where pilgrims can study the wisdom of others and bring themselves nearer to the divinity of the Kasaphescence.";
			if (ParentObject.GetGender().Name != "female")
			{
				ParentObject.SetPronounSet("she/her");
			}
			ParentObject.SetIntProperty("Librarian", 1);
			if (ParentObject.GetPart("ForceEmitter") is ForceEmitter forceEmitter)
			{
				forceEmitter.StartActive = false;
			}
			ParentObject.RemovePart("Chat");
			ParentObject.RemovePart("ConversationScript");
			ParentObject.RemovePart("Miner");
			ParentObject.RemovePart("TurretTinker");
			ParentObject.AddPart(new ConversationScript("MechanimistLibrarian", ClearLost: true));
			ParentObject.RequirePart<AISuppressIndependentBehavior>();
			ParentObject.RequirePart<Interesting>();
			ParentObject.RemovePart(this);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Librarian", x);
		}
		return base.HandleEvent(E);
	}
}
