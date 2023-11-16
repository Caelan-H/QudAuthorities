using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Rewired;
using XRL.Language;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.UI;

[UIView("Trade", false, true, false, "Trade,Menu", null, false, 0, false)]
public class TradeUI : IWantsTextConsoleInit
{
	public enum TradeScreenMode
	{
		Trade,
		Container
	}

	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	public static GameObject _Trader = null;

	public static double Performance = 1.0;

	public static bool AssumeTradersHaveWater = true;

	public static int[] ScrollPosition = new int[2];

	public static double[] Totals = new double[2];

	public static int[] Weight = new int[2];

	public static List<TradeEntry>[] Objects = null;

	public static int[][] NumberSelected = new int[2][];

	public static int nTotalWeight = 0;

	public static int nMaxWeight = 0;

	private static int SideSelected = 0;

	private static int RowSelect = 0;

	public static string sReadout = "";

	public static float costMultiple = 1f;

	public static TradeScreenMode ScreenMode;

	public static string TradeScreenVerb
	{
		get
		{
			if (ScreenMode != 0 || !(costMultiple > 0f))
			{
				return "transfer";
			}
			return "trade";
		}
	}

	void IWantsTextConsoleInit.Init(TextConsole console, ScreenBuffer buffer)
	{
		_TextConsole = console;
		_ScreenBuffer = buffer;
	}

	public static double GetMultiplier(GameObject GO)
	{
		if (GO == null || !GO.IsCurrency)
		{
			return Performance;
		}
		return 1.0;
	}

	public static bool ValidForTrade(GameObject obj, GameObject Trader, GameObject Other = null, float costMultiple = 1f, bool AcceptRelevant = true)
	{
		if (Other != null && obj.MovingIntoWouldCreateContainmentLoop(Other))
		{
			return false;
		}
		if (AcceptRelevant && !CanAcceptObjectEvent.Check(obj, Trader, Other))
		{
			return false;
		}
		if (ScreenMode == TradeScreenMode.Container)
		{
			return true;
		}
		if (obj.IsNatural())
		{
			return false;
		}
		if (costMultiple > 0f && obj.HasPropertyOrTag("WaterContainer"))
		{
			LiquidVolume liquidVolume = obj.LiquidVolume;
			if (liquidVolume != null && liquidVolume.IsFreshWater() && !obj.HasPart("TinkerItem"))
			{
				return false;
			}
		}
		if (Trader.IsPlayer())
		{
			if (obj.HasPropertyOrTag("PlayerWontSell"))
			{
				return false;
			}
		}
		else
		{
			if (obj.HasPropertyOrTag("WontSell"))
			{
				return false;
			}
			if (Trader.HasPropertyOrTag("WontSell") && Trader.GetPropertyOrTag("WontSell").Contains(obj.Blueprint))
			{
				return false;
			}
			if (Trader.HasPropertyOrTag("WontSellTag") && obj.HasTagOrProperty(Trader.GetPropertyOrTag("WontSellTag")))
			{
				return false;
			}
		}
		if (obj.HasPropertyOrTag("QuestItem"))
		{
			return true;
		}
		if (!CanBeTradedEvent.Check(obj, Trader, Other, costMultiple))
		{
			return false;
		}
		return true;
	}

	public static void GetObjects(GameObject Trader, List<TradeEntry> ReturnObjects, GameObject Other, float costMultiple = 1f)
	{
		List<GameObject> list = new List<GameObject>(64);
		bool acceptRelevant = CanAcceptObjectEvent.Relevant(Other);
		foreach (GameObject @object in Trader.Inventory.GetObjects())
		{
			if (ValidForTrade(@object, Trader, Other, costMultiple, acceptRelevant))
			{
				list.Add(@object);
			}
		}
		list.Sort(new SortGOCategory());
		string text = "";
		foreach (GameObject item in list)
		{
			item.Seen();
			string inventoryCategory = item.GetInventoryCategory();
			if (inventoryCategory != text)
			{
				text = inventoryCategory;
				ReturnObjects.Add(new TradeEntry(text));
			}
			ReturnObjects.Add(new TradeEntry(item));
		}
	}

	public static string FormatPrice(double Price, float multiplier)
	{
		return $"{Price * (double)multiplier:0.00}";
	}

	public static void Reset()
	{
		ScrollPosition = new int[2];
		Totals = new double[2];
		Weight = new int[2];
		if (Objects == null)
		{
			Objects = new List<TradeEntry>[2];
			Objects[0] = new List<TradeEntry>();
			Objects[1] = new List<TradeEntry>();
		}
		Objects[0].Clear();
		Objects[1].Clear();
		NumberSelected = new int[2][];
	}

	public static int GetSideOfObject(GameObject obj)
	{
		if (FindInTradeList(Objects[0], obj) > -1)
		{
			return 0;
		}
		return 1;
	}

	public static int FindInTradeList(List<TradeEntry> list, GameObject obj)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GO == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public static double ItemValueEach(GameObject obj, bool? TraderInventory = null)
	{
		double num = obj.ValueEach;
		if (_Trader != null && (TraderInventory == true || (!TraderInventory.HasValue && FindInTradeList(Objects[0], obj) != -1)))
		{
			int intProperty = _Trader.GetIntProperty("MinimumSellValue");
			if (intProperty > 0 && num < (double)intProperty)
			{
				num = intProperty;
			}
		}
		return num;
	}

	public static double GetValue(GameObject obj, bool? TraderInventory = null)
	{
		if (TraderInventory == true || (!TraderInventory.HasValue && FindInTradeList(Objects[0], obj) != -1))
		{
			return ItemValueEach(obj, true) / GetMultiplier(obj);
		}
		if (TraderInventory == false || (!TraderInventory.HasValue && FindInTradeList(Objects[1], obj) != -1))
		{
			return ItemValueEach(obj, false) * GetMultiplier(obj);
		}
		return 0.0;
	}

	public static int GetNumberSelected(GameObject obj)
	{
		int num = FindInTradeList(Objects[0], obj);
		if (num != -1)
		{
			return NumberSelected[0][num];
		}
		num = FindInTradeList(Objects[1], obj);
		if (num != -1)
		{
			return NumberSelected[1][num];
		}
		return -999;
	}

	public static void SetSelectedObject(GameObject obj)
	{
		int num = FindInTradeList(Objects[0], obj);
		if (num != -1)
		{
			SideSelected = 0;
			RowSelect = num;
		}
		num = FindInTradeList(Objects[1], obj);
		if (num != -1)
		{
			SideSelected = 1;
			RowSelect = num;
		}
	}

