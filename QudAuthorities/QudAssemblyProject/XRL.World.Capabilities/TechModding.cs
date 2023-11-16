using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World.Capabilities;

public static class TechModding
{
	[NonSerialized]
	private static Dictionary<string, IModification> ModificationInstances = new Dictionary<string, IModification>(64);

	private static Type[] modDescArgs2 = new Type[2]
	{
		typeof(int),
		typeof(GameObject)
	};

	private static Type[] modDescArgs1 = new Type[1] { typeof(int) };

	public static bool CanMod(GameObject obj)
	{
		if (The.Player == null || !The.Player.HasSkill("Tinkering"))
		{
			return false;
		}
		string text = ModKey(obj);
		if (text != null && obj.Understood())
		{
			foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
			{
				if (knownRecipe.Type == "Mod" && ModAppropriate(obj, knownRecipe, text))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string ModKey(GameObject obj)
	{
		string propertyOrTag = obj.GetPropertyOrTag("Mods");
		if (!string.IsNullOrEmpty(propertyOrTag) && propertyOrTag != "None" && obj.GetIntProperty("nMods") < 3)
		{
			return propertyOrTag;
		}
		return null;
	}

	public static bool ModAppropriate(GameObject obj, TinkerData mod, string key)
	{
		if (key != null && mod.CanMod(key) && ModificationApplicable(mod.PartName, obj))
		{
			return true;
		}
		return false;
	}

	public static bool ModAppropriate(GameObject obj, TinkerData mod)
	{
		return ModAppropriate(obj, mod, ModKey(obj));
	}

	public static bool ModificationApplicable(string Name, GameObject obj, GameObject actor = null, string key = null)
	{
		if ((key ?? ModKey(obj)) == null)
		{
			return false;
		}
		if (obj.HasPart(Name))
		{
			return false;
		}
		if (actor != null && actor.IsPlayerControlled() && !obj.Understood())
		{
			return false;
		}
		if (!CanBeModdedEvent.Check(actor, obj, Name))
		{
			return false;
		}
		if (!ModificationInstances.TryGetValue(Name, out var value))
		{
			value = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts." + Name)) as IModification;
			if (value == null)
			{
				throw new Exception(Name + " is not a modification part");
			}
			ModificationInstances.Add(Name, value);
		}
		return value.ModificationApplicable(obj);
	}

	public static string GetModificationDescription(string Name, int Tier)
	{
		if (Name.Contains("[mod]"))
		{
			Name = Name.Replace("[mod]", "");
		}
		MethodInfo method = ModManager.ResolveType("XRL.World.Parts." + Name).GetMethod("GetDescription", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, modDescArgs1, null);
		if (method != null)
		{
			return (string)method.Invoke(null, new object[1] { Tier });
		}
		return ModificationFactory.ModsByPart[Name].Description;
	}

	public static string GetModificationDescription(string Name, GameObject obj)
	{
		if (obj == null)
		{
			return GetModificationDescription(Name, 1);
		}
		if (Name.Contains("[mod]"))
		{
			Name = Name.Replace("[mod]", "");
		}
		Type type = ModManager.ResolveType("XRL.World.Parts." + Name);
		MethodInfo method = type.GetMethod("GetDescription", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, modDescArgs2, null);
		if (method != null)
		{
			return (string)method.Invoke(null, new object[2]
			{
				Math.Max(obj.GetTier(), 1),
				obj
			});
		}
		MethodInfo method2 = type.GetMethod("GetDescription", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, modDescArgs1, null);
		if (method2 != null)
		{
			return (string)method2.Invoke(null, new object[1] { Math.Max(obj.GetTier(), 1) });
		}
		return ModificationFactory.ModsByPart[Name].Description;
	}

	public static bool ApplyModification(GameObject GO, IModification ModPart, bool DoRegistration = true, GameObject actor = null, bool Creation = false)
	{
		if (actor != null && !ModPart.BeingAppliedBy(GO, actor))
		{
			return false;
		}
		GO.AddPart(ModPart, DoRegistration, Creation);
		GO.ModIntProperty("nMods", 1);
		if (GO.HasPart("Commerce") && ModificationFactory.ModsByPart.ContainsKey(ModPart.Name))
		{
			GO.GetPart<Commerce>().Value *= ModificationFactory.ModsByPart[ModPart.Name].Value;
		}
		ModPart.ApplyModification(GO);
		ModificationAppliedEvent.Send(GO, ModPart);
		return true;
	}

	public static bool ApplyModification(GameObject GO, string ModPartName, out IModification ModPart, int Tier, bool DoRegistration = true, GameObject actor = null, bool Creation = false)
	{
		Type type = ModManager.ResolveType("XRL.World.Parts." + ModPartName);
		if (type == null)
		{
			MetricsManager.LogError("ApplyModification", "Couldn't resolve unknown mod part: " + ModPartName);
			ModPart = null;
			return false;
		}
		Tier = XRL.World.Capabilities.Tier.Constrain(Tier);
		object[] args = new object[1] { Tier };
		ModPart = Activator.CreateInstance(type, args) as IModification;
		if (ModPart == null)
		{
			if (!(Activator.CreateInstance(type, args) is IPart))
			{
				throw new Exception("failed to load " + type);
			}
			throw new Exception(type?.ToString() + " is not an IModification");
		}
		return ApplyModification(GO, ModPart, DoRegistration, actor, Creation);
	}

	public static bool ApplyModification(GameObject GO, string ModPartName, int Tier, bool DoRegistration = true, GameObject actor = null, bool Creation = false)
	{
		IModification ModPart;
		return ApplyModification(GO, ModPartName, out ModPart, Tier, DoRegistration, actor, Creation);
	}

	public static bool ApplyModification(GameObject GO, string ModPartName, out IModification ModPart, bool DoRegistration = true, GameObject actor = null, bool Creation = false)
	{
		return ApplyModification(GO, ModPartName, out ModPart, GO.GetTier(), DoRegistration, actor, Creation);
	}

	public static bool ApplyModification(GameObject GO, string ModPartName, bool DoRegistration = true, GameObject actor = null, bool Creation = false)
	{
		IModification ModPart;
		return ApplyModification(GO, ModPartName, out ModPart, DoRegistration, actor, Creation);
	}

	public static bool ApplyModificationFromPopulationTable(GameObject GO, string Table, int Tier, bool Creation = false)
	{
		string blueprint = PopulationManager.RollOneFrom(Table).Blueprint;
		return ApplyModification(GO, blueprint, Tier, Creation);
	}

	public static bool ApplyModificationFromPopulationTable(GameObject GO, string Table, bool Creation = false)
	{
		string blueprint = PopulationManager.RollOneFrom(Table).Blueprint;
		return ApplyModification(GO, blueprint, Creation);
	}
}
