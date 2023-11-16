using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.UI;

public class Sidebar
{
	public static string State = "right";

	public static string VState = "top";

	public static bool _Hidden = false;

	public static int _SidebarState = 0;

	public static XRL.World.GameObject _CurrentTarget = null;

	public static Dictionary<XRL.World.GameObject, string> AutogotItems = new Dictionary<XRL.World.GameObject, string>();

	private static XRL.World.GameObject PlayerBody = null;

	public static string sWeight = "";

	public static string sPlayerHPColor;

	public static int LastHP = -1;

	public static int LastHPBase = -1;

	public static int LastWeight = -1;

	public static int LastMaxWeight = -1;

	public static bool Analgesic = false;

	public static bool WaitingForHPWarning = true;

	public static StringBuilder SB = new StringBuilder(2048);

	public static List<string> MessageCache = null;

	public static float XPBarPercentage = 0f;

	public static bool bOverlayUpdated = false;

	public static int _CurrentXP = 0;

	public static int _NextXP = 0;

	public static string _sPlayerHP = "";

	public static int _CurrentHP = 0;

	public static int _MaxHP = 0;

	public static string _AbilityText;

	public static string _LeftText;

	public static string _RightText;

	public static Dictionary<char, int> Codepage437Mapping = new Dictionary<char, int>
	{
		{ '\0', 32 },
		{ '\u0001', 9786 },
		{ '\u0002', 9787 },
		{ '\u0003', 9829 },
		{ '\u0004', 9830 },
		{ '\u0005', 9827 },
		{ '\u0006', 9824 },
		{ '\a', 8226 },
		{ '\b', 9688 },
		{ '\t', 9675 },
		{ '\v', 9794 },
		{ '\f', 9792 },
		{ '\u000e', 9835 },
		{ '\u000f', 9788 },
		{ '\u0010', 9654 },
		{ '\u0011', 9664 },
		{ '\u0012', 8597 },
		{ '\u0013', 8252 },
		{ '\u0014', 182 },
		{ '\u0015', 167 },
		{ '\u0016', 9602 },
		{ '\u0017', 8616 },
		{ '\u0018', 8593 },
		{ '\u0019', 8595 },
		{ '\u001a', 8594 },
		{ '\u001b', 8592 },
		{ '\u001c', 8735 },
		{ '\u001d', 8596 },
		{ '\u001e', 9650 },
		{ '\u001f', 9660 },
		{ '\u0080', 199 },
		{ '\u0081', 252 },
		{ '\u0082', 233 },
		{ '\u0083', 226 },
		{ '\u0084', 228 },
		{ '\u0085', 224 },
		{ '\u0086', 229 },
		{ '\u0087', 231 },
		{ '\u0088', 234 },
		{ '\u0089', 235 },
		{ '\u008a', 232 },
		{ '\u008b', 239 },
		{ '\u008c', 238 },
		{ '\u008d', 236 },
		{ '\u008e', 196 },
		{ '\u008f', 197 },
		{ '\u0090', 201 },
		{ '\u0091', 230 },
		{ '\u0092', 198 },
		{ '\u0093', 244 },
		{ '\u0094', 246 },
		{ '\u0095', 242 },
		{ '\u0096', 251 },
		{ '\u0097', 249 },
		{ '\u0098', 255 },
		{ '\u0099', 214 },
		{ '\u009a', 220 },
		{ '\u009b', 162 },
		{ '\u009c', 163 },
		{ '\u009d', 165 },
		{ '\u009e', 8359 },
		{ '\u009f', 402 },
		{ '\u00a0', 225 },
		{ '¡', 237 },
		{ '¢', 243 },
		{ '£', 250 },
		{ '¤', 241 },
		{ '¥', 209 },
		{ '¦', 170 },
		{ '§', 186 },
		{ '\u00a8', 191 },
		{ '©', 8976 },
		{ 'ª', 172 },
		{ '«', 189 },
		{ '¬', 188 },
		{ '­', 161 },
		{ '®', 171 },
		{ '\u00af', 187 },
		{ '°', 9617 },
		{ '±', 9618 },
		{ '²', 9619 },
		{ '³', 9474 },
		{ '\u00b4', 9508 },
		{ 'µ', 9569 },
		{ '¶', 9570 },
		{ '·', 9558 },
		{ '\u00b8', 9557 },
		{ '¹', 9571 },
		{ 'º', 9553 },
		{ '»', 9559 },
		{ '¼', 9565 },
		{ '½', 9564 },
		{ '¾', 9563 },
		{ '¿', 9488 },
		{ 'À', 9492 },
		{ 'Á', 9524 },
		{ 'Â', 9516 },
		{ 'Ã', 9500 },
		{ 'Ä', 9472 },
		{ 'Å', 9532 },
		{ 'Æ', 9566 },
		{ 'Ç', 9567 },
		{ 'È', 9562 },
		{ 'É', 9556 },
		{ 'Ê', 9577 },
		{ 'Ë', 9574 },
		{ 'Ì', 9568 },
		{ 'Í', 9552 },
		{ 'Î', 9580 },
		{ 'Ï', 9575 },
		{ 'Ð', 9576 },
		{ 'Ñ', 9572 },
		{ 'Ò', 9573 },
		{ 'Ó', 9561 },
		{ 'Ô', 9560 },
		{ 'Õ', 9554 },
		{ 'Ö', 9555 },
		{ '×', 9579 },
		{ 'Ø', 9578 },
		{ 'Ù', 9496 },
		{ 'Ú', 9484 },
		{ 'Û', 9608 },
		{ 'Ü', 9604 },
		{ 'Ý', 9612 },
		{ 'Þ', 9616 },
		{ 'ß', 9600 },
		{ 'à', 945 },
		{ 'á', 223 },
		{ 'â', 915 },
		{ 'ã', 960 },
		{ 'ä', 931 },
		{ 'å', 963 },
		{ 'æ', 181 },
		{ 'ç', 964 },
		{ 'è', 934 },
		{ 'é', 920 },
		{ 'ê', 937 },
		{ 'ë', 948 },
		{ 'ì', 8734 },
		{ 'í', 966 },
		{ 'î', 949 },
		{ 'ï', 8745 },
		{ 'ð', 8801 },
		{ 'ñ', 177 },
		{ 'ò', 8805 },
		{ 'ó', 8804 },
		{ 'ô', 8992 },
		{ 'õ', 8993 },
		{ 'ö', 247 },
		{ '÷', 8776 },
		{ 'ø', 176 },
		{ 'ù', 8729 },
		{ 'ú', 183 },
		{ 'û', 8730 },
		{ 'ü', 8319 },
		{ 'ý', 178 },
		{ 'þ', 9632 },
		{ 'ÿ', 160 }
	};

