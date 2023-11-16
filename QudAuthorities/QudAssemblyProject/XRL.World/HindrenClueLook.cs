using System;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class HindrenClueLook
{
	public string target;

	public string text;

	public HindrenClueLook()
	{
	}

	public HindrenClueLook(string target, string text)
	{
		this.target = target;
		this.text = text;
	}

	public void apply(GameObject go)
	{
		HindrenClueItem p = new HindrenClueItem();
		go.AddPart(p);
		Description part = go.GetPart<Description>();
		part._Short = part._Short + " " + text;
		if (target == "Kesehind")
		{
			go.Body.GetPart("Body")[0].Equipped?.MakeBloodstained("blood", 7);
		}
	}
}
