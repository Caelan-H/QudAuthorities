using XRL.World;

namespace XRL.UI.ObjectFinderClassifiers;

public class Walls : ObjectFinder.Classifier
{
	public override bool Check(GameObject go)
	{
		return go.HasTag("Wall");
	}
}