	public static void SetNumberSelected(GameObject obj, int amount)
	{
		int num = FindInTradeList(Objects[0], obj);
		if (num != -1)
		{
			NumberSelected[0][num] = amount;
		}
		num = FindInTradeList(Objects[1], obj);
		if (num != -1)
		{
			NumberSelected[1][num] = amount;
		}
		UpdateTotals();
	}

	public static void PerformObjectDropped(GameObject Object, int DroppedOnSide)
	{
		if (FindInTradeList(Objects[DroppedOnSide], Object) != -1)
		{
			int num = FindInTradeList(Objects[DroppedOnSide], Object);
			NumberSelected[DroppedOnSide][num] = 0;
			UpdateTotals();
			return;
		}
		int num2 = FindInTradeList(Objects[1 - DroppedOnSide], Object);
		if (num2 != -1)
		{
			NumberSelected[1 - DroppedOnSide][num2] = Objects[1 - DroppedOnSide][num2].GO.Count;
			UpdateTotals();
		}
	}

	public static void UpdateTotals()
	{
		for (int i = 0; i <= 1; i++)
		{
			double num = 1.0;
			switch (i)
			{
			case 0:
				num = 1.0 / Performance;
				break;
			case 1:
				num = Performance;
				break;
			}
			Totals[i] = 0.0;
			Weight[i] = 0;
			for (int j = 0; j < Objects[i].Count; j++)
			{
				if (Objects[i][j].GO != null && NumberSelected[i][j] > 0)
				{
					Weight[i] += Objects[i][j].GO.WeightEach * NumberSelected[i][j];
					if (Objects[i][j].GO.IsCurrency)
					{
						Totals[i] += ItemValueEach(Objects[i][j].GO) * (double)NumberSelected[i][j];
					}
					else
					{
						Totals[i] += ItemValueEach(Objects[i][j].GO) * num * (double)NumberSelected[i][j];
					}
				}
			}
			Totals[i] *= costMultiple;
		}
		sReadout = " {{C|" + $"{Totals[0]:0.###}" + "}} drams <-> {{C|" + $"{Totals[1]:0.###}" + "}} drams ÄÄ {{W|$" + The.Player.GetFreeDrams() + "}} ";
	}

