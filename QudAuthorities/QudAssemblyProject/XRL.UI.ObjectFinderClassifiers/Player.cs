using XRL.Core;
using XRL.World;

namespace XRL.UI.ObjectFinderClassifiers;

public class Player : ObjectFinder.Classifier
{
	public override bool Check(GameObject go)
	{
		return go == XRLCore.Core?.Game?.Player?.Body;
	}
}
