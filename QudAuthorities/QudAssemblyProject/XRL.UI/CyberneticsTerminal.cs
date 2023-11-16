using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsTerminal
{
	public static CyberneticsTerminal instance;

	public List<CyberneticsCreditWedge> Wedges = new List<CyberneticsCreditWedge>();

	public List<GameObject> Implants = new List<GameObject>();

	public int nLicenses;

	public int nFreeLicenses;

	public int nLicensesUsed;

	public int nCredits;

	public int nSelected;

	public int Position;

	public int nTopLine;

	public GameObject terminal;

	public GameObject obj;

	public TerminalScreen _currentScreen;

	public TerminalScreen currentScreen
	{
		get
		{
			return _currentScreen;
		}
		set
		{
			_currentScreen = value;
			if (_currentScreen == null)
			{
				return;
			}
			nTopLine = 0;
			nSelected = 0;
			Position = 0;
			_currentScreen.Update();
			Wedges.Clear();
			nCredits = 0;
			obj.ForeachInventoryAndEquipment(delegate(GameObject GO)
			{
				if (GO.GetPart("CyberneticsCreditWedge") is CyberneticsCreditWedge cyberneticsCreditWedge && cyberneticsCreditWedge.Credits > 0)
				{
					nCredits += cyberneticsCreditWedge.Credits * GO.Count;
					Wedges.Add(cyberneticsCreditWedge);
				}
			});
			nLicenses = obj.GetIntProperty("CyberneticsLicenses");
			nFreeLicenses = obj.GetIntProperty("FreeCyberneticsLicenses");
			Implants.Clear();
			terminal.pPhysics.CurrentCell.ForeachAdjacentCell(delegate(Cell C)
			{
				C.ForeachObjectWithPart("Container", delegate(GameObject GO)
				{
					GO.Inventory.ForeachObject(delegate(GameObject obj2)
					{
						if (obj2.IsImplant && obj2.Understood())
						{
							Implants.Add(obj2);
						}
					});
				});
			});
			obj.Inventory.ForeachObject(delegate(GameObject obj2)
			{
				if (obj2.IsImplant && obj2.Understood())
				{
					Implants.Add(obj2);
				}
			});
			nLicensesUsed = 0;
			obj.Body.ForeachInstalledCybernetics(delegate(GameObject obj2)
			{
				if (obj2.IsImplant)
				{
					nLicensesUsed += obj2.GetPart<CyberneticsBaseItem>().Cost;
				}
			});
		}
	}

	public string currentText => currentScreen.renderedText;

	public bool Authorized
	{
		get
		{
			if (obj != null && obj.IsTrueKin())
			{
				return true;
			}
			return HackActive;
		}
	}

	public int HackLevel
	{
		get
		{
			if (terminal == null)
			{
				return 0;
			}
			return terminal.GetIntProperty("HackLevel");
		}
	}

	public int SecurityHardeningLevel
	{
		get
		{
			if (terminal == null)
			{
				return 0;
			}
			if (!terminal.HasIntProperty("SecurityHardeningLevel"))
			{
				terminal.SetIntProperty("SecurityHardeningLevel", Stat.Random(0, 4));
			}
			return terminal.GetIntProperty("SecurityHardeningLevel");
		}
	}

	public int SecurityAlertLevel
	{
		get
		{
			if (terminal == null)
			{
				return 0;
			}
			return terminal.GetIntProperty("SecurityAlertLevel");
		}
	}

	public bool HackActive => HackLevel > SecurityAlertLevel;

	public static void ShowTerminal(GameObject Terminal, GameObject Object, TerminalScreen startingScreen)
	{
		instance = new CyberneticsTerminal();
		instance._ShowTerminal(Terminal, Object, startingScreen);
		instance = null;
	}

	public static void ShowTerminal(GameObject Terminal, GameObject Object)
	{
		instance = new CyberneticsTerminal();
		instance._ShowTerminal(Terminal, Object);
		instance = null;
	}

	public void _ShowTerminal(GameObject Terminal, GameObject obj, TerminalScreen startingScreen = null)
	{
		GameManager.Instance.PushGameView("CyberneticsTerminal");
		terminal = Terminal;
		this.obj = obj;
		if (startingScreen == null)
		{
			startingScreen = ((!Authorized) ? ((CyberneticsScreen)new CyberneticsScreenGoodbye()) : ((CyberneticsScreen)new CyberneticsScreenMainMenu()));
		}
		TextConsole.LoadScrapBuffers();
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer2;
		ScreenBuffer scrapBuffer2 = TextConsole.ScrapBuffer2;
		currentScreen = startingScreen;
		Keys keys = Keys.None;
		bool flag = false;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Stopwatch stopwatch2 = new Stopwatch();
		stopwatch2.Start();
		int num = 3;
		int num2 = 76;
		while (!flag)
		{
			Event.ResetPool();
			scrapBuffer2.Copy(scrapBuffer);
			scrapBuffer2.Fill(num, 2, num2, 24, 32, ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			scrapBuffer2.BeveledBox(num, 2, num2, 24, ColorUtility.Bright(TextColor.Black), 1);
			scrapBuffer2.Goto(20, 2);
			if (currentScreen != null)
			{
				currentScreen.BeforeRender(scrapBuffer2);
			}
			if (stopwatch2.ElapsedMilliseconds > 15)
			{
				Position++;
				stopwatch2.Reset();
				stopwatch2.Start();
			}
			int num3 = num + 2;
			int num4 = 4;
			int num5 = 0;
			string text = "";
			int i;
			for (i = 0; i < Position && i < currentScreen.renderedText.Length; i++)
			{
				if (num3 > num2 - 2)
				{
					if (num5 >= nTopLine)
					{
						num4++;
					}
					num3 = num + 2;
					num5++;
					if (num5 >= nTopLine + 19)
					{
						scrapBuffer2.Goto(num + 2, num4);
						scrapBuffer2.Write("<more...>");
						break;
					}
				}
				if (currentText[i] == '\r')
				{
					continue;
				}
				if (currentText[i] == '&')
				{
					if (i < currentScreen.renderedText.Length - 1 && currentText[i + 1] == '&')
					{
						i++;
						if (num5 >= nTopLine)
						{
							scrapBuffer2.Goto(num3, num4);
							scrapBuffer2.Write(text + "&&");
						}
						num3++;
					}
					else
					{
						text = "";
						text += currentText.Substring(i, 1);
						i++;
						text += currentText.Substring(i, 1);
					}
				}
				else if (currentText[i] == '\n')
				{
					if (num5 >= nTopLine)
					{
						num4++;
					}
					num3 = num + 2;
					num5++;
					if (num5 >= nTopLine + 19)
					{
						scrapBuffer2.Goto(num + 2, num4);
						scrapBuffer2.Write("<more...>");
						break;
					}
				}
				else
				{
					if (num5 >= nTopLine)
					{
						scrapBuffer2.Goto(num3, num4);
						scrapBuffer2.Write(text + currentText.Substring(i, 1).ToUpper());
					}
					num3++;
				}
			}
			if (i >= currentScreen.renderedText.Length)
			{
				currentScreen.TextComplete();
			}
			if (stopwatch.ElapsedMilliseconds % 1000 > 500)
			{
				scrapBuffer2.Write("_");
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer2);
			if (Keyboard.kbhit())
			{
				currentScreen.TextComplete();
				keys = Keyboard.getvk(MapDirectionToArrows: true, pumpActions: false, wait: false);
				if (Position < currentScreen.renderedText.Length + 1)
				{
					Position = currentScreen.renderedText.Length + 1;
					continue;
				}
				bool flag2 = false;
				if (currentScreen.HackOption != -1 && keys == (Keys)131085)
				{
					nSelected = currentScreen.HackOption;
					flag2 = true;
				}
				else if (keys >= Keys.A && keys <= Keys.Z)
				{
					int num6 = (int)(keys - 65);
					if (currentScreen.HackOption == -1)
					{
						if (num6 < currentScreen.Options.Count)
						{
							nSelected = num6;
							flag2 = true;
						}
					}
					else if (num6 < currentScreen.Options.Count - 1)
					{
						nSelected = ((num6 >= currentScreen.HackOption) ? (num6 - 1) : num6);
						flag2 = true;
					}
				}
				if (flag2)
				{
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 19 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 19;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					keys = Keys.Space;
				}
				if (keys == Keys.Escape)
				{
					currentScreen.Back();
				}
				if (keys == Keys.Space || keys == Keys.Enter)
				{
					currentScreen.Activate();
				}
				if (currentScreen == null)
				{
					flag = true;
				}
				switch (keys)
				{
				case Keys.Prior:
					if (nSelected < 10)
					{
						nSelected = 0;
					}
					else
					{
						nSelected -= 10;
					}
					if (nSelected < 0)
					{
						nSelected = currentScreen.Options.Count - 1;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				case Keys.Next:
					if (nSelected < currentScreen.Options.Count - 11)
					{
						nSelected += 10;
					}
					else
					{
						nSelected = currentScreen.Options.Count - 1;
					}
					if (nSelected >= currentScreen.Options.Count)
					{
						nSelected = 0;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				case Keys.NumPad8:
					nSelected--;
					if (nSelected < 0)
					{
						nSelected = currentScreen.Options.Count - 1;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				case Keys.NumPad2:
					nSelected++;
					if (nSelected >= currentScreen.Options.Count)
					{
						nSelected = 0;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				}
			}
			else
			{
				Thread.Sleep(7);
			}
		}
		GameManager.Instance.PopGameView();
	}

	public bool AttemptHack()
	{
		int num = SecurityHardeningLevel + SecurityAlertLevel;
		HackingSifrah hackingSifrah = new HackingSifrah(terminal, 4 + num, 2 + num * 2, obj?.Stat("Intelligence") ?? 0);
		hackingSifrah.HandlerID = terminal.id;
		hackingSifrah.HandlerPartName = "CyberneticsTerminal2";
		hackingSifrah.Play(terminal);
		if (hackingSifrah.InterfaceExitRequested)
		{
			return false;
		}
		return HackActive;
	}

	public bool checkSecurity(int alertChance, TerminalScreen screen)
	{
		if (HackActive && alertChance.in100())
		{
			terminal.ModIntProperty("SecurityAlertLevel", 1);
		}
		if (Authorized)
		{
			currentScreen = screen;
			return true;
		}
		currentScreen = new CyberneticsScreenGoodbye();
		return false;
	}

	public bool checkSecurity(int alertChance, int times, TerminalScreen screen)
	{
		if (HackActive)
		{
			for (int i = 0; i < times; i++)
			{
				if (alertChance.in100())
				{
					terminal.ModIntProperty("SecurityAlertLevel", 1);
					break;
				}
			}
		}
		if (Authorized)
		{
			currentScreen = screen;
			return true;
		}
		currentScreen = new CyberneticsScreenGoodbye();
		return false;
	}
}