	public static void ShowTradeScreen(GameObject Trader, float _costMultiple = 1f, TradeScreenMode screenMode = TradeScreenMode.Trade)
	{
		bool flag = Trader.IsPlayerLed();
		if (flag)
		{
			_costMultiple = 0f;
		}
		costMultiple = _costMultiple;
		ScreenMode = screenMode;
		TextConsole.LoadScrapBuffers();
		GameManager.Instance.PushGameView("Trade");
		Reset();
		while (true)
		{
			if (Trader == null)
			{
				GameManager.Instance.PopGameView();
				Reset();
				break;
			}
			if (!Trader.HasPart("Inventory"))
			{
				Popup.ShowFail(Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " cannot carry things.");
				GameManager.Instance.PopGameView();
				Reset();
				break;
			}
			_Trader = Trader;
			int intProperty = _Trader.GetIntProperty("TraderCreditExtended");
			if (intProperty > 0)
			{
				int freeDrams = The.Player.GetFreeDrams();
				if (freeDrams <= 0)
				{
					Popup.Show(_Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will not trade with you until you pay " + _Trader.them + " the {{C|" + intProperty + "}} " + ((intProperty == 1) ? "dram" : "drams") + " of {{B|fresh water}} you owe " + _Trader.them + ".");
					GameManager.Instance.PopGameView();
					Reset();
					break;
				}
				if (freeDrams < intProperty)
				{
					if (Popup.ShowYesNo(_Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will not trade with you until you pay " + _Trader.them + " the {{C|" + intProperty + "}} " + ((intProperty == 1) ? "dram" : "drams") + " of {{B|fresh water}} you owe " + _Trader.them + ". Do you want to give " + _Trader.them + " your {{C|" + freeDrams + "}} " + ((freeDrams == 1) ? "dram" : "drams") + " now?") == DialogResult.Yes)
					{
						intProperty -= freeDrams;
						The.Player.UseDrams(freeDrams);
						_Trader.SetIntProperty("TraderCreditExtended", intProperty);
					}
					GameManager.Instance.PopGameView();
					Reset();
					break;
				}
				if (Popup.ShowYesNo(_Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will not trade with you until you pay " + _Trader.them + " the {{C|" + intProperty + "}} " + ((intProperty == 1) ? "dram" : "drams") + " of {{B|fresh water}} you owe " + _Trader.them + ". Do you want to give it to " + _Trader.them + " now?") != 0)
				{
					GameManager.Instance.PopGameView();
					Reset();
					break;
				}
				The.Player.UseDrams(intProperty);
				_Trader.RemoveIntProperty("TraderCreditExtended");
			}
			Performance = GetTradePerformanceEvent.GetFor(The.Player, _Trader);
			Tinkering_Repair tinkering_Repair = Trader.GetPart("Tinkering_Repair") as Tinkering_Repair;
			int identifyLevel = Tinkering.GetIdentifyLevel(Trader);
			bool flag2 = identifyLevel > 0;
			bool flag3 = tinkering_Repair != null;
			bool flag4 = Trader.HasSkill("Tinkering_Tinker1");
			bool flag5 = Trader.GetIntProperty("Librarian") != 0;
			SideSelected = 0;
			RowSelect = 0;
			int num = 21;
			int num2 = 1;
			int num23;
			int num26;
			List<GameObject> list4;
			List<GameObject> list5;
			while (true)
			{
				IL_03df:
				Objects[0].Clear();
				Objects[1].Clear();
				ScrollPosition[0] = 0;
				ScrollPosition[1] = 0;
				Totals[0] = 0.0;
				Totals[1] = 0.0;
				GetObjects(Trader, Objects[0], The.Player, costMultiple);
				GetObjects(The.Player, Objects[1], Trader, costMultiple);
				NumberSelected[0] = new int[Objects[0].Count];
				NumberSelected[1] = new int[Objects[1].Count];
				if (Objects[0].Count <= 0 && costMultiple > 0f)
				{
					if (!AllowTradeWithNoInventoryEvent.Check(The.Player, Trader))
					{
						Popup.Show(Trader.Does("have", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " nothing to trade.");
						_Trader = null;
						GameManager.Instance.PopGameView();
						Reset();
						return;
					}
				}
				nTotalWeight = The.Player.GetCarriedWeight();
				nMaxWeight = The.Player.GetMaxCarriedWeight();
				UpdateTotals();
				if (Options.OverlayPrereleaseTrade)
				{
					TradeView.instance.QueueInventoryUpdate();
				}
				bool flag6 = false;
				int num3 = 0;
				string s = "[{{W|" + ControlManager.getCommandInputDescription("Add one item", mapGlyphs: false) + "}}/{{W|" + ControlManager.getCommandInputDescription("Remove one item", mapGlyphs: false) + "}} Add/Remove]";
				int length = ColorUtility.StripFormatting(s).Length;
				string s2 = "[{{W|" + ControlManager.getCommandInputDescription("Offer", mapGlyphs: false) + "}} Offer]";
				int length2 = ColorUtility.StripFormatting(s2).Length;
				bool flag7 = false;
				bool flag11;
				int num30;
				while (true)
				{
					Keys keys;
					if (!flag7)
					{
						if (Objects[SideSelected].Count == 0)
						{
							SideSelected = 1 - SideSelected;
						}
						Event.ResetPool(resetMinEventPools: false);
						_ScreenBuffer.Clear();
						string text = "";
						string text2 = "";
						IRenderable renderable = null;
						int num4 = 0;
						int num5 = ScrollPosition[0];
						while (num4 < num && num5 < Objects[0].Count)
						{
							_ScreenBuffer.Goto(2, num4 + num2);
							GameObject gO = Objects[0][num5].GO;
							if (gO != null)
							{
								if (NumberSelected[0][num5] > 0)
								{
									_ScreenBuffer.Write("{{&Y^g|" + NumberSelected[0][num5] + "}} ");
								}
								_ScreenBuffer.Write(gO.RenderForUI());
								_ScreenBuffer.Write(" ");
								_ScreenBuffer.Write(gO.DisplayName);
								if (Trader.IsOwned() && gO.OwnedByPlayer)
								{
									_ScreenBuffer.Write(" {{G|[owned by you]}}");
								}
								string text3 = "";
								if (SideSelected == 0 && RowSelect == num4)
								{
									text = gO.DisplayNameSingle;
									if (Trader.IsOwned() && gO.OwnedByPlayer)
									{
										text += " {{G|[owned by you]}}";
									}
									renderable = gO.RenderForUI();
									text2 = " {{K|" + gO.WeightEach + "#}}";
									if (screenMode == TradeScreenMode.Trade)
									{
										string text4 = (gO.IsCurrency ? "Y" : "B");
										text3 = "{{" + text4 + "|$}}{{C|" + FormatPrice(GetValue(gO, true), costMultiple) + "}}";
										text2 = text2 + " " + text3;
									}
								}
								else if (screenMode == TradeScreenMode.Trade)
								{
									string text5 = (gO.IsCurrency ? "W" : "b");
									text3 = "{{" + text5 + "|$}}{{c|" + FormatPrice(GetValue(gO, true), costMultiple) + "}}";
								}
								int x2 = 40 - ColorUtility.LengthExceptFormatting(text3);
								_ScreenBuffer.Goto(x2, num4 + num2);
								_ScreenBuffer.Write(text3);
							}
							else
							{
								string s3 = "{{K|[{{y|" + Objects[0][num5].CategoryName + "}}]}}";
								_ScreenBuffer.Goto(40 - ColorUtility.LengthExceptFormatting(s3), num4 + num2);
								_ScreenBuffer.Write(s3);
							}
							num4++;
							num5++;
						}
						_ScreenBuffer.Fill(41, num2, 77, num2 + num, 32, 0);
						int num6 = 0;
						int num7 = ScrollPosition[1];
						while (num6 < num && num7 < Objects[1].Count)
						{
							_ScreenBuffer.Goto(42, num6 + num2);
							GameObject gO2 = Objects[1][num7].GO;
							if (gO2 != null)
							{
								if (NumberSelected[1][num7] > 0)
								{
									_ScreenBuffer.Write("{{&Y^g|" + NumberSelected[1][num7] + "}} ");
								}
								_ScreenBuffer.Write(gO2.RenderForUI());
								_ScreenBuffer.Write(" ");
								_ScreenBuffer.Write(gO2.DisplayName);
								string text3 = "";
								if (SideSelected == 1 && RowSelect == num6)
								{
									text = gO2.DisplayNameSingle;
									renderable = gO2.RenderForUI();
									text2 = " {{K|" + gO2.WeightEach + "#}}";
									if (screenMode == TradeScreenMode.Trade)
									{
										string text6 = (gO2.IsCurrency ? "Y" : "B");
										text3 = "{{" + text6 + "|$}}{{C|" + FormatPrice(GetValue(gO2, false), costMultiple) + "}}";
										text2 = text2 + " " + text3;
									}
								}
								else if (screenMode == TradeScreenMode.Trade)
								{
									string text7 = (gO2.IsCurrency ? "W" : "b");
									text3 = "{{" + text7 + "|$}}{{c|" + FormatPrice(GetValue(gO2, false), costMultiple) + "}}";
								}
								int x3 = 79 - ColorUtility.LengthExceptFormatting(text3);
								_ScreenBuffer.Goto(x3, num6 + num2);
								_ScreenBuffer.Write(text3);
							}
							else
							{
								string s4 = "{{K|[{{y|" + Objects[1][num7].CategoryName + "}}]}}";
								_ScreenBuffer.Goto(79 - ColorUtility.LengthExceptFormatting(s4), num6 + num2);
								_ScreenBuffer.Write(s4);
							}
							num6++;
							num7++;
						}
						_ScreenBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
						_ScreenBuffer.Goto(2, 0);
						_ScreenBuffer.Write("[ {{W|" + (Trader.IsCreature ? Trader.poss("inventory") : Trader.ShortDisplayName) + "}} ]");
						_ScreenBuffer.Goto(42, 0);
						_ScreenBuffer.Write("[ {{W|Your inventory}} ]");
						_ScreenBuffer.Goto(40, 0);
						_ScreenBuffer.Write(194);
						for (int i = 1; i < 22; i++)
						{
							_ScreenBuffer.Goto(40, i);
							_ScreenBuffer.Write(179);
						}
						for (int j = 1; j < 79; j++)
						{
							_ScreenBuffer.Goto(j, 22);
							_ScreenBuffer.Write(196);
						}
						if (!Trader.IsCreature && Trader.IsOwned())
						{
							_ScreenBuffer.WriteAt(2, 22, "{{R|[ owned by someone else ]}}");
						}
						if (SideSelected == 0)
						{
							_ScreenBuffer.Goto(1, RowSelect + num2);
						}
						else
						{
							_ScreenBuffer.Goto(41, RowSelect + num2);
						}
						_ScreenBuffer.Write("{{&k^Y|>}}");
						_ScreenBuffer.Goto(40, 22);
						_ScreenBuffer.Write(193);
						_ScreenBuffer.Goto(0, 22);
						_ScreenBuffer.Write(195);
						_ScreenBuffer.Goto(79, 22);
						_ScreenBuffer.Write(180);
						if (Objects[SideSelected].Count > 0 && Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]] != null)
						{
							_ScreenBuffer.Goto(2, 23);
							if (renderable != null)
							{
								_ScreenBuffer.Write(renderable);
								_ScreenBuffer.Goto(4, 23);
							}
							_ScreenBuffer.Write(text);
							if (!string.IsNullOrEmpty(text2))
							{
								_ScreenBuffer.Goto(79 - ColorUtility.LengthExceptFormatting(text2), 23);
								_ScreenBuffer.Write(text2);
							}
						}
						int num8 = 2;
						_ScreenBuffer.Goto(num8, 24);
						if (ControlManager.activeControllerType != ControllerType.Joystick)
						{
							_ScreenBuffer.Write("[{{W|ESC}} Exit]");
							num8 += 11;
						}
						_ScreenBuffer.Goto(num8, 24);
						if (ControlManager.activeControllerType == ControllerType.Joystick)
						{
							_ScreenBuffer.Write(s);
							num8 += length + 1;
						}
						else
						{
							_ScreenBuffer.Write("[{{W|+}}/{{W|-}} Add/Remove]");
							num8 += 17;
						}
						if (ControlManager.activeControllerType != ControllerType.Joystick)
						{
							_ScreenBuffer.Goto(num8, 24);
							_ScreenBuffer.Write("[{{W|0-9}} Pick]");
							num8 += 11;
						}
						if (screenMode == TradeScreenMode.Trade)
						{
							_ScreenBuffer.Goto(num8, 24);
							if (ControlManager.activeControllerType == ControllerType.Joystick)
							{
								_ScreenBuffer.Write(s2);
								num8 += length2 + 1;
							}
							else
							{
								_ScreenBuffer.Write("[{{W|o}} Offer]");
								num8 += 10;
							}
							if (Options.SifrahHaggling)
							{
								_ScreenBuffer.Goto(num8, 24);
								_ScreenBuffer.Write("[{{W|/}} Haggle]");
								num8 += 11;
							}
						}
						else
						{
							_ScreenBuffer.Goto(num8, 24);
							if (ControlManager.activeControllerType == ControllerType.Joystick)
							{
								_ScreenBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("Offer", mapGlyphs: false) + "}} Transfer]");
								num8 += length2 + 4;
							}
							else
							{
								_ScreenBuffer.Write("[{{W|o}} Transfer]");
								num8 += 13;
							}
						}
						_ScreenBuffer.Goto(num8, 24);
						if (ControlManager.activeControllerType == ControllerType.Joystick)
						{
							_ScreenBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("Vendor Actions", mapGlyphs: false) + "}} Actions]");
						}
						else
						{
							_ScreenBuffer.Write("[{{W|Space}} Actions]");
						}
						num8 += 16;
						if (screenMode == TradeScreenMode.Trade)
						{
							string s5 = " {{C|" + $"{Totals[0]:0.###}" + "}} drams ";
							_ScreenBuffer.Goto(39 - ColorUtility.LengthExceptFormatting(s5), 22);
							_ScreenBuffer.Write(s5);
							_ScreenBuffer.Goto(42, 22);
							_ScreenBuffer.Write(" {{C|" + $"{Totals[1]:0.###}" + "}} drams ÄÄ {{W|$" + The.Player.GetFreeDrams() + "}} ");
						}
						for (int k = 0; k <= 1; k++)
						{
							if (Objects[k].Count <= num)
							{
								continue;
							}
							for (int l = 1; l < 22; l++)
							{
								if (k == 0)
								{
									_ScreenBuffer.Goto(0, l);
								}
								else
								{
									_ScreenBuffer.Goto(79, l);
								}
								_ScreenBuffer.Write(177, ColorUtility.Bright((ushort)0), 0);
							}
							_ = (int)Math.Ceiling((double)Objects[k].Count / (double)num);
							int num9 = (int)Math.Ceiling((double)(Objects[k].Count + num) / (double)num);
							_ = 0;
							if (num9 <= 0)
							{
								num9 = 1;
							}
							int num10 = 21 / num9;
							if (num10 <= 0)
							{
								num10 = 1;
							}
							int num11 = (int)((double)(21 - num10) * ((double)ScrollPosition[k] / (double)(Objects[k].Count - num)));
							num11++;
							for (int m = num11; m < num11 + num10; m++)
							{
								if (k == 0)
								{
									_ScreenBuffer.Goto(0, m);
								}
								else
								{
									_ScreenBuffer.Goto(79, m);
								}
								_ScreenBuffer.Write(219, ColorUtility.Bright(7), 0);
							}
						}
						int num12 = (int)(0.25 * Math.Ceiling(Totals[0] - Totals[1]));
						int num13 = nTotalWeight + Weight[0] - Weight[1] - num12;
						string text8 = "K";
						if (num13 > nMaxWeight)
						{
							text8 = "R";
						}
						string s6 = " {{" + text8 + "|" + num13 + "/" + nMaxWeight + " lbs.}} ";
						_ScreenBuffer.Goto(77 - ColorUtility.LengthExceptFormatting(s6), 22);
						_ScreenBuffer.Write(s6);
						_TextConsole.DrawBuffer(_ScreenBuffer, null, Options.OverlayPrereleaseTrade);
						keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
						if (keys == Keys.Escape)
						{
							flag7 = true;
						}
						if (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "RightClick")
						{
							flag7 = true;
						}
						if (keys >= Keys.D0 && keys <= Keys.D9)
						{
							if (Objects[SideSelected].Count > 0)
							{
								GameObject gO3 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								if (gO3 != null)
								{
									int num14 = (int)(keys - 48);
									num3 = ((num3 < gO3.Count) ? (num3 * 10 + num14) : num14);
									if (SideSelected == 1 && NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] == 0 && (ScreenMode != TradeScreenMode.Container || Trader.IsOwned()) && !gO3.ConfirmUseImportant(null, TradeScreenVerb, null, num3))
									{
										continue;
									}
									if (num3 > gO3.Count)
									{
										NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = gO3.Count;
									}
									else
									{
										NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = num3;
									}
								}
								UpdateTotals();
								continue;
							}
						}
						else
						{
							num3 = 0;
						}
						if (keys == Keys.Oemtilde && Objects[SideSelected].Count > 0)
						{
							NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = 0;
							UpdateTotals();
							continue;
						}
						if ((ConsoleLib.Console.Keyboard.vkCode == Keys.Space || (ConsoleLib.Console.Keyboard.vkCode == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "Passthrough:Vendor Actions")) && Objects[SideSelected].Count > 0)
						{
							GameObject gO4 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							List<string> list = new List<string> { "Look" };
							List<char> list2 = new List<char> { 'l' };
							if (flag2 && !gO4.Understood())
							{
								list.Add("Identify");
								list2.Add('i');
							}
							if (flag3 && IsRepairableEvent.Check(Trader, gO4, null, tinkering_Repair))
							{
								list.Add("Repair");
								list2.Add('r');
							}
							if (flag4 && (gO4.Understood() || identifyLevel >= gO4.GetComplexity()) && gO4.NeedsRecharge())
							{
								list.Add("Recharge");
								list2.Add('c');
							}
							if (flag5 && gO4.HasInventoryActionWithCommand("Read"))
							{
								list.Add("Read");
								list2.Add('b');
							}
							int num15 = Popup.ShowOptionList("select an action", list.ToArray(), list2.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
							if (num15 >= 0)
							{
								if (list[num15] == "Identify")
								{
									ConsoleLib.Console.Keyboard.vkCode = Keys.I;
								}
								else if (list[num15] == "Read")
								{
									ConsoleLib.Console.Keyboard.vkCode = Keys.B;
								}
								else if (list[num15] == "Repair")
								{
									ConsoleLib.Console.Keyboard.vkCode = Keys.R;
								}
								else if (list[num15] == "Look")
								{
									ConsoleLib.Console.Keyboard.vkCode = Keys.L;
								}
								else if (list[num15] == "Recharge")
								{
									ConsoleLib.Console.Keyboard.vkCode = Keys.C;
								}
							}
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.R)
						{
							if (flag3)
							{
								if (Objects[SideSelected].Count <= 0)
								{
									continue;
								}
								GameObject gO5 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								if (gO5 == null)
								{
									continue;
								}
								bool flag8 = gO5.IsPlural || gO5.Count > 1;
								if (IsRepairableEvent.Check(Trader, gO5, null, tinkering_Repair))
								{
									if (!Tinkering_Repair.IsRepairableBy(gO5, Trader, null, tinkering_Repair))
									{
										Popup.ShowBlock((flag8 ? "These items are" : "This item is") + " too complex for " + Trader.t() + " to repair.");
										continue;
									}
									int num16 = Math.Max(5 + (int)(GetValue(gO5, false) / 25.0), 5) * gO5.Count;
									if (The.Player.GetFreeDrams() < num16)
									{
										Popup.Show("You need {{C|" + num16 + "}} " + ((num16 == 1) ? "dram" : "drams") + " of fresh water to repair " + (flag8 ? "those" : "that") + ".");
									}
									else if (Popup.ShowYesNo("You may repair " + (flag8 ? "those" : "this") + " for {{C|" + num16 + "}} " + ((num16 == 1) ? "dram" : "drams") + " of fresh water.") == DialogResult.Yes && The.Player.UseDrams(num16))
									{
										Trader.GiveDrams(num16);
										RepairedEvent.Send(Trader, gO5, null, tinkering_Repair);
									}
								}
								else
								{
									Popup.ShowBlock((flag8 ? "Those items aren't" : "That item isn't") + " broken!");
								}
							}
							else
							{
								Popup.Show("This trader doesn't have the skill to repair items.");
							}
							continue;
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.Tab)
						{
							bool flag9 = false;
							int n = 0;
							for (int count = Objects[SideSelected].Count; n < count; n++)
							{
								GameObject gO6 = Objects[SideSelected][n].GO;
								if (gO6 != null)
								{
									int count2 = gO6.Count;
									if (NumberSelected[SideSelected][n] != count2 && (SideSelected != 1 || !gO6.IsImportant()))
									{
										NumberSelected[SideSelected][n] = count2;
										flag9 = true;
									}
								}
							}
							if (!flag9)
							{
								int num17 = 0;
								for (int num18 = NumberSelected[SideSelected].Length; num17 < num18; num17++)
								{
									NumberSelected[SideSelected][num17] = 0;
								}
							}
							UpdateTotals();
							continue;
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.L)
						{
							if (Objects[SideSelected].Count > 0)
							{
								GameObject gO7 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								InventoryActionEvent.Check(gO7, The.Player, gO7, "Look");
							}
							continue;
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.C)
						{
							if (!flag4)
							{
								continue;
							}
							if (Objects[SideSelected].Count > 0)
							{
								GameObject gO8 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								if (gO8 != null && (gO8.Understood() || identifyLevel >= gO8.GetComplexity()) && RechargeAction(gO8, Trader))
								{
									break;
								}
							}
							else
							{
								Popup.Show("This trader doesn't have the skill to recharge items.");
							}
							continue;
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.B && flag5)
						{
							if (Objects[SideSelected].Count > 0)
							{
								GameObject gO9 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								InventoryActionEvent.Check(gO9, The.Player, gO9, "Read");
							}
							continue;
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.L)
						{
							if (Objects[SideSelected].Count > 0)
							{
								GameObject gO10 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								InventoryActionEvent.Check(gO10, The.Player, gO10, "Look");
							}
							continue;
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.I)
						{
							if (Objects[SideSelected].Count <= 0)
							{
								continue;
							}
							if (flag2)
							{
								GameObject gO11 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								if (gO11 == null)
								{
									continue;
								}
								if (!gO11.Understood())
								{
									int complexity = gO11.GetComplexity();
									int examineDifficulty = gO11.GetExamineDifficulty();
									if (The.Player.HasPart("Dystechnia"))
									{
										Popup.ShowFail("You can't understand " + Grammar.MakePossessive(Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)) + " explanation.");
										continue;
									}
									if (identifyLevel < complexity)
									{
										Popup.ShowFail("This item is too complex for " + Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " to identify.");
										continue;
									}
									float num19 = complexity + examineDifficulty;
									int num20 = (int)Math.Max(2.0, -0.0667 + 1.24 * (double)num19 + 0.0967 * Math.Pow(num19, 2.0) + 0.0979 * Math.Pow(num19, 3.0));
									if (The.Player.GetFreeDrams() < num20)
									{
										Popup.ShowFail("You do not have the required {{C|" + num20 + "}} " + ((num20 == 1) ? "dram" : "drams") + " to identify this item.");
									}
									else if (Popup.ShowYesNo("You may identify this for " + num20 + " " + ((num20 == 1) ? "dram" : "drams") + " of fresh water.") == DialogResult.Yes && The.Player.UseDrams(num20))
									{
										Trader.GiveDrams(num20);
										gO11.MakeUnderstood();
									}
								}
								else
								{
									Popup.ShowFail("You already understand this item.");
								}
							}
							else
							{
								Popup.ShowBlock(Trader.Does("don't", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " have the skill to identify artifacts.");
							}
							continue;
						}
						if (ConsoleLib.Console.Keyboard.vkCode == Keys.Enter && Objects[SideSelected].Count > 0)
						{
							GameObject gO12 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							if (gO12 != null)
							{
								if (NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] == gO12.Count)
								{
									NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = 0;
								}
								else
								{
									if (SideSelected == 1 && (ScreenMode != TradeScreenMode.Container || Trader.IsOwned()) && !gO12.ConfirmUseImportant(null, TradeScreenVerb))
									{
										goto IL_1eab;
									}
									NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = gO12.Count;
								}
								UpdateTotals();
							}
						}
						goto IL_1eab;
					}
					ScreenBuffer.ClearImposterSuppression();
					if (flag6)
					{
						The.Player.UseEnergy(1000, "Trading");
						_Trader.UseEnergy(1000, "Trading");
					}
					GameManager.Instance.PopGameView();
					_TextConsole.DrawBuffer(TextConsole.ScrapBuffer2, null, Options.OverlayPrereleaseTrade);
					_Trader = null;
					Reset();
					if (flag)
					{
						Trader.pBrain.PerformReequip();
					}
					return;
					IL_1eab:
					if ((ConsoleLib.Console.Keyboard.vkCode == Keys.Add || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next) || keys == Keys.Oemplus) && Objects[SideSelected].Count > 0)
					{
						GameObject gO13 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
						if (gO13 != null && (SideSelected != 1 || NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] != 0 || (ScreenMode == TradeScreenMode.Container && !Trader.IsOwned()) || gO13.ConfirmUseImportant(null, TradeScreenVerb, null, 1)))
						{
							if (NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] < Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO.Count)
							{
								NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]]++;
							}
							UpdateTotals();
						}
					}
					if ((ConsoleLib.Console.Keyboard.vkCode == Keys.Subtract || keys == Keys.NumPad7 || keys == Keys.OemMinus) && Objects[SideSelected].Count > 0 && Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO != null)
					{
						if (NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] > 0)
						{
							NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]]--;
						}
						UpdateTotals();
					}
					if (keys == Keys.NumPad4 && SideSelected == 1 && Objects[0].Count > 0)
					{
						SideSelected = 0;
					}
					if (keys == Keys.NumPad6 && SideSelected == 0 && Objects[1].Count > 0)
					{
						SideSelected = 1;
					}
					if (keys == Keys.NumPad8)
					{
						if (RowSelect == 0)
						{
							if (ScrollPosition[SideSelected] > 0)
							{
								ScrollPosition[SideSelected]--;
							}
						}
						else
						{
							RowSelect--;
						}
					}
					if (keys == Keys.NumPad2)
					{
						if (RowSelect < num - 1 && RowSelect + ScrollPosition[SideSelected] < Objects[SideSelected].Count - 1)
						{
							RowSelect++;
						}
						else if (ScrollPosition[SideSelected] + num < Objects[SideSelected].Count)
						{
							ScrollPosition[SideSelected]++;
						}
					}
					if (keys == Keys.Prior || ConsoleLib.Console.Keyboard.RawCode == Keys.Prior || ConsoleLib.Console.Keyboard.RawCode == Keys.Back)
					{
						if (RowSelect > 0)
						{
							RowSelect = 0;
						}
						else
						{
							ScrollPosition[SideSelected] -= num - 1;
							if (ScrollPosition[SideSelected] < 0)
							{
								ScrollPosition[SideSelected] = 0;
							}
						}
					}
					if (keys == Keys.Next || keys == Keys.Next || ConsoleLib.Console.Keyboard.RawCode == Keys.Next || ConsoleLib.Console.Keyboard.RawCode == Keys.Next)
					{
						if (RowSelect < num - 1)
						{
							RowSelect = num - 1;
							if (RowSelect + ScrollPosition[SideSelected] >= Objects[SideSelected].Count - 1)
							{
								RowSelect = Objects[SideSelected].Count - 1 - ScrollPosition[SideSelected];
							}
						}
						else if (RowSelect == num - 1)
						{
							ScrollPosition[SideSelected] += num - 1;
							if (RowSelect + ScrollPosition[SideSelected] >= Objects[SideSelected].Count - 1)
							{
								ScrollPosition[SideSelected] = Objects[SideSelected].Count - 1 - RowSelect;
							}
						}
					}
					if (RowSelect + ScrollPosition[SideSelected] >= Objects[SideSelected].Count - 1)
					{
						RowSelect = Objects[SideSelected].Count - 1 - ScrollPosition[SideSelected];
					}
					bool flag10 = false;
					flag11 = false;
					if (keys == Keys.OemQuestion && Options.SifrahHaggling && screenMode == TradeScreenMode.Trade)
					{
						if (Totals[0] == 0.0 && Totals[1] == 0.0)
						{
							Popup.ShowFail("Set up a trade, then haggle for it.");
							continue;
						}
						int num21 = (int)Math.Ceiling(Totals[0] - Totals[1]);
						if (num21 > 0 && num21 > The.Player.GetFreeDrams() * 2)
						{
							Popup.ShowFail("You must have at least half the fresh water you would need to pay for this trade available in order to attempt to haggle for it.");
							continue;
						}
						HagglingSifrah hagglingSifrah = new HagglingSifrah(Trader);
						hagglingSifrah.Play(Trader);
						if (hagglingSifrah.Abort)
						{
							continue;
						}
						if (hagglingSifrah.Performance > 0)
						{
							Performance += (1.0 - Performance) * (double)hagglingSifrah.Performance / 100.0;
						}
						else
						{
							Performance -= Performance * (double)(-hagglingSifrah.Performance) / 100.0;
						}
						UpdateTotals();
						int num22 = (int)Math.Ceiling(Totals[0] - Totals[1]);
						string text9 = null;
						if (num21 == num22)
						{
							text9 = "In the end, though, it makes no difference.";
						}
						else if (num21 >= 0 && num22 >= 0)
						{
							text9 = "As a result, the trade costs you " + num22 + " " + ((num22 == 1) ? "dram" : "drams") + " rather than " + num21 + ".";
						}
						else if (num21 < 0 && num22 < 0)
						{
							text9 = "As a result, the trade is worth " + -num22 + " " + ((-num22 == 1) ? "dram" : "drams") + " rather than " + -num21 + ".";
						}
						else if (num21 >= 0 && num22 < 0)
						{
							text9 = "As a result, the trade goes from costing you " + num21 + " " + ((num21 == 1) ? "dram" : "drams") + " to being worth " + -num22 + ".";
						}
						else if (num21 < 0 && num22 >= 0)
						{
							text9 = "As a result, the trade goes from being worth " + -num21 + " " + ((-num21 == 1) ? "dram" : "drams") + " to being worth " + num22 + ".";
						}
						StringBuilder stringBuilder = Event.NewStringBuilder();
						if (!string.IsNullOrEmpty(hagglingSifrah.Description))
						{
							stringBuilder.Compound(hagglingSifrah.Description);
						}
						if (!string.IsNullOrEmpty(text9))
						{
							stringBuilder.Compound(text9);
						}
						if (stringBuilder.Length > 0)
						{
							Popup.Show(stringBuilder.ToString());
						}
						flag10 = true;
						flag11 = true;
					}
					if (keys == Keys.O || keys == Keys.F1)
					{
						flag10 = true;
					}
					num23 = (int)Math.Ceiling(Totals[0] - Totals[1]);
					if (!flag10)
					{
						continue;
					}
					if (num23 > 0)
					{
						int freeDrams2 = The.Player.GetFreeDrams();
						if (freeDrams2 >= num23)
						{
							if (flag11)
							{
								Popup.Show("You pony up " + num23 + " " + ((num23 == 1) ? "dram" : "drams") + " of fresh water to even up the trade.");
							}
							else if (Popup.ShowYesNo("You'll have to pony up " + num23 + " " + ((num23 == 1) ? "dram" : "drams") + " of fresh water to even up the trade. Agreed?") == DialogResult.No)
							{
								continue;
							}
						}
						else
						{
							if (!flag11)
							{
								Popup.Show("You don't have " + num23 + " " + ((num23 == 1) ? "dram" : "drams") + " of fresh water to even up the trade!");
								continue;
							}
							int num24 = num23 - freeDrams2;
							if (freeDrams2 > 0)
							{
								Popup.Show("You pony up " + freeDrams2 + " " + ((freeDrams2 == 1) ? "dram" : "drams") + " of fresh water, and now owe " + Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " " + num24 + " " + ((num24 == 1) ? "dram" : "drams") + ".");
							}
							else
							{
								Popup.Show("You now owe " + Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " " + num24 + " " + ((num24 == 1) ? "dram" : "drams") + " of fresh water.");
							}
							Trader.ModIntProperty("TraderCreditExtended", num24);
						}
					}
					if (num23 < 0)
					{
						int num25 = -num23;
						if (AssumeTradersHaveWater || Trader.GetFreeDrams() >= num25)
						{
							if (flag11)
							{
								Popup.Show(Trader.Does("pony", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " up " + num25 + " " + ((num25 == 1) ? "dram" : "drams") + " of fresh water to even up the trade.");
							}
							else if (Popup.ShowYesNo(Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will have to pony up " + num25 + " " + ((num25 == 1) ? "dram" : "drams") + " of fresh water to even up the trade. Agreed?") == DialogResult.No)
							{
								continue;
							}
						}
						else if (flag11)
						{
							Popup.Show(Trader.Does("don't", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " have " + num25 + " " + ((num25 == 1) ? "dram" : "drams") + " of fresh water to even up the trade!");
						}
						else if (Popup.ShowYesNo(Trader.Does("don't", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " have " + num25 + " " + ((num25 == 1) ? "dram" : "drams") + " of fresh water to even up the trade! Do you want to complete the trade anyway?") != 0)
						{
							continue;
						}
					}
					num26 = -num23;
					List<GameObject> list3 = null;
					for (int num27 = 0; num27 < Objects[1].Count; num27++)
					{
						if (NumberSelected[1][num27] > 0)
						{
							if (list3 == null)
							{
								list3 = new List<GameObject>();
							}
							list3.Add(Objects[1][num27].GO);
						}
					}
					int num28 = The.Player.GetStorableDrams("water", null, list3);
					List<GameObject> countForStore = null;
					for (int num29 = 0; num29 < Objects[0].Count; num29++)
					{
						if (NumberSelected[0][num29] > 0 && NumberSelected[0][num29] >= Objects[0][num29].GO.Count)
						{
							if (countForStore == null)
							{
								countForStore = new List<GameObject>();
							}
							countForStore.Add(Objects[0][num29].GO);
						}
					}
					if (countForStore != null)
					{
						num28 += Trader.GetStorableDrams("water", null, null, (GameObject o) => countForStore.Contains(o));
					}
					if (num23 < 0 && num28 < num26)
					{
						if (flag11)
						{
							Popup.Show("You don't have enough water containers to carry that many drams! You can store " + num28 + " " + ((num28 == 1) ? "dram" : "drams") + ".");
						}
						else if (Popup.ShowYesNo("You don't have enough water containers to carry that many drams! Do you want to complete the trade for the " + num28 + " " + ((num28 == 1) ? "dram" : "drams") + " you can store?") == DialogResult.No)
						{
							continue;
						}
					}
					if (screenMode == TradeScreenMode.Container && NumberSelected[0].Any((int x) => x > 0) && !Trader.FireEvent(Event.New("BeforeContentsTaken", "Taker", The.Player)))
					{
						break;
					}
					if (num23 > 0)
					{
						The.Player.UseDrams(num23);
					}
					list4 = new List<GameObject>(16);
					list5 = new List<GameObject>(16);
					num30 = 0;
					goto IL_2d5f;
				}
				continue;
				IL_2d5f:
				for (; num30 < Objects[0].Count; num30++)
				{
					if (NumberSelected[0][num30] > 0)
					{
						GameObject gO14 = Objects[0][num30].GO;
						gO14.SplitStack(NumberSelected[0][num30], Trader);
						if (!TryRemove(gO14, Trader, list5, list4, flag11))
						{
							goto IL_03df;
						}
						gO14.RemoveIntProperty("_stock");
						list5.Add(gO14);
					}
				}
				for (int num31 = 0; num31 < Objects[1].Count; num31++)
				{
					if (NumberSelected[1][num31] > 0)
					{
						GameObject gO15 = Objects[1][num31].GO;
						gO15.SplitStack(NumberSelected[1][num31], The.Player);
						if (!TryRemove(gO15, The.Player, list5, list4, flag11))
						{
							goto IL_03df;
						}
						if (Trader.HasTagOrProperty("Merchant"))
						{
							gO15.SetIntProperty("_stock", 1);
						}
						else if (Trader.HasPart("Container"))
						{
							gO15.SetIntProperty("StoredByPlayer", 1);
						}
						list4.Add(gO15);
					}
				}
				if (screenMode == TradeScreenMode.Container)
				{
					if (list5.Count > 0 && !Trader.FireEvent(Event.New("AfterContentsTaken", "Taker", The.Player)))
					{
						ReturnItems(list5, list4);
						continue;
					}
					foreach (GameObject item in list4)
					{
						try
						{
							if (!Trader.FireEvent(Event.New("CommandTakeObject", "Object", item, "PutBy", The.Player, "EnergyCost", 0)))
							{
								The.Player.ReceiveObject(item);
							}
						}
						catch (Exception x4)
						{
							MetricsManager.LogException("trade move to container", x4);
						}
					}
					break;
				}
				foreach (GameObject item2 in list4)
				{
					try
					{
						if (item2?.pPhysics != null)
						{
							item2.pPhysics.Owner = null;
						}
						Trader.TakeObject(item2, Silent: false, 0);
					}
					catch (Exception x5)
					{
						MetricsManager.LogException("trade move to trader", x5);
					}
				}
				if (list4.Count > 0 && Trader.HasPart("Container"))
				{
					The.Player.FireEvent(Event.New("PutSomethingIn", "Object", Trader));
				}
				break;
			}
			foreach (GameObject item3 in list5)
			{
				try
				{
					if (!The.Player.TakeObject(item3, Silent: false, 0))
					{
						Trader.ReceiveObject(item3);
					}
				}
				catch (Exception x6)
				{
					MetricsManager.LogException("trade move to player", x6);
				}
			}
			try
			{
				if (num23 < 0)
				{
					The.Player.GiveDrams(num26);
					Trader.UseDrams(num26);
				}
				else if (num23 > 0)
				{
					Trader.GiveDrams(num23);
				}
			}
			catch (Exception x7)
			{
				MetricsManager.LogException("trade water exchange", x7);
			}
			if (screenMode == TradeScreenMode.Trade)
			{
				if (list4.Count > 0 || list5.Count > 0)
				{
					Popup.Show("Trade complete!");
				}
				else
				{
					Popup.Show("Nothing to trade.");
				}
			}
		}
	}

	private static bool TryRemove(GameObject Object, GameObject Receiver, List<GameObject> TradeToPlayer, List<GameObject> TradeToTrader, bool Force = false)
	{
		Event e = Event.New("CommandRemoveObject", "Object", Object).SetSilent(Silent: true);
		if (Receiver.FireEvent(e) || Force)
		{
			return true;
		}
		string text = (Receiver.IsPlayer() ? "you" : Receiver.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: true));
		Popup.ShowBlock("Trade could not be completed, " + text + " couldn't drop object: " + Object.DisplayName);
		ReturnItems(TradeToPlayer, TradeToTrader);
		if (GameObject.validate(ref Object))
		{
			Object.CheckStack();
		}
		return false;
	}

	private static void ReturnItems(List<GameObject> TradeToPlayer, List<GameObject> TradeToTrader)
	{
		foreach (GameObject item in TradeToPlayer)
		{
			_Trader.ReceiveObject(item);
		}
		foreach (GameObject item2 in TradeToTrader)
		{
			The.Player.ReceiveObject(item2);
		}
	}

	public static bool RechargeAction(GameObject GO, GameObject Trader)
	{
		bool AnyRelevant = false;
		bool AnyRechargeable = false;
		bool AnyNotFullyCharged = false;
		bool AnyRecharged = false;
		Predicate<IRechargeable> pProc = delegate(IRechargeable P)
		{
			AnyRelevant = true;
			if (!P.CanBeRecharged())
			{
				return true;
			}
			AnyRechargeable = true;
			int rechargeAmount = P.GetRechargeAmount();
			if (rechargeAmount <= 0)
			{
				return true;
			}
			AnyNotFullyCharged = true;
			int num = Math.Max(rechargeAmount / 500, 1);
			string text = ((P.ParentObject.Count > 1) ? "one of those" : P.ParentObject.indicativeDistal);
			if (The.Player.GetFreeDrams() < num)
			{
				Popup.Show("You need {{C|" + Grammar.Cardinal(num) + "}} " + ((num == 1) ? "dram" : "drams") + " of fresh water to charge " + text + ".");
				return false;
			}
			if (Popup.ShowYesNo("You may recharge " + text + " for {{C|" + Grammar.Cardinal(num) + "}} " + ((num == 1) ? "dram" : "drams") + " of fresh water.") == DialogResult.Yes && The.Player.UseDrams(num))
			{
				P.ParentObject.SplitFromStack();
				P.AddCharge(rechargeAmount);
				P.ParentObject.CheckStack();
				Trader.GiveDrams(num);
				AnyRecharged = true;
			}
			return true;
		};
		GO.ForeachPartDescendedFrom(pProc);
		EnergyCellSocket part = GO.GetPart<EnergyCellSocket>();
		if (part != null && part.Cell != null)
		{
			part.Cell.ForeachPartDescendedFrom(pProc);
		}
		if (!AnyRelevant)
		{
			Popup.Show("That item has no cell or rechargeable capacitor in it.");
		}
		else if (!AnyRechargeable)
		{
			Popup.Show("That item cannot be recharged this way.");
		}
		else if (!AnyNotFullyCharged)
		{
			Popup.Show(GO.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + GO.Is + " fully charged!");
		}
		if (AnyRecharged && Options.Sound)
		{
			SoundManager.PlaySound("whine_up");
		}
		return AnyRecharged;
	}
}