	public static Dictionary<char, int> Codepage437Inverse;

	private static StringBuilder RTFFormatterSB = new StringBuilder(2048);

	[NonSerialized]
	public static List<string> Objects = new List<string>();

	public static bool Hidden
	{
		get
		{
			if (Options.ShiftHidesSidebar && Keyboard.bShift)
			{
				return !_Hidden;
			}
			return _Hidden;
		}
		set
		{
			_Hidden = value;
		}
	}

	public static int SidebarState
	{
		get
		{
			return _SidebarState;
		}
		set
		{
			The.Game.Player.Messages.Cache_0_12Valid = false;
			_SidebarState = value;
		}
	}

	public static XRL.World.GameObject CurrentTarget
	{
		get
		{
			XRL.World.GameObject.validate(ref _CurrentTarget);
			return _CurrentTarget;
		}
		set
		{
			if (value != _CurrentTarget)
			{
				_CurrentTarget = value;
				if (_CurrentTarget != null && _CurrentTarget.IsHostileTowards(The.Player))
				{
					AutoAct.Interrupt(null, null, _CurrentTarget);
				}
			}
		}
	}

	public static int CurrentXP
	{
		get
		{
			return _CurrentXP;
		}
		set
		{
			if (_CurrentXP != value)
			{
				bOverlayUpdated = true;
			}
			_CurrentXP = value;
		}
	}

	public static int NextXP
	{
		get
		{
			return _NextXP;
		}
		set
		{
			if (_NextXP != value)
			{
				bOverlayUpdated = true;
			}
			_NextXP = value;
		}
	}

	public static string sPlayerHP
	{
		get
		{
			return _sPlayerHP;
		}
		set
		{
			if (_sPlayerHP != value)
			{
				bOverlayUpdated = true;
			}
			_sPlayerHP = value;
		}
	}

	public static int CurrentHP
	{
		get
		{
			return _CurrentHP;
		}
		set
		{
			if (_CurrentHP != value)
			{
				bOverlayUpdated = true;
			}
			_CurrentHP = value;
		}
	}

	public static int MaxHP
	{
		get
		{
			return _MaxHP;
		}
		set
		{
			if (_MaxHP != value)
			{
				bOverlayUpdated = true;
			}
			_MaxHP = value;
		}
	}

	public static string AbilityText
	{
		get
		{
			return _AbilityText;
		}
		set
		{
			if (_AbilityText != value)
			{
				bOverlayUpdated = true;
			}
			_AbilityText = value;
		}
	}

	public static string LeftText
	{
		get
		{
			return _LeftText;
		}
		set
		{
			if (_LeftText != value)
			{
				bOverlayUpdated = true;
			}
			_LeftText = value;
		}
	}

	public static string RightText
	{
		get
		{
			return _RightText;
		}
		set
		{
			if (_RightText != value)
			{
				bOverlayUpdated = true;
			}
			_RightText = value;
		}
	}

	public static void UpdateState()
	{
		if (XRLCore.Core?.Game?.Player?.Body?.pPhysics == null)
		{
			return;
		}
		XRL.World.Parts.Physics pPhysics = XRLCore.Core.Game.Player.Body.pPhysics;
		if (pPhysics != null && pPhysics.CurrentCell != null)
		{
			if (pPhysics.CurrentCell.Y > 10 && VState == "bottom")
			{
				SetSidebarVState("top");
			}
			if (pPhysics.CurrentCell.Y < 8 && VState == "top")
			{
				SetSidebarVState("bottom");
			}
			if (pPhysics.CurrentCell.X > 42 && State == "right")
			{
				SetSidebarState("left");
			}
			if (pPhysics.CurrentCell.X < 38 && State == "left")
			{
				SetSidebarState("right");
			}
		}
	}

	public static bool AnyAutogotItems()
	{
		return AutogotItems.Count > 0;
	}

	public static void ClearAutogotItems()
	{
		AutogotItems.Clear();
	}

	public static void AddAutogotItem(XRL.World.GameObject GO)
	{
		if (!AutogotItems.ContainsKey(GO) && GO.pRender != null)
		{
			AutogotItems.Add(GO, "<autogot> " + GO.ShortDisplayName);
		}
	}

	public static void SetSidebarVState(string _State)
	{
		VState = _State;
	}

	public static void SetSidebarState(string _State)
	{
		State = _State;
	}

