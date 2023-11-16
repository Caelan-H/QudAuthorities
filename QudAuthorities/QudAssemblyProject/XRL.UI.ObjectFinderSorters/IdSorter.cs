using XRL.World;

namespace XRL.UI.ObjectFinderSorters;

public class IdSorter : ObjectFinder.Sorter
{
	public override string GetDisplayName()
	{
		return "Id";
	}

	public override int Compare(GameObject a, GameObject b)
	{
		return string.Compare(a.id, b.id);
	}
}
