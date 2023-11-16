using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DesalinationPellet : IPart
{
	public string RemoveLiquid = "salt";

	public string RemoveLiquidAmount = "200";

	public string ConvertLiquid = "slime";

	public string ConvertLiquidTo = "gel";

	public string ConvertLiquidAmount = "8000";

	public string Message = "=subject.The==subject.name= =verb:fizzle= for several seconds.";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			GameObject gameObject = null;
			Inventory inventory = IComponent<GameObject>.ThePlayer.Inventory;
			List<GameObject> list = Event.NewGameObjectList();
			inventory.GetObjects(list);
			gameObject = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
			if (gameObject == null)
			{
				return false;
			}
			if (gameObject.LiquidVolume == null)
			{
				Popup.ShowFail("It doesn't seem to do anything.");
				return false;
			}
			Popup.Show("You drop " + ParentObject.a + ParentObject.ShortDisplayName + " into " + gameObject.the + gameObject.DisplayNameOnlyDirect + ".\n\n" + PurifyLiquid(gameObject, ShowMessage: false, Single: true, E.Actor.IsPlayer()));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GameObject firstObjectWithPart = E.Cell.GetFirstObjectWithPart("LiquidVolume");
		if (firstObjectWithPart != null && firstObjectWithPart.IsOpenLiquidVolume())
		{
			PurifyLiquid(firstObjectWithPart, ShowMessage: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (E.Object.IsOpenLiquidVolume())
		{
			PurifyLiquid(E.Object, ShowMessage: true);
		}
		return base.HandleEvent(E);
	}

	public string PurifyLiquid(GameObject GO, bool ShowMessage = false, bool Single = false, bool MakeUnderstood = false)
	{
		LiquidVolume liquidVolume = GO.LiquidVolume;
		if (liquidVolume == null)
		{
			return "";
		}
		GameObject gameObject = (Single ? ParentObject.RemoveOne() : ParentObject);
		string text = Message;
		if (!string.IsNullOrEmpty(text))
		{
			text = ((!liquidVolume.IsOpenVolume()) ? GameText.VariableReplace(text, null, liquidVolume.GetLiquidName()) : GameText.VariableReplace(text, GO));
		}
		bool flag = false;
		if (!string.IsNullOrEmpty(RemoveLiquid) && LiquidVolume.getLiquid(RemoveLiquid) != null && liquidVolume.ComponentLiquids.ContainsKey(RemoveLiquid))
		{
			flag = true;
			int num = 0;
			for (int num2 = gameObject.Count; num2 > 0; num2--)
			{
				num += RemoveLiquidAmount.RollCached();
			}
			if (num > 0)
			{
				if (liquidVolume.ComponentLiquids.Count == 1)
				{
					if (liquidVolume.Volume > num)
					{
						liquidVolume.Volume -= num;
					}
					else
					{
						liquidVolume.Empty();
					}
				}
				else
				{
					int num3 = Math.Min(Math.Max((int)Math.Ceiling((float)(liquidVolume.Volume * liquidVolume.ComponentLiquids[RemoveLiquid]) / 1000f), 1), num);
					LiquidVolume liquidVolume2 = new LiquidVolume();
					liquidVolume2.ComponentLiquids.Clear();
					liquidVolume2.ComponentLiquids.Add(RemoveLiquid, 1000);
					liquidVolume2.MaxVolume = num3;
					liquidVolume2.Volume = -num3;
					liquidVolume.MixWith(liquidVolume2);
				}
			}
		}
		if (!string.IsNullOrEmpty(ConvertLiquid) && !string.IsNullOrEmpty(ConvertLiquidTo) && liquidVolume.ComponentLiquids.ContainsKey(ConvertLiquid))
		{
			flag = true;
			int num4 = 0;
			for (int num5 = gameObject.Count; num5 > 0; num5--)
			{
				num4 += ConvertLiquidAmount.RollCached();
			}
			if (num4 > 0)
			{
				int num6 = 0;
				int maxVolume = liquidVolume.MaxVolume;
				try
				{
					liquidVolume.MaxVolume = liquidVolume.Volume;
					if (liquidVolume.ComponentLiquids.Count == 1)
					{
						if (liquidVolume.Volume > num4)
						{
							num6 = num4;
							liquidVolume.Volume -= num4;
						}
						else
						{
							num6 = liquidVolume.Volume;
							liquidVolume.Empty();
						}
					}
					else
					{
						num6 = Math.Min(Math.Max((int)Math.Ceiling((float)(liquidVolume.Volume * liquidVolume.ComponentLiquids[ConvertLiquid]) / 1000f), 1), num4);
						LiquidVolume liquidVolume3 = new LiquidVolume();
						liquidVolume3.ComponentLiquids.Clear();
						liquidVolume3.ComponentLiquids.Add(ConvertLiquid, 1000);
						liquidVolume3.MaxVolume = num6;
						liquidVolume3.Volume = -num6;
						liquidVolume.MixWith(liquidVolume3);
					}
					if (num6 > 0)
					{
						LiquidVolume liquidVolume4 = new LiquidVolume();
						liquidVolume4.ComponentLiquids.Clear();
						liquidVolume4.ComponentLiquids.Add(ConvertLiquidTo, 1000);
						liquidVolume4.MaxVolume = num6;
						liquidVolume4.Volume = num6;
						liquidVolume.MixWith(liquidVolume4);
					}
				}
				finally
				{
					liquidVolume.MaxVolume = maxVolume;
				}
			}
		}
		if (flag)
		{
			liquidVolume.Update();
			if (MakeUnderstood)
			{
				gameObject.MakeUnderstood();
			}
		}
		if (ShowMessage && !string.IsNullOrEmpty(text))
		{
			IComponent<GameObject>.EmitMessage(GO, text);
		}
		gameObject.Obliterate();
		return text;
	}
}
