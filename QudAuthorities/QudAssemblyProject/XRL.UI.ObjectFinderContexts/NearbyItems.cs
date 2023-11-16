using System.Collections.Generic;
using System.Linq;
using Qud.UI;
using XRL.Core;
using XRL.World;

namespace XRL.UI.ObjectFinderContexts;

public class NearbyItems : ObjectFinder.Context
{
	public NearbyItems()
	{
		GameManager.Instance.gameQueue.queueSingletonTask("NearbyItemsInit", delegate
		{
			UpdateItems(The.Core);
		});
	}

	public override string GetDisplayName()
	{
		return "Nearby Items";
	}

	public override void Enable()
	{
		XRLCore.RegisterOnBeginPlayerTurnCallback(UpdateItems);
		XRLCore.RegisterOnEndPlayerTurnCallback(UpdateItems, Single: true);
	}

	public override void Disable()
	{
		XRLCore.RemoveOnBeginPlayerTurnCallback(UpdateItems);
		XRLCore.RemoveOnEndPlayerTurnCallback(UpdateItems, Single: true);
	}

	public void UpdateItems(XRLCore core)
	{
		Cell cell = core.Game?.Player?.Body?.CurrentCell;
		finder?.UpdateContext(this, cell?.GetLocalAdjacentCells()?.Prepend(cell).SelectMany(GetObjectsFromCell));
		SingletonWindowBase<NearbyItemsWindow>.instance.UpdateGameContext();
	}

	private IEnumerable<GameObject> GetObjectsFromCell(Cell c)
	{
		return c.Objects.Where((GameObject o) => o.IsVisible());
	}
}
