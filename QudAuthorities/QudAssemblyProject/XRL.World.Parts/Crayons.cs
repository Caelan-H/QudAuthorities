using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Crayons : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Draw", "draw", "DrawWithCrayons", null, 'w');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DrawWithCrayons")
		{
			Cell cell = E.Actor.CurrentCell;
			if (cell != null)
			{
				string text = Popup.AskString("What do you want to draw?", "", 64);
				if (!string.IsNullOrEmpty(text))
				{
					string text2 = Popup.ShowColorPicker("What color do you want to draw with?", 0, null, 60, RespectOptionNewlines: false, AllowEscape: false, 0, "", includeNone: false);
					if (!string.IsNullOrEmpty(text2))
					{
						string text3 = XRL.UI.PickDirection.ShowPicker();
						if (text3 != null)
						{
							Cell cell2 = cell.GetCellFromDirection(text3);
							if (cell2 != null)
							{
								if (ParentObject.GetLongProperty("Nanocrayons") == 1)
								{
									WishResult wishResult = WishSearcher.SearchForCrayonBlueprint(text);
									if (string.IsNullOrEmpty(wishResult.Result))
									{
										Popup.Show("You're not talented enough to draw that.");
										MetricsManager.LogEvent("NanocrayonFail:" + text);
									}
									else
									{
										Popup.Show("You draw a pretty picture.");
										Popup.Show("The picture stretches into the 3rd dimension and becomes real.");
										if (cell2.IsSolid())
										{
											cell2 = cell2.getClosestPassableCell(cell) ?? cell2;
										}
										GameObject gameObject = cell2.AddObject(wishResult.Result);
										Temporary.CarryOver(ParentObject, gameObject);
										Phase.carryOver(E.Actor.PhaseMatches(gameObject) ? ParentObject : E.Actor, gameObject);
										gameObject.pRender?.SetForegroundColor(text2);
										ParentObject.Destroy();
										MetricsManager.LogEvent("Nanocrayon:" + text + ":" + text2);
										E.Actor.UseEnergy(1000, "Item Crayons");
										E.RequestInterfaceExit();
									}
								}
								else
								{
									MetricsManager.LogEvent("Crayon:" + text + ":" + text2);
									GameObject highestRenderLayerObject = cell2.GetHighestRenderLayerObject();
									if (highestRenderLayerObject != null && highestRenderLayerObject.pRender != null)
									{
										highestRenderLayerObject.pRender.DetailColor = text2;
									}
									Popup.Show("You draw a pretty picture.");
									E.Actor.UseEnergy(1000, "Item Crayons");
									E.RequestInterfaceExit();
								}
							}
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (2.in1000())
		{
			E.Object.SetLongProperty("Nanocrayons", 1L);
		}
		return base.HandleEvent(E);
	}

	public static string GetSubterraneanGrowthColor()
	{
		return Stat.Random(1, 4) switch
		{
			1 => "r", 
			2 => "c", 
			3 => "w", 
			_ => "Y", 
		};
	}

	public static string GetRandomColor()
	{
		return Stat.Random(1, 7) switch
		{
			1 => "R", 
			2 => "W", 
			3 => "G", 
			4 => "B", 
			5 => "M", 
			6 => "C", 
			_ => "Y", 
		};
	}

	public static string GetRandomColorAll()
	{
		return Stat.Random(1, 14) switch
		{
			1 => "R", 
			2 => "W", 
			3 => "G", 
			4 => "B", 
			5 => "M", 
			6 => "C", 
			7 => "Y", 
			8 => "r", 
			9 => "w", 
			10 => "g", 
			11 => "b", 
			12 => "m", 
			13 => "c", 
			_ => "y", 
		};
	}

	public static List<string> GetRandomDistinctColorsAll(int numColors)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < numColors; i++)
		{
			int num = 0;
			string randomColorAll;
			do
			{
				randomColorAll = GetRandomColorAll();
				num++;
			}
			while (list.Contains(randomColorAll) && num < 20);
			list.Add(randomColorAll);
		}
		return list;
	}
}
