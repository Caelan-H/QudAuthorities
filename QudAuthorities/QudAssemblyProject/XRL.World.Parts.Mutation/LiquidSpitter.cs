using System;
using System.Collections.Generic;
using System.Linq;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LiquidSpitter : BaseMutation
{
	public static readonly Dictionary<string, string> LiquidAnimationColors = new Dictionary<string, string>
	{
		{ "lava", "&W" },
		{ "acid", "&G" },
		{ "honey", "&w" },
		{ "slime", "&g" },
		{ "water", "&B" },
		{ "salt", "&Y" },
		{ "base", "&B" },
		{ "wine", "&m" },
		{ "asphalt", "&K" },
		{ "oil", "&K" },
		{ "blood", "&r" },
		{ "sludge", "&w" },
		{ "goo", "&G" },
		{ "putrid", "&g" },
		{ "gel", "&Y" },
		{ "ooze", "&K" },
		{ "cider", "&w" },
		{ "convalessence", "&C" },
		{ "neutronflux", "&y" },
		{ "cloning", "&Y" },
		{ "proteangunk", "&c" },
		{ "wax", "&Y" },
		{ "ink", "&K" },
		{ "sap", "&W" },
		{ "brainbrine", "&g" },
		{ "algae", "&g" },
		{ "sunslag", "&Y" }
	};

	public static readonly string[] Exclude = new string[1] { "neutronflux" };

	public static readonly string[] Dilute = new string[3] { "cloning", "brainbrine", "sunslag" };

	public const string ABL_CMD = "CommandSpitLiquid";

	public new Guid ActivatedAbilityID = Guid.Empty;

	[Obsolete("save compat")]
	public string Placeholder = "";

	public List<string> Liquids = new List<string>();

	private string _LiquidName;

	public string LiquidName
	{
		get
		{
			if (_LiquidName == null)
			{
				LiquidVolume liquidVolume = CreatePool().LiquidVolume;
				_LiquidName = liquidVolume.GetLiquidName();
			}
			return _LiquidName;
		}
	}

	public string GetAnimationColor
	{
		get
		{
			if (Liquids.Count > 0 && LiquidAnimationColors.TryGetValue(Liquids.GetRandomElementCosmetic(), out var value))
			{
				return value;
			}
			return "&y";
		}
	}

	public LiquidSpitter()
	{
		DisplayName = "Liquid Spitter";
		Type = "Physical";
	}

	public LiquidSpitter(string Liquid)
		: this()
	{
		AddLiquid(Liquid);
	}

	[Obsolete("save compat")]
	public override void Attach()
	{
		string[] exclude = Exclude;
		foreach (string item in exclude)
		{
			Liquids.Remove(item);
		}
		base.Attach();
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		LiquidSpitter obj = base.DeepCopy(Parent) as LiquidSpitter;
		obj.Liquids = new List<string>(Liquids);
		return obj;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID)
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command != "CommandSpitLiquid")
		{
			return base.HandleEvent(E);
		}
		if (Liquids.Count == 0)
		{
			MetricsManager.LogError($"No liquids to spit (Creature: {ParentObject}, Old: {!Placeholder.IsNullOrEmpty()})");
			return ParentObject.ShowFailure("You lack a liquid to spit!");
		}
		List<Cell> list = PickBurst(1, 8, bLocked: false, AllowVis.OnlyVisible);
		if (list.IsNullOrEmpty())
		{
			return false;
		}
		if (list.Any((Cell C) => C.DistanceTo(ParentObject) > 9))
		{
			return ParentObject.ShowFailure("That is out of range! (8 squares)");
		}
		SlimeGlands.SlimeAnimation(GetAnimationColor, ParentObject.CurrentCell, list[0]);
		CooldownMyActivatedAbility(ActivatedAbilityID, 40);
		UseEnergy(1000, "Physical Mutation Spit Liquid");
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (80.in100() || i == 0)
			{
				list[i].AddObject(CreatePool());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (Liquids.Contains("salt"))
		{
			E.Add("salt", 1);
		}
		else if (Liquids.Contains("convalessence"))
		{
			E.Add("ice", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You spit a puddle of " + LiquidName + ".";
	}

	public override string GetLevelText(int Level)
	{
		return "Cooldown: 10 rounds\nRange: 8\nArea: 3x3";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (intParameter <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasLOSTo(gameObjectParameter, IncludeSolid: true, UseTargetability: true))
			{
				E.AddAICommand("CommandSpitLiquid");
			}
		}
		return true;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spit Liquid", "CommandSpitLiquid", "Physical Mutation", null, "­");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public GameObject CreatePool()
	{
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("BasePool");
		LiquidVolume liquidVolume = gameObject.LiquidVolume;
		liquidVolume.ComponentLiquids.Clear();
		foreach (string liquid in Liquids)
		{
			liquidVolume.ComponentLiquids[liquid] = liquidVolume.StartVolume.RollCached();
		}
		liquidVolume.Update();
		if (liquidVolume.Primary.IsNullOrEmpty())
		{
			MetricsManager.LogError("Spitting liquid without primary " + string.Format("(Liquids: {0}, Creature: {1}, Volume: {2}, ", string.Join(", ", Liquids), ParentObject, liquidVolume.Volume) + string.Format("Old: {0}), Components: {1})", !Placeholder.IsNullOrEmpty(), string.Join(", ", liquidVolume.ComponentLiquids)));
		}
		return gameObject;
	}

	public void AddLiquid(string ID)
	{
		if (!Liquids.Contains(ID) && !Exclude.Contains(ID))
		{
			Liquids.Add(ID);
			_LiquidName = null;
			if (Liquids.Count == 1 && Dilute.Contains(ID))
			{
				Liquids.Add("water");
			}
		}
	}
}
