using System;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Graffitied : IPart
{
	public int ChanceOneIn = 10000;

	public string graffitiText;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID && ID != GetUnknownShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{graffitied|graffitied}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AddGraffiti(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		AddGraffiti(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Stat.Random(1, ChanceOneIn) == 1 && E.Context != "Sample")
		{
			Graffiti(ParentObject);
		}
		else
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public void Graffiti(GameObject wall)
	{
		wall.pRender.SetForegroundColor(Crayons.GetRandomColorAll());
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		MarkovChainData data = MarkovBook.CorpusData[text];
		graffitiText = MarkovChain.GenerateShortSentence(data);
		graffitiText = Grammar.Obfuscate(graffitiText.TrimEnd(' '), 10);
		wall.SetIntProperty("HasGraffiti", 1);
	}

	public void AddGraffiti(IShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(graffitiText))
		{
			E.Base.Append("\n\n").Append("Graffiti is scrawled across the surface. It reads: \n\n\"").Append("{{")
				.Append(ParentObject.pRender.GetForegroundColor())
				.Append("|")
				.Append(graffitiText)
				.Append("}}")
				.Append("\"\n");
		}
	}
}
