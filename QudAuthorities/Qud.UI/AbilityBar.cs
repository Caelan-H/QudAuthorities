using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using TMPro;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace Qud.UI;

[ExecuteAlways]
[UIView("AbilityBar", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "AbilityBar", UICanvasHost = 1)]
public class AbilityBar : SingletonWindowBase<AbilityBar>
{
	private struct AbilityDescription : IEquatable<AbilityDescription>
	{
		public int KeyCode;

		public ActivatedAbilityEntry Entry;

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			AbilityDescription abilityDescription = (AbilityDescription)obj;
			if (KeyCode == abilityDescription.KeyCode)
			{
				return Entry.Equals(abilityDescription.Entry);
			}
			return false;
		}

		public bool Equals(AbilityDescription obj)
		{
			if (KeyCode == obj.KeyCode)
			{
				return Entry.Equals(obj.Entry);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return new { KeyCode, Entry }.GetHashCode();
		}
	}

	private class AbilityDescriptionSort : IComparer<AbilityDescription>
	{
		public int Compare(AbilityDescription a, AbilityDescription b)
		{
			int num = a.KeyCode - b.KeyCode;
			if (num == 0)
			{
				return string.Compare(a.Entry.DisplayName, b.Entry.DisplayName);
			}
			return num;
		}
	}

	public UITextSkin EffectText;

	public RectTransform ButtonArea;

	public RectTransform TargetArea;

	public UITextSkin TargetText;

	public UITextSkin TargetHealthText;

	public UnityEngine.GameObject ButtonPrefab;

	public TextMeshProUGUI PageText;

	public UnityEngine.GameObject PagerArea;

	public UITextSkin AbilityCommandText;

	public UITextSkin CycleCommandText;

	private List<UnityEngine.GameObject> AbilityButtons = new List<UnityEngine.GameObject>();

	private StringBuilder SB = new StringBuilder();

	private object effectLock = new object();

	private string effectText;

	private string lastEffectText;

	private bool effectTextDirty;

	private object targetLock = new object();

	private string targetText;

	private string targetHealthText;

	private string lastTargetText = "not null";

	private string lastTargetHealthText = "";

	private bool targetTextDirty;

	private int offset;

	private int numPerPage;

	private List<AbilityDescription> abilities = new List<AbilityDescription>();

	private List<AbilityDescription> lastAbilities = new List<AbilityDescription>();

	private bool abilitiesDirty;

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public override void Init()
	{
		base.Init();
		XRLCore.RegisterAfterRenderCallback(AfterRender);
	}

	private void AfterRender(XRLCore core, ScreenBuffer sb)
	{
		XRL.World.GameObject body = core.Game.Player.Body;
		if (body == null)
		{
			return;
		}
		string text = null;
		lock (SB)
		{
			SB.Clear();
			SB.Append("{{Y|ACTIVE EFFECTS:}} ");
			bool flag = true;
			foreach (Effect effect in body.Effects)
			{
				string description = effect.GetDescription();
				if (!string.IsNullOrEmpty(description))
				{
					if (!flag)
					{
						SB.Append(", ");
					}
					else
					{
						flag = false;
					}
					SB.Append(Markup.Wrap(description));
				}
			}
			text = SB.ToString();
			SB.Clear();
		}
		if (lastEffectText != text)
		{
			lock (effectLock)
			{
				effectText = RTF.FormatToRTF(text);
				lastEffectText = text;
				effectTextDirty = true;
			}
		}
		ActivatedAbilities activatedAbilities = body.ActivatedAbilities;
		List<AbilityDescription> list;
		if (activatedAbilities != null && activatedAbilities.AbilityByGuid != null)
		{
			list = new List<AbilityDescription>(activatedAbilities.AbilityByGuid.Count);
			foreach (ActivatedAbilityEntry value2 in activatedAbilities.AbilityByGuid.Values)
			{
				if (!AbilityManager.commandToKey.TryGetValue(value2.Command ?? "<null command>", out var value))
				{
					value = int.MaxValue;
				}
				list.Add(new AbilityDescription
				{
					KeyCode = value,
					Entry = new ActivatedAbilityEntry(value2)
				});
			}
		}
		else
		{
			list = new List<AbilityDescription>(0);
		}
		if (list.Count != lastAbilities.Count || !list.TrueForAll(lastAbilities.Contains))
		{
			lock (abilities)
			{
				lastAbilities.Clear();
				lastAbilities.AddRange(list);
				abilities.Clear();
				abilities.AddRange(list);
				abilities.Sort(new AbilityDescriptionSort());
				abilitiesDirty = true;
			}
		}
		XRL.World.GameObject currentTarget = Sidebar.CurrentTarget;
		if (currentTarget != null)
		{
			string text2 = null;
			string text3 = null;
			lock (SB)
			{
				SB.Clear().Append("{{C|TARGET: ").Append(currentTarget.DisplayName)
					.Append("}}");
				text3 = SB.ToString();
				Description description2 = currentTarget.GetPart("Description") as Description;
				SB.Clear().Append(Strings.WoundLevel(currentTarget));
				if (description2 != null)
				{
					if (!string.IsNullOrEmpty(description2.GetFeelingDescription()))
					{
						SB.Append(", ").Append(Markup.Wrap(description2.GetFeelingDescription()));
					}
					if (!string.IsNullOrEmpty(description2.GetDifficultyDescription()))
					{
						SB.Append(", ").Append(Markup.Wrap(description2.GetDifficultyDescription()));
					}
				}
				text2 = SB.ToString();
			}
			if (lastTargetText != text3 || lastTargetHealthText != text2)
			{
				lock (targetLock)
				{
					lastTargetText = text3;
					lastTargetHealthText = text2;
					targetText = RTF.FormatToRTF(text3);
					targetHealthText = RTF.FormatToRTF(text2);
					targetTextDirty = true;
					return;
				}
			}
			return;
		}
		string text4 = "{{C|TARGET:}} {{K|[none]}}";
		if (lastTargetText != text4)
		{
			lock (targetLock)
			{
				lastTargetText = text4;
				lastTargetHealthText = "";
				targetText = Sidebar.FormatToRTF(lastTargetText);
				targetHealthText = "";
				targetTextDirty = true;
			}
		}
	}

