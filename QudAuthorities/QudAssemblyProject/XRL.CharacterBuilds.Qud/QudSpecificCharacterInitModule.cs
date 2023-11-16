using XRL.World;

namespace XRL.CharacterBuilds.Qud;

public class QudSpecificCharacterInitModule : AbstractEmbarkBuilderModule
{
	public override void InitFromSeed(string seed)
	{
	}

	public override void Init()
	{
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_BEFOREBOOTPLAYEROBJECT)
		{
			string text = "Humanoid";
			GenotypeEntry genotypeEntry = builder.GetModule<QudGenotypeModule>()?.data?.Entry;
			if (genotypeEntry != null)
			{
				text = (genotypeEntry.BodyObject.IsNullOrEmpty() ? text : genotypeEntry.BodyObject);
			}
			SubtypeEntry subtypeEntry = builder.GetModule<QudSubtypeModule>()?.data?.Entry;
			if (subtypeEntry != null)
			{
				text = (subtypeEntry.BodyObject.IsNullOrEmpty() ? text : subtypeEntry.BodyObject);
			}
			text = info.fireBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT, game, text);
			GameObject gameObject = GameObject.create(text);
			gameObject.id = "OriginalPlayer";
			gameObject.pBrain.Factions = "";
			gameObject.pBrain.FactionMembership.Clear();
			string value = subtypeEntry?.Species ?? genotypeEntry?.Species;
			if (!string.IsNullOrEmpty(value))
			{
				gameObject.SetStringProperty("Species", value);
			}
			gameObject.SetStringProperty("OriginalPlayerBody", "1");
			return gameObject;
		}
		return base.handleBootEvent(id, game, info, element);
	}
}
