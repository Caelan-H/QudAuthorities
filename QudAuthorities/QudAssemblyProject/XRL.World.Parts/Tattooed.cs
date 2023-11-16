using System;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Tattooed : IPart
{
	public int ChanceOneIn = 10000;

	public override bool SameAs(IPart p)
	{
		if ((p as Tattooed).ChanceOneIn != ChanceOneIn)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		MarkovChainData data = MarkovBook.CorpusData[text];
		try
		{
			string desc = Grammar.InitLower(Grammar.RemoveBadTitleEndingWords(MarkovChain.GenerateFragment(data, MarkovChain.GenerateSeedFromWord(data, Grammar.GetWeightedStartingArticle()), Stat.Random(1, 5))));
			if (Stat.Random(1, ChanceOneIn) == 1)
			{
				Tattoos.ApplyTattoo(ParentObject, desc);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("tattoo generation", x);
		}
		return base.HandleEvent(E);
	}
}