	public static void Update()
	{
		if (PlayerBody == null)
		{
			return;
		}
		if (PlayerBody.pPhysics == null)
		{
			PlayerBody = null;
			return;
		}
		sPlayerHPColor = PlayerBody.GetHPColor();
		if (PlayerBody.GetIntProperty("Analgesia") > 0)
		{
			sPlayerHP = Strings.WoundLevel(PlayerBody);
			Analgesic = true;
		}
		else
		{
			Analgesic = false;
			int hitpoints = PlayerBody.hitpoints;
			int baseHitpoints = PlayerBody.baseHitpoints;
			if (hitpoints != LastHP || baseHitpoints != LastHPBase)
			{
				LastHP = hitpoints;
				LastHPBase = baseHitpoints;
				sPlayerHP = XRL.World.Event.NewStringBuilder().Append(sPlayerHPColor).Append(hitpoints)
					.Append(" &y/ ")
					.Append(LastHPBase)
					.ToString();
			}
		}
		int carriedWeight = PlayerBody.GetCarriedWeight();
		int maxCarriedWeight = PlayerBody.GetMaxCarriedWeight();
		if (carriedWeight != LastWeight || maxCarriedWeight != LastMaxWeight)
		{
			LastWeight = carriedWeight;
			LastMaxWeight = maxCarriedWeight;
			sWeight = XRL.World.Event.NewStringBuilder().Append('#').Append(carriedWeight)
				.Append('/')
				.Append(maxCarriedWeight)
				.ToString();
		}
		PlayerBody.UpdateVisibleStatusColor(sPlayerHPColor);
	}

	public static void DrawMessageBox(ScreenBuffer _ScreenBuffer, int x1, int y1, int x2, int y2)
	{
	}

