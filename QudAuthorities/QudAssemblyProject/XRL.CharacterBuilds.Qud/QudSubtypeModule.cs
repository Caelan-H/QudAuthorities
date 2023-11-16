using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.CharacterBuilds.Qud.UI;
using XRL.CharacterCreation;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;

namespace XRL.CharacterBuilds.Qud;

public class QudSubtypeModule : EmbarkBuilderModule<QudSubtypeModuleData>
{
	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILE = "BootPlayerTile";

	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILEFOREGROUND = "BootPlayerTileForeground";

	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILEBACKGROUND = "BootPlayerTileBackground";

	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILEDETAIL = "BootPlayerTileDetail";

	public Dictionary<string, SubtypeEntry> genotypesByName => SubtypeFactory.SubtypesByName;

	public List<SubtypeEntry> subtypes => SubtypeFactory.Subtypes;

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudGenotypeModule>()?.data?.Genotype != null;
	}

	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public void SelectSubtype(string id)
	{
		setData(new QudSubtypeModuleData(id));
	}

	public override void handleModuleDataChange(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		if (module is QudGenotypeModule && base.data != null)
		{
			if (builder.GetModule<QudGenotypeModule>().data?.Genotype == null)
			{
				setData(null);
			}
			else if (!GetSelections().Any((ChoiceWithColorIcon s) => s.Id == base.data.Subtype))
			{
				setData(null);
			}
		}
		base.handleModuleDataChange(module, oldValues, newValues);
	}

	public override void InitFromSeed(string seed)
	{
		BallBag<SubtypeEntry> ballBag = new BallBag<SubtypeEntry>(getModuleRngFromSeed(seed));
		foreach (SubtypeEntry subtype in SubtypeFactory.Subtypes)
		{
			ballBag.Add(subtype, subtype.RandomWeight);
		}
		QudSubtypeModuleData qudSubtypeModuleData = new QudSubtypeModuleData();
		qudSubtypeModuleData.Subtype = ballBag.PeekOne().Name;
		base.data = qudSubtypeModuleData;
	}

	private static void AddItem(XRL.World.GameObject GO, XRL.World.GameObject player)
	{
		GO.Seen();
		GO.MakeUnderstood();
		GO.GetPart<EnergyCellSocket>()?.Cell?.MakeUnderstood();
		player.ReceiveObject(GO);
	}

	private static void AddItem(string Blueprint, XRL.World.GameObject player)
	{
		try
		{
			AddItem(XRL.World.GameObject.create(Blueprint), player);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception creating " + Blueprint, x);
		}
	}

	private static void AddItem(string Blueprint, int number, XRL.World.GameObject player)
	{
		try
		{
			for (int i = 0; i < number; i++)
			{
				AddItem(XRL.World.GameObject.create(Blueprint), player);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception creating " + Blueprint, x);
		}
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		SubtypeEntry subtypeEntry = base.data?.Entry;
		if (subtypeEntry == null)
		{
			return base.handleBootEvent(id, game, info, element);
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE)
		{
			return element ?? subtypeEntry.Tile;
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEDETAIL)
		{
			return element ?? subtypeEntry.DetailColor;
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
		{
			XRL.World.GameObject gameObject = (XRL.World.GameObject)element;
			gameObject.SetStringProperty("Subtype", base.data.Subtype);
			if (!string.IsNullOrEmpty(subtypeEntry.Gear))
			{
				string[] array = subtypeEntry.Gear.Split(',');
				foreach (string text in array)
				{
					if (PopulationManager.Populations.ContainsKey(text))
					{
						foreach (PopulationResult item in PopulationManager.Populations[text].Generate(new Dictionary<string, string>(), ""))
						{
							AddItem(item.Blueprint, item.Number, gameObject);
						}
					}
					else
					{
						Debug.LogError("Unknown gear population table: " + text);
					}
				}
			}
			List<string> list = info.getData<QudGenotypeModuleData>()?.Entry?.RemoveSkills;
			foreach (string skill in subtypeEntry.Skills)
			{
				if (!subtypeEntry.RemoveSkills.Contains(skill) && (list == null || !list.Contains(skill)))
				{
					gameObject.AddSkill(skill);
				}
			}
			if (subtypeEntry.CyberneticsLicensePoints != -999)
			{
				gameObject.ModIntProperty("CyberneticsLicenses", subtypeEntry.CyberneticsLicensePoints);
			}
			if (subtypeEntry.SaveModifiers.Any((SubtypeSaveModifier x) => x.Amount != 0 && x.Amount != -999))
			{
				SaveModifiers saveModifiers = gameObject.RequirePart<SaveModifiers>();
				saveModifiers.WorksOnSelf = true;
				saveModifiers.WorksOnEquipper = false;
				foreach (SubtypeSaveModifier saveModifier in subtypeEntry.SaveModifiers)
				{
					if (saveModifier.Amount != 0 && saveModifier.Amount != -999)
					{
						saveModifiers.AddModifier(saveModifier.Vs, saveModifier.Amount);
					}
				}
			}
			foreach (SubtypeReputation reputation in subtypeEntry.Reputations)
			{
				Faction.PlayerReputation.modify(reputation.With, reputation.Value, null, null, silent: true);
			}
		}
		else if (id == QudGameBootModule.BOOTEVENT_AFTERBOOTPLAYEROBJECT && !subtypeEntry.Class.IsNullOrEmpty())
		{
			XRL.World.GameObject body = (XRL.World.GameObject)element;
			string[] array = subtypeEntry.Class.Split(',');
			foreach (string text2 in array)
			{
				if (!text2.IsNullOrEmpty())
				{
					try
					{
						(ModManager.CreateInstance("XRL.CharacterCreation." + text2) as ICustomChargenClass).BuildCharacterBody(body);
					}
					catch (Exception ex)
					{
						Debug.LogError("Exception executing builder: " + text2 + " -> " + ex);
					}
				}
			}
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public override object handleUIEvent(string ID, object Element)
	{
		SubtypeEntry subtypeEntry = base.data?.Entry;
		if (subtypeEntry == null)
		{
			return base.handleUIEvent(ID, Element);
		}
		if (ID == QudAttributesModuleWindow.EID_BEFORE_GET_BASE_ATTRIBUTES)
		{
			List<AttributeDataElement> list = Element as List<AttributeDataElement>;
			foreach (KeyValuePair<string, SubtypeStat> stat2 in subtypeEntry.Stats)
			{
				AttributeDataElement attributeDataElement = list?.FirstOrDefault((AttributeDataElement a) => a.Attribute == stat2.Value.Name);
				if (attributeDataElement != null)
				{
					if (stat2.Value.Minimum != -999)
					{
						attributeDataElement.BaseValue = stat2.Value.Minimum;
					}
					if (stat2.Value.Minimum != -999)
					{
						attributeDataElement.Minimum = stat2.Value.Minimum;
					}
					if (stat2.Value.Maximum != -999)
					{
						attributeDataElement.Maximum = stat2.Value.Maximum;
					}
				}
			}
		}
		else if (ID == QudAttributesModuleWindow.EID_GET_BASE_ATTRIBUTES)
		{
			List<AttributeDataElement> source = Element as List<AttributeDataElement>;
			foreach (KeyValuePair<string, SubtypeStat> stat in subtypeEntry.Stats)
			{
				AttributeDataElement attributeDataElement2 = source.FirstOrDefault((AttributeDataElement a) => a.Attribute == stat.Value.Name);
				if (attributeDataElement2 != null && stat.Value.Bonus != -999)
				{
					attributeDataElement2.Bonus += stat.Value.Bonus;
				}
			}
		}
		return base.handleUIEvent(ID, Element);
	}

	public string getSelected()
	{
		return base.data?.Subtype;
	}

	public bool hasCategories()
	{
		if (TryGetSubtypeClass(out var Subtypes))
		{
			return Subtypes.Categories.Count > 1;
		}
		return false;
	}

	public IEnumerable<SubtypeEntry> getAvailableSelections()
	{
		if (TryGetSubtypeClass(out var Subtypes))
		{
			return Subtypes.Categories.SelectMany((SubtypeCategory c) => c.Subtypes);
		}
		return new SubtypeEntry[0];
	}

	public ChoiceWithColorIcon choiceFromSubtype(SubtypeEntry subtype)
	{
		return new ChoiceWithColorIcon
		{
			Id = subtype.Name,
			Title = subtype.DisplayName,
			IconPath = subtype.Tile,
			IconDetailColor = ConsoleLib.Console.ColorUtility.ColorFromString(subtype.DetailColor),
			IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y'],
			Description = subtype.GetFlatChargenInfo(),
			Chosen = IsChoiceSelected
		};
	}

	public bool TryGetSubtypeClass(out SubtypeClass Subtypes)
	{
		return SubtypeFactory.TryGetSubtypeClass(builder.GetModule<QudGenotypeModule>()?.data?.Entry, out Subtypes);
	}

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		foreach (SubtypeEntry availableSelection in getAvailableSelections())
		{
			yield return choiceFromSubtype(availableSelection);
		}
	}

	public IEnumerable<CategoryIcons> GetSelectionCategories()
	{
		if (!TryGetSubtypeClass(out var Subtypes))
		{
			yield break;
		}
		foreach (SubtypeCategory category in Subtypes.Categories)
		{
			if (category.Subtypes.Count > 0)
			{
				yield return new CategoryIcons
				{
					Title = category.DisplayName,
					Choices = category.Subtypes.Select(choiceFromSubtype).ToList()
				};
			}
		}
	}

	public bool IsChoiceSelected(ChoiceWithColorIcon choice)
	{
		return getSelected() == choice?.Id;
	}
}