	public void OnEffectsClick()
	{
		Debug.Log("Click effects");
	}

	public void Update()
	{
		if (!base.canvas.enabled)
		{
			return;
		}
		if (!CapabilityManager.AllowKeyboardHotkeys)
		{
			if (AbilityCommandText.text != "ABILITIES:")
			{
				AbilityCommandText.text = "ABILITIES:";
			}
			if (CycleCommandText.text != "")
			{
				CycleCommandText.text = "";
			}
		}
		if (effectTextDirty)
		{
			lock (effectLock)
			{
				if (EffectText.text != effectText)
				{
					EffectText.SetText(effectText);
				}
				effectTextDirty = false;
			}
		}
		if (targetTextDirty)
		{
			lock (targetLock)
			{
				if (TargetArea.gameObject.activeSelf != (targetText != null))
				{
					TargetArea.gameObject.SetActive(targetText != null);
				}
				if (TargetText.text != targetText)
				{
					TargetText.SetText(targetText);
				}
				if (TargetHealthText.text != targetHealthText)
				{
					TargetHealthText.SetText(targetHealthText);
				}
				targetTextDirty = false;
			}
		}
		if (abilitiesDirty)
		{
			lock (abilities)
			{
				if (AbilityButtons.Count != abilities.Count)
				{
					foreach (Transform item in ButtonArea.transform)
					{
						if (!AbilityButtons.Contains(item.gameObject))
						{
							UnityEngine.Object.Destroy(item.gameObject);
						}
					}
					while (AbilityButtons.Count > abilities.Count)
					{
						AbilityButtons[AbilityButtons.Count - 1].Destroy();
						AbilityButtons.RemoveAt(AbilityButtons.Count - 1);
					}
					while (AbilityButtons.Count < abilities.Count)
					{
						UnityEngine.GameObject gameObject = ButtonPrefab.Instantiate();
						gameObject.transform.SetParent(ButtonArea.transform, worldPositionStays: false);
						AbilityButtons.Add(gameObject);
					}
				}
				for (int i = 0; i < abilities.Count; i++)
				{
					AbilityBarButton component = AbilityButtons[i].GetComponent<AbilityBarButton>();
					string text = (abilities[i].Entry.Enabled ? "&C" : "&c") + abilities[i].Entry.DisplayName;
					if (!abilities[i].Entry.Enabled)
					{
						text += " {{K|[disabled]}}";
					}
					else if (abilities[i].Entry.Cooldown > 0)
					{
						text = text + " {{C|[" + Math.Ceiling((float)abilities[i].Entry.Cooldown / 10f) + "]}}";
					}
					if (abilities[i].Entry.Toggleable)
					{
						text = ((!abilities[i].Entry.ToggleState) ? (text + " {{K|[{{y|off}}]}}") : (text + " {{K|[{{g|on}}]}}"));
					}
					if (CapabilityManager.AllowKeyboardHotkeys && abilities[i].KeyCode != int.MaxValue)
					{
						text = text + " {{Y|<{{w|" + Keyboard.MetaToString(abilities[i].KeyCode) + "}}>}}";
					}
					component.disabled = !abilities[i].Entry.Enabled || abilities[i].Entry.Cooldown > 0;
					component.command = abilities[i].Entry.Command;
					component.Text.text = RTF.FormatToRTF(text);
				}
				abilitiesDirty = false;
			}
		}
		if (ButtonArea != null && Math.Floor(ButtonArea.rect.width / 175f) != (double)numPerPage)
		{
			if (numPerPage != 0)
			{
				offset /= numPerPage;
			}
			numPerPage = Math.Max(1, (int)ButtonArea.rect.width / 175);
			offset *= numPerPage;
			while (offset > AbilityButtons.Count)
			{
				offset -= numPerPage;
			}
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.Tab) && (Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl)))
		{
			offset += numPerPage;
			if (offset >= AbilityButtons.Count)
			{
				offset = 0;
			}
		}
		PageText.text = ((numPerPage == 0) ? "0" : (offset / numPerPage + 1).ToString());
		PagerArea.SetActive(AbilityButtons.Count == 0 || AbilityButtons.Count > numPerPage);
		for (int j = 0; j < AbilityButtons.Count; j++)
		{
			bool flag = j >= offset && j < offset + numPerPage;
			if (AbilityButtons[j].activeSelf != flag)
			{
				AbilityButtons[j].SetActive(flag);
			}
		}
	}

	public void MovePage(int direction)
	{
		offset += direction * numPerPage;
		if (offset >= AbilityButtons.Count)
		{
			offset = 0;
		}
		if (offset < 0)
		{
			offset = Math.Max(0, (int)Math.Floor(((float)AbilityButtons.Count - 1f) / (float)numPerPage) * numPerPage);
		}
		abilitiesDirty = true;
	}
}