	private static void PutTargetInSB()
	{
		string text = CurrentTarget.DisplayName;
		if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(SB) > 21)
		{
			text = ConsoleLib.Console.ColorUtility.ClipExceptFormatting(text, 21);
		}
		SB.Clear().Append("{{R|[{{y|").Append(text)
			.Append("}}]}}");
	}

	public static string ToCP437(string s)
	{
		if (s == null)
		{
			return null;
		}
		if (Codepage437Inverse == null)
		{
			Codepage437Inverse = Codepage437Mapping.Where((KeyValuePair<char, int> kv) => kv.Value != 32).ToDictionary((Func<KeyValuePair<char, int>, char>)((KeyValuePair<char, int> kv) => (char)kv.Value), (Func<KeyValuePair<char, int>, int>)((KeyValuePair<char, int> kv) => kv.Key));
		}
		lock (RTFFormatterSB)
		{
			RTFFormatterSB.Length = 0;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (Codepage437Inverse.TryGetValue(c, out var value))
				{
					c = (char)value;
				}
				RTFFormatterSB.Append(c);
			}
			return RTFFormatterSB.ToString();
		}
	}

	public static string FromCP437(string s)
	{
		if (s == null)
		{
			return null;
		}
		lock (RTFFormatterSB)
		{
			RTFFormatterSB.Length = 0;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (Codepage437Mapping.TryGetValue(c, out var value))
				{
					c = (char)value;
				}
				RTFFormatterSB.Append(c);
			}
			return RTFFormatterSB.ToString();
		}
	}

	public static string FormatToRTF(string s, string opacity = "FF")
	{
		if (s == null)
		{
			return "";
		}
		lock (RTFFormatterSB)
		{
			RTFFormatterSB.Length = 0;
			FormatToRTF(s, RTFFormatterSB, opacity);
			return RTFFormatterSB.ToString();
		}
	}

	public static void FormatToRTF(string s, StringBuilder sb, string opacity = "FF")
	{
		if (s == null)
		{
			return;
		}
		s = Markup.Transform(s);
		bool flag = false;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '&')
			{
				if (i < s.Length - 1 && s[i + 1] == '&')
				{
					i++;
					sb.Append("&");
					continue;
				}
				if (flag)
				{
					sb.Append("</color>");
				}
				flag = true;
				i++;
				if (i < s.Length)
				{
					sb.Append("<color=#");
					if (!ConsoleLib.Console.ColorUtility.ColorMap.ContainsKey(s[i]))
					{
						Debug.Log("Unknown color code: " + s[i]);
						sb.Append("DD00DDFF>");
						continue;
					}
					Color color = ConsoleLib.Console.ColorUtility.ColorMap[s[i]];
					sb.Append(((int)Math.Min(color.r * 255f, 255f)).ToString("X2"));
					sb.Append(((int)Math.Min(color.g * 255f, 255f)).ToString("X2"));
					sb.Append(((int)Math.Min(color.b * 255f, 255f)).ToString("X2"));
					sb.Append(opacity);
					sb.Append(">");
				}
			}
			else if (s[i] == '^')
			{
				if (i < s.Length - 1 && s[i + 1] == '^')
				{
					i++;
					sb.Append("^");
				}
				else
				{
					i++;
				}
			}
			else
			{
				char c = s[i];
				if (Codepage437Mapping.TryGetValue(c, out var value))
				{
					c = (char)value;
				}
				sb.Append(c);
			}
		}
		if (flag)
		{
			sb.Append("</color>");
		}
	}

	public static void Render(ScreenBuffer _ScreenBuffer)
	{
		if (GameManager.bDraw == 23 || Keyboard.bAlt || GameManager.bDraw == 24)
		{
			return;
		}
		if (Options.ModernUI)
		{
			PlayerBody = The.Player;
			if (PlayerBody == null)
			{
				return;
			}
			if (XRLCore.Core.Game.Player.Body.GetIntProperty("Analgesia") > 0)
			{
				sPlayerHP = Strings.WoundLevel(XRLCore.Core.Game.Player.Body);
				Analgesic = true;
			}
			else
			{
				Analgesic = false;
				int hitpoints = PlayerBody.hitpoints;
				int baseHitpoints = PlayerBody.baseHitpoints;
				if (hitpoints != LastHP || baseHitpoints != LastHPBase)
				{
					LastHP = hitpoints;
					LastHPBase = baseHitpoints;
					sPlayerHP = XRL.World.Event.NewStringBuilder().Append("&Y").Append(hitpoints)
						.Append(" / ")
						.Append(LastHPBase)
						.ToString();
				}
			}
			int statValue = PlayerBody.GetStatValue("Level");
			int xPForLevel = Leveler.GetXPForLevel(statValue);
			CurrentXP = PlayerBody.GetStatValue("XP");
			NextXP = Leveler.GetXPForLevel(statValue + 1);
			XPBarPercentage = (float)(CurrentXP - xPForLevel) / (float)(NextXP - xPForLevel);
			CurrentHP = PlayerBody.hitpoints;
			MaxHP = PlayerBody.baseHitpoints;
			SB.Length = 0;
			string option = Options.GetOption("OptionDisplayHPWarning");
			if (option != null && option.Contains("%"))
			{
				int num = 0;
				if (option == "10%")
				{
					num = 10;
				}
				if (option == "20%")
				{
					num = 20;
				}
				if (option == "30%")
				{
					num = 30;
				}
				if (option == "40%")
				{
					num = 40;
				}
				if (option == "50%")
				{
					num = 50;
				}
				if (option == "60%")
				{
					num = 60;
				}
				if (option == "70%")
				{
					num = 70;
				}
				if (option == "80%")
				{
					num = 80;
				}
				if (option == "90%")
				{
					num = 90;
				}
				if (option == "100%")
				{
					num = 100;
				}
				if (MaxHP > 0)
				{
					if ((float)(CurrentHP * 100 / MaxHP) < (float)num)
					{
						if (WaitingForHPWarning)
						{
							WaitingForHPWarning = false;
							Popup.ShowSpace("{{R|Your health has dropped below {{C|" + option + "}}!}}");
						}
					}
					else if (!WaitingForHPWarning)
					{
						WaitingForHPWarning = true;
					}
				}
			}
			if (CurrentTarget != null)
			{
				SB.Clear().Append("Target: ").Append(CurrentTarget.DisplayName)
					.Append(" - ")
					.Append(Strings.WoundLevel(CurrentTarget));
				StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
				FormatToRTF(SB.ToString(), stringBuilder);
				SB.Length = 0;
				SB.Append(stringBuilder);
			}
			else
			{
				SB.Append("<color=#FFFFFF>ST:").Append(PlayerBody.Statistics["Strength"].Value).Append("</color> ");
				SB.Append("<color=#d74200>TO:").Append(PlayerBody.Statistics["Toughness"].Value).Append("</color> ");
				SB.Append("<color=#cfc041>AG:").Append(PlayerBody.Statistics["Agility"].Value).Append("</color> ");
				SB.Append("<color=#0096ff>IN:").Append(PlayerBody.Statistics["Intelligence"].Value).Append("</color> ");
				SB.Append("<color=#00c420>WI:").Append(PlayerBody.Statistics["Willpower"].Value).Append("</color> ");
				SB.Append("<color=#da5bd6>EG:").Append(PlayerBody.Statistics["Ego"].Value).Append("</color> ");
				int num2 = PlayerBody.Speed;
				if (PlayerBody.pPhysics.Temperature < PlayerBody.pPhysics.FreezeTemperature && PlayerBody.pPhysics.Temperature > PlayerBody.pPhysics.BrittleTemperature)
				{
					num2 -= (int)(100.0 * (0.5 - (double)(Math.Abs((float)(PlayerBody.pPhysics.Temperature - PlayerBody.pPhysics.BrittleTemperature) / (float)(PlayerBody.pPhysics.FreezeTemperature - PlayerBody.pPhysics.BrittleTemperature)) * 0.5f)));
				}
				SB.AppendFormat(" QN:{0} ", num2);
				SB.AppendFormat("MS:{0} ", 100 - PlayerBody.Statistics["MoveSpeed"].Value + 100);
				SB.AppendFormat("AV:{0} ", PlayerBody.Statistics["AV"].Value);
				SB.AppendFormat("DV:{0} ", Stats.GetCombatDV(PlayerBody));
				SB.AppendFormat("MA:{0} ", Stats.GetCombatMA(PlayerBody));
			}
			LeftText = SB.ToString();
			SB.Length = 0;
			SB.Append("<color=#77bfcf>").Append(Calendar.getTime());
			SB.Append(", ");
			SB.Append(Calendar.getDay());
			SB.Append(" of ");
			SB.Append(Calendar.getMonth()).Append("</color>");
			SB.Append(" ");
			SB.AppendFormat("T:{0}", (PlayerBody.GetPart("Physics") as XRL.World.Parts.Physics).Temperature);
			SB.Append(" ");
			if (PlayerBody.GetPart("Stomach") is Stomach stomach)
			{
				FormatToRTF(stomach.WaterStatus(), SB);
				if (PlayerBody.pPhysics.CurrentCell != null && !PlayerBody.pPhysics.CurrentCell.ParentZone.IsWorldMap())
				{
					SB.Append(",");
					FormatToRTF(stomach.FoodStatus(), SB);
				}
			}
			int carriedWeight = PlayerBody.GetCarriedWeight();
			int maxCarriedWeight = PlayerBody.GetMaxCarriedWeight();
			if (carriedWeight != LastWeight || maxCarriedWeight != LastMaxWeight)
			{
				LastWeight = carriedWeight;
				LastMaxWeight = maxCarriedWeight;
			}
			SB.Append(" ");
			SB.Append(LastWeight).Append("/").Append(LastMaxWeight)
				.Append("#");
			RightText = SB.ToString();
			SB.Length = 0;
			if (PlayerBody.HasPart("ActivatedAbilities"))
			{
				ActivatedAbilities activatedAbilities = PlayerBody.GetPart("ActivatedAbilities") as ActivatedAbilities;
				if (activatedAbilities.AbilityByGuid != null && activatedAbilities.AbilityByGuid.Count > 0)
				{
					foreach (ActivatedAbilityEntry value4 in activatedAbilities.AbilityByGuid.Values)
					{
						string value = "<color=white>";
						string value2 = "yellow";
						if (value4.Cooldown > 0)
						{
							value = "<color=grey>";
							SB.Append(value);
							FormatToRTF(value4.DisplayName, SB);
							SB.Append(" [").Append(value4.CooldownTurns).Append("]");
							value2 = "grey";
						}
						else
						{
							if (!value4.Enabled)
							{
								value = "<color=grey>";
								value2 = "grey";
							}
							if (value4.Toggleable && !value4.ToggleState)
							{
								value = "<color=red>";
							}
							if (value4.Toggleable && value4.ToggleState)
							{
								value = "<color=green>";
							}
							SB.Append(value);
							FormatToRTF(value4.DisplayName, SB);
						}
						SB.Append("</color>");
						if (!string.IsNullOrEmpty(value4.Command) && AbilityManager.commandToKey.ContainsKey(value4.Command))
						{
							SB.Append(" <<color=");
							SB.Append(value2);
							SB.Append(">");
							Keyboard.MetaToString(AbilityManager.commandToKey[value4.Command], SB);
							SB.Append("</color>>");
						}
						SB.Append("    ");
					}
				}
			}
			AbilityText = SB.ToString();
			return;
		}
		if (XRLCore.Core.Game.Player.Body != PlayerBody)
		{
			PlayerBody = XRLCore.Core.Game.Player.Body;
			if (PlayerBody == null)
			{
				return;
			}
			Update();
		}
		if (GameManager.bDraw == 25 || GameManager.bDraw == 26)
		{
			return;
		}
		int num3 = 0;
		if (Hidden)
		{
			if (State == "left")
			{
				_ScreenBuffer.Goto(0, 0);
			}
			if (State == "right")
			{
				_ScreenBuffer.Goto(77, 0);
			}
			_ScreenBuffer.Write("[&W/&y]");
			if (State == "left")
			{
				_ScreenBuffer.Goto(0, 1);
			}
			if (State == "right")
			{
				_ScreenBuffer.Goto(80 - sPlayerHP.Length, 1);
			}
			_ScreenBuffer.Write(sPlayerHP);
			if (State == "left")
			{
				num3 = 0;
			}
			if (State == "right")
			{
				num3 = 67;
			}
			if (CurrentTarget != null)
			{
				_ScreenBuffer.Goto(num3 + 1, 2);
				_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
			}
			RenderCurrentCellPopup(_ScreenBuffer);
		}
		else
		{
			if (GameManager.bDraw == 27)
			{
				return;
			}
			int num4 = 60;
			num3 = 61;
			if (State == "left")
			{
				num4 = 25;
				num3 = 0;
			}
			else if (State == "right")
			{
				num4 = 55;
				num3 = 56;
			}
			_ScreenBuffer.Goto(num4, 0);
			for (int i = 0; i < 25; i++)
			{
				_ScreenBuffer.Goto(num4, i);
				_ScreenBuffer.Write(179);
			}
			if (State == "left")
			{
				_ScreenBuffer.Fill(0, 0, num4 - 1, 24, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			}
			if (State == "right")
			{
				_ScreenBuffer.Fill(num4 + 1, 0, 79, 24, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			}
			if (GameManager.bDraw == 28)
			{
				return;
			}
			if (SidebarState == 3)
			{
				if (XRLCore.Core.HostileWalkObjects.Count > 0)
				{
					XRLCore.Core.HostileWalkObjects.Sort(new XRLCore.SortObjectBydistanceToPlayer());
					for (int j = 0; j < XRLCore.Core.HostileWalkObjects.Count && j < 14; j++)
					{
						string text = XRLCore.Core.HostileWalkObjects[j].DisplayName;
						if (text.Length > 20)
						{
							text = text.Substring(0, 20);
						}
						if (State == "left")
						{
							_ScreenBuffer.Goto(0, j);
						}
						if (State == "right")
						{
							_ScreenBuffer.Goto(num4 + 1, j);
						}
						XRL.World.GameObject gameObject = XRLCore.Core.HostileWalkObjects[j];
						if (gameObject == CurrentTarget)
						{
							_ScreenBuffer.Write("^r" + text);
						}
						else
						{
							_ScreenBuffer.Write(text);
						}
						SB.Length = 0;
						SB.Append(gameObject.hitpoints);
						SB.Append("/");
						SB.Append(gameObject.baseHitpoints);
						SB.Append('\u0003');
						int num5 = 100;
						if (gameObject.baseHitpoints != 0)
						{
							num5 = gameObject.hitpoints * 100 / gameObject.baseHitpoints;
						}
						if (num5 < 15)
						{
							_ScreenBuffer.Write("&r");
							_ScreenBuffer.Write(SB);
						}
						else if (num5 < 33)
						{
							_ScreenBuffer.Write("&R");
							_ScreenBuffer.Write(SB);
						}
						else if (num5 < 66)
						{
							_ScreenBuffer.Write("&W");
							_ScreenBuffer.Write(SB);
						}
						else if (num5 < 100)
						{
							_ScreenBuffer.Write("&G");
							_ScreenBuffer.Write(SB);
						}
						else
						{
							_ScreenBuffer.Write("&Y");
							_ScreenBuffer.Write(SB);
						}
					}
				}
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 16, num4, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num4 + 1, 16, 79, 24);
					}
				}
				else
				{
					StringBuilder lines = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines, 0, 16, num4, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines, num4 + 1, 16, 79, 24);
					}
				}
				if (State == "left")
				{
					_ScreenBuffer.Goto(0, 14);
				}
				if (State == "right")
				{
					_ScreenBuffer.Goto(num4 + 2, 14);
				}
				_ScreenBuffer.Write(sPlayerHP);
				if (State == "left")
				{
					_ScreenBuffer.Goto(0, 15);
				}
				if (State == "right")
				{
					_ScreenBuffer.Goto(num4 + 1, 15);
				}
				if (CurrentTarget != null)
				{
					_ScreenBuffer.Goto(num3 + 1, 12);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num3 + 1, 13);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
			}
			else if (SidebarState == 2)
			{
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 16, num4, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num4 + 1, 16, 79, 24);
					}
				}
				else
				{
					StringBuilder lines2 = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines2, 0, 16, num4, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines2, num4 + 1, 16, 79, 24);
					}
				}
				if (CurrentTarget != null)
				{
					_ScreenBuffer.Goto(num3 + 1, 12);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num3 + 1, 13);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
				_ScreenBuffer.Goto(num3 + 1, 11);
				_ScreenBuffer.Write("HP: ");
				_ScreenBuffer.Write(sPlayerHP);
				_ScreenBuffer.Goto(num3 + 15, 11);
				_ScreenBuffer.Write(sWeight);
				List<ActivatedAbilityEntry> list = new List<ActivatedAbilityEntry>();
				int num6 = 0;
				foreach (int key in AbilityManager.keyToAbility.Keys)
				{
					if (num6 > 11)
					{
						break;
					}
					ActivatedAbilityEntry activatedAbilityEntry = AbilityManager.keyToAbility[key];
					list.Add(activatedAbilityEntry);
					_ScreenBuffer.Goto(num3, num6);
					SB.Length = 0;
					string value3 = "y";
					if (activatedAbilityEntry.Toggleable)
					{
						value3 = (activatedAbilityEntry.ToggleState ? "g" : "r");
					}
					else if (!activatedAbilityEntry.Enabled || activatedAbilityEntry.Cooldown > 0)
					{
						value3 = "K";
					}
					SB.Append("{{").Append(value3).Append('|')
						.Append(Keyboard.MetaToString(key));
					if (activatedAbilityEntry.Cooldown > 0)
					{
						SB.Append("{{Y|({{C|").Append(activatedAbilityEntry.CooldownTurns).Append("}})-}}")
							.Append(activatedAbilityEntry.DisplayName);
					}
					else
					{
						SB.Append("{{y|-}}").Append(activatedAbilityEntry.DisplayName);
					}
					SB.Append("}}");
					string s = SB.ToString();
					if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(s) > 25)
					{
						s = ConsoleLib.Console.ColorUtility.ClipExceptFormatting(s, 25);
					}
					_ScreenBuffer.Write(s);
					num6++;
				}
				ActivatedAbilities activatedAbilities2 = The.Player.ActivatedAbilities;
				if (activatedAbilities2 != null)
				{
					foreach (Guid key2 in activatedAbilities2.AbilityByGuid.Keys)
					{
						if (num6 > 11)
						{
							break;
						}
						ActivatedAbilityEntry activatedAbilityEntry2 = activatedAbilities2.AbilityByGuid[key2];
						if (!list.CleanContains(activatedAbilityEntry2))
						{
							list.Add(activatedAbilityEntry2);
							_ScreenBuffer.Goto(num3, num6);
							string text2 = "";
							string text3 = "y";
							if (activatedAbilityEntry2.Toggleable)
							{
								text3 = (activatedAbilityEntry2.ToggleState ? "g" : "r");
							}
							else if (!activatedAbilityEntry2.Enabled || activatedAbilityEntry2.Cooldown > 0)
							{
								text3 = "K";
							}
							text2 = ((activatedAbilityEntry2.Cooldown <= 0) ? (text2 + "{{" + text3 + "|" + activatedAbilityEntry2.DisplayName + "}}") : (text2 + "{{Y|({{C|" + activatedAbilityEntry2.CooldownTurns + "}})-}}{{" + text3 + "|" + activatedAbilityEntry2.DisplayName + "}}"));
							if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text2) > 25)
							{
								text2 = ConsoleLib.Console.ColorUtility.ClipExceptFormatting(text2, 25);
							}
							_ScreenBuffer.Write(text2);
							num6++;
						}
					}
				}
			}
			else if (SidebarState == 1)
			{
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 1, num4, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num4 + 1, 1, 79, 24);
					}
				}
				else
				{
					StringBuilder lines3 = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines3, 0, 1, num4, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines3, num4 + 1, 1, 79, 24);
					}
				}
				if (CurrentTarget != null && CurrentTarget.HasStat("Hitpoints"))
				{
					_ScreenBuffer.Goto(num3 + 1, 1);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num3 + 1, 2);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
				_ScreenBuffer.Goto(num3 + 1, 0);
				_ScreenBuffer.Write("HP: " + sPlayerHP);
				_ScreenBuffer.Goto(num3 + 15, 0);
				_ScreenBuffer.Write(sWeight);
			}
			else if (SidebarState == 0)
			{
				if (GameManager.bDraw == 29 || GameManager.bDraw == 30)
				{
					return;
				}
				if (XRLCore.Core.Game.Player.Messages.Cache_0_12Valid)
				{
					if (State == "left")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, 0, 16, num4, 24);
					}
					else if (State == "right")
					{
						Text.DrawBottomToTop(_ScreenBuffer, MessageCache, num4 + 1, 16, 79, 24);
					}
				}
				else
				{
					StringBuilder lines4 = XRLCore.Core.Game.Player.Messages.GetLines(0, 12);
					if (State == "left")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines4, 0, 16, num4, 24);
					}
					else if (State == "right")
					{
						MessageCache = Text.DrawBottomToTop(_ScreenBuffer, lines4, num4 + 1, 16, 79, 24);
					}
				}
				if (GameManager.bDraw == 31)
				{
					return;
				}
				if (CurrentTarget != null && CurrentTarget.HasStat("Hitpoints"))
				{
					_ScreenBuffer.Goto(num3 + 1, 12);
					PutTargetInSB();
					_ScreenBuffer.Write(SB);
					_ScreenBuffer.Goto(num3 + 1, 13);
					_ScreenBuffer.Write(Strings.WoundLevel(CurrentTarget));
				}
				if (GameManager.bDraw == 32)
				{
					return;
				}
				_ScreenBuffer.Goto(num3 + 1, 0);
				if (Options.LocationIntseadOfName)
				{
					if (The.Player?.CurrentZone != null)
					{
						_ScreenBuffer.Write(StringFormat.ClipLine(WorldFactory.Factory.ZoneDisplayName(The.Player.CurrentZone.ZoneID), 22));
					}
					else
					{
						_ScreenBuffer.Write("unknown");
					}
				}
				else if (XRLCore.Core.Game.Player.Body.HasEffect("Dominated"))
				{
					if (("Dom: " + XRLCore.Core.Game.Player.Body.DisplayName).Length < 24)
					{
						_ScreenBuffer.Write("Dom: " + XRLCore.Core.Game.Player.Body.DisplayName);
					}
					else
					{
						_ScreenBuffer.Write(("Dom: " + XRLCore.Core.Game.Player.Body.DisplayName).Substring(0, 24));
					}
				}
				else
				{
					_ScreenBuffer.Write(XRLCore.Core.Game.PlayerName);
				}
				if (GameManager.bDraw == 33)
				{
					return;
				}
				_ScreenBuffer.Goto(num3 + 1, 1);
				StringBuilder stringBuilder2 = XRL.World.Event.NewStringBuilder();
				stringBuilder2.Append("&YST&y: &C").Append(PlayerBody.Statistics["Strength"].Value);
				_ScreenBuffer.Write(stringBuilder2);
				_ScreenBuffer.Goto(num3 + 9, 1);
				stringBuilder2.Length = 0;
				stringBuilder2.Append("&WAG&y: &C").Append(PlayerBody.Statistics["Agility"].Value);
				_ScreenBuffer.Write(stringBuilder2);
				_ScreenBuffer.Goto(num3 + 17, 1);
				int num7 = PlayerBody.Speed;
				if (PlayerBody.pPhysics.Temperature < PlayerBody.pPhysics.FreezeTemperature && PlayerBody.pPhysics.Temperature > PlayerBody.pPhysics.BrittleTemperature)
				{
					num7 -= (int)(100.0 * (0.5 - (double)(Math.Abs((float)(PlayerBody.pPhysics.Temperature - PlayerBody.pPhysics.BrittleTemperature) / (float)(PlayerBody.pPhysics.FreezeTemperature - PlayerBody.pPhysics.BrittleTemperature)) * 0.5f)));
				}
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("QN: {0}", num7));
				_ScreenBuffer.Goto(num3 + 17, 2);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("MS: {0}", 100 - PlayerBody.Statistics["MoveSpeed"].Value + 100));
				_ScreenBuffer.Goto(num3 + 1, 2);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&RTO&y: &C{0}", PlayerBody.Statistics["Toughness"].Value));
				_ScreenBuffer.Goto(num3 + 9, 2);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&GWI&y: &C{0}", PlayerBody.Statistics["Willpower"].Value));
				_ScreenBuffer.Goto(num3 + 17, 3);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("T: {0}", (PlayerBody.GetPart("Physics") as XRL.World.Parts.Physics).Temperature));
				_ScreenBuffer.Goto(num3 + 1, 3);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&BIN&y: &C{0}", PlayerBody.Statistics["Intelligence"].Value));
				_ScreenBuffer.Goto(num3 + 9, 3);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("&MEG&y: &C{0}", PlayerBody.Statistics["Ego"].Value));
				_ScreenBuffer.Goto(num3 + 1, 4);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("AV: {0}", PlayerBody.Statistics["AV"].Value));
				_ScreenBuffer.Goto(num3 + 9, 4);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("DV: {0}", Stats.GetCombatDV(PlayerBody)));
				_ScreenBuffer.Goto(num3 + 17, 4);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("MA: {0}", Stats.GetCombatMA(PlayerBody)));
				if (GameManager.bDraw == 34)
				{
					return;
				}
				_ScreenBuffer.Goto(num3 + 1, 5);
				_ScreenBuffer.Write(XRL.World.Event.NewStringBuilder().AppendFormat("XP: {0}&K / {1}", PlayerBody.Statistics["XP"].Value, Leveler.GetXPForLevel(PlayerBody.Statistics["Level"].Value + 1)));
				if (PlayerBody.GetPart("Stomach") is Stomach stomach2)
				{
					_ScreenBuffer.Goto(num3 + 1, 6);
					_ScreenBuffer.Write(stomach2.WaterStatus());
					if (PlayerBody.pPhysics.CurrentCell != null && !PlayerBody.pPhysics.CurrentCell.ParentZone.IsWorldMap())
					{
						_ScreenBuffer.Write("&y,");
						_ScreenBuffer.Write(stomach2.FoodStatus());
					}
				}
				_ScreenBuffer.Goto(num3 + 1, 7);
				_ScreenBuffer.Write("HP: ");
				_ScreenBuffer.Write(sPlayerHP);
				_ScreenBuffer.Goto(num3 + 15, 7);
				_ScreenBuffer.Write(sWeight);
				_ScreenBuffer.Goto(num3 + 1, 9);
				_ScreenBuffer.Write(Calendar.getDay());
				_ScreenBuffer.Write(" of ");
				_ScreenBuffer.Write(Calendar.getMonth());
				_ScreenBuffer.Goto(num3 + 1, 10);
				_ScreenBuffer.Write(Calendar.getTime());
				RenderMissleStatus(PlayerBody, num3 + 1, 11, _ScreenBuffer);
				if (GameManager.bDraw == 35)
				{
					return;
				}
			}
			if (Options.GetOption("OptionShowSidebarAbilities") == "Yes")
			{
				RenderAbilityStatus(PlayerBody, num4, 3, _ScreenBuffer);
			}
			_ScreenBuffer.Goto(num4, 19);
			_ScreenBuffer.Write(193);
			_ScreenBuffer.Goto(num4, 20);
			_ScreenBuffer.Write("&W*&y");
			_ScreenBuffer.Goto(num4, 21);
			_ScreenBuffer.Write("&W/&y");
			_ScreenBuffer.Goto(num4, 22);
			_ScreenBuffer.Write(194);
			if (GameManager.bDraw != 36)
			{
				RenderCurrentCellPopup(_ScreenBuffer);
				_ = GameManager.bDraw;
				_ = 37;
			}
		}
	}

	private static void RenderAbilityStatus(XRL.World.GameObject Player, int xp, int yp, ScreenBuffer _Buffer)
	{
		if (!Player.HasPart("ActivatedAbilities"))
		{
			return;
		}
		ActivatedAbilities activatedAbilities = Player.GetPart("ActivatedAbilities") as ActivatedAbilities;
		if (activatedAbilities.AbilityByGuid == null || activatedAbilities.AbilityByGuid.Count <= 0)
		{
			return;
		}
		int num = yp;
		_Buffer.Goto(xp, num);
		_Buffer.Write("Á");
		foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
		{
			num++;
			string text = "&G";
			if (!value.Enabled)
			{
				text = "&K";
			}
			if (value.Cooldown > 0)
			{
				text = "&K";
			}
			if (value.Toggleable && !value.ToggleState)
			{
				text = "&r";
			}
			if (value.Toggleable && value.ToggleState)
			{
				text = "&g";
			}
			_Buffer.Goto(xp, num);
			_Buffer.Write(text + value.Icon);
		}
		num++;
		_Buffer.Goto(xp, num);
		_Buffer.Write("Â");
	}

	private static void RenderMissleStatus(XRL.World.GameObject Player, int xp, int yp, ScreenBuffer _Buffer)
	{
		List<XRL.World.GameObject> missileWeapons = Player.GetMissileWeapons();
		if (missileWeapons == null || missileWeapons.Count <= 0)
		{
			return;
		}
		int num = 0;
		while (num < missileWeapons.Count && num < 4)
		{
			if (missileWeapons[num].GetPart("MissileWeapon") is MissileWeapon missileWeapon)
			{
				string s = missileWeapon.Status();
				if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(s) > 24)
				{
					s = ConsoleLib.Console.ColorUtility.ClipExceptFormatting(s, 24);
				}
				_Buffer.WriteAt(xp, yp, s);
				yp++;
				num++;
			}
		}
	}

	private static void RenderCurrentCellPopup(ScreenBuffer _Buffer)
	{
		if (Options.GetOption("OptionShowCurrentCellPopup") != "Yes")
		{
			return;
		}
		XRL.World.GameObject Player = XRLCore.Core.Game.Player.Body;
		if (Player.pPhysics.CurrentCell == null)
		{
			return;
		}
		Objects.Clear();
		foreach (XRL.World.GameObject key in AutogotItems.Keys)
		{
			Objects.Add(AutogotItems[key]);
		}
		foreach (XRL.World.GameObject item in Player.CurrentCell.LoopObjectsWithPart("Physics", (XRL.World.GameObject GO) => GO != Player && GO.IsTakeable()))
		{
			Objects.Add(item.ShortDisplayName);
		}
		List<string> list = new List<string>(Objects.Count);
		int num = 0;
		int num2 = 0;
		foreach (string @object in Objects)
		{
			num2++;
			if (num2 == 10)
			{
				list.Add("<more...>");
				break;
			}
			list.Add(@object + "\n");
			string text = ConsoleLib.Console.ColorUtility.StripFormatting(@object);
			if (text.Length > num)
			{
				num = text.Length;
			}
		}
		num++;
		int num3;
		int num4;
		if ((Hidden && State == "left") || (!Hidden && State == "right"))
		{
			num3 = 0;
			num4 = ((VState == "bottom") ? (24 - Objects.Count - 1) : (Hidden ? 3 : 0));
		}
		else
		{
			num3 = 80 - num - 3;
			num4 = ((VState == "bottom") ? (24 - Objects.Count - 1) : (Hidden ? 3 : 0));
		}
		if (num > 0 && Objects.Count > 0)
		{
			_Buffer.Fill(num3, num4, num3 + num + 2, num4 + list.Count + 1, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			_Buffer.ThickSingleBox(num3, num4, num3 + num + 2, num4 + list.Count + 1, ConsoleLib.Console.ColorUtility.MakeColor(ConsoleLib.Console.ColorUtility.Bright(TextColor.Black), TextColor.Black));
		}
		int num5 = 0;
		foreach (string item2 in list)
		{
			_Buffer.Goto(num3 + 2, num4 + 1 + num5);
			_Buffer.Write(item2);
			num5++;
		}
	}
}
