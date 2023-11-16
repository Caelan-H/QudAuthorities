using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class MamonPart : IPart
{
	[NonSerialized]
	private bool Rendered;

	public override bool Render(RenderEvent E)
	{
		if (!Rendered)
		{
			Rendered = true;
			XRLCore.Core.Game.FinishQuestStep("Raising Indrix", "Find Mamon Souldrinker");
		}
		return true;
	}
}
