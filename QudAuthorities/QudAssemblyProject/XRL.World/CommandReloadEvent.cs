using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CommandReloadEvent : MinEvent
{
	public const int PASSES = 3;

	public GameObject Actor;

	public GameObject Weapon;

	public GameObject LastAmmo;

	public List<IComponent<GameObject>> CheckedForReload = new List<IComponent<GameObject>>();

	public List<IComponent<GameObject>> NeededReload = new List<IComponent<GameObject>>();

	public List<IComponent<GameObject>> TriedToReload = new List<IComponent<GameObject>>();

	public List<IComponent<GameObject>> Reloaded = new List<IComponent<GameObject>>();

	public List<GameObject> ObjectsReloaded = new List<GameObject>();

	public bool FreeAction;

	public bool FromDialog;

	public int MinimumCharge;

	public int MaxEnergyCost;

	public int TotalEnergyCost;

	public int Pass;

	public new static readonly int ID;

	public static CommandReloadEvent instance;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CommandReloadEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public CommandReloadEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Weapon = null;
		LastAmmo = null;
		CheckedForReload.Clear();
		NeededReload.Clear();
		TriedToReload.Clear();
		Reloaded.Clear();
		ObjectsReloaded.Clear();
		FreeAction = false;
		FromDialog = false;
		MinimumCharge = 0;
		MaxEnergyCost = 0;
		TotalEnergyCost = 0;
		Pass = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public void EnergyCost(int amount)
	{
		TotalEnergyCost += amount;
		if (amount > MaxEnergyCost)
		{
			MaxEnergyCost = amount;
		}
	}

	public static bool Execute(GameObject Actor, bool FreeAction = false, bool FromDialog = false, int MinimumCharge = 0)
	{
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			if (!Actor.CanMoveExtremities("Reload", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (instance == null)
			{
				instance = new CommandReloadEvent();
			}
			instance.Reset();
			instance.Actor = Actor;
			instance.Weapon = null;
			instance.FreeAction = FreeAction;
			instance.FromDialog = FromDialog;
			instance.MinimumCharge = MinimumCharge;
			for (int i = 1; i <= 3; i++)
			{
				instance.Pass = i;
				if (!Actor.HandleEvent(instance))
				{
					return false;
				}
			}
			if (!FreeAction)
			{
				Actor.UseEnergy(instance.MaxEnergyCost, "Reload");
			}
		}
		return true;
	}

	public static bool Execute(GameObject Actor, GameObject Weapon, GameObject LastAmmo = null, bool FreeAction = false, bool FromDialog = false, int MinimumCharge = 0)
	{
		if (Weapon.WantEvent(ID, CascadeLevel))
		{
			if (!Actor.CanMoveExtremities("Reload", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (instance == null)
			{
				instance = new CommandReloadEvent();
			}
			instance.Reset();
			instance.Actor = Actor;
			instance.Weapon = Weapon;
			instance.LastAmmo = LastAmmo;
			instance.FreeAction = FreeAction;
			instance.FromDialog = FromDialog;
			instance.MinimumCharge = MinimumCharge;
			for (int i = 1; i <= 3; i++)
			{
				instance.Pass = i;
				if (!Weapon.HandleEvent(instance))
				{
					return false;
				}
			}
			if (!FreeAction)
			{
				Actor.UseEnergy(instance.MaxEnergyCost, "Reload");
			}
		}
		return true;
	}
}
