using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_DeployTurret : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public static void Init()
	{
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandDeployTurret");
		base.Register(Object);
	}

	private bool UsableForTurret(GameObject obj)
	{
		if (obj == ParentObject)
		{
			return false;
		}
		if (!(obj.GetPart("MissileWeapon") is MissileWeapon missileWeapon))
		{
			return false;
		}
		if (!missileWeapon.FiresManually)
		{
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandDeployTurret" && ParentObject.CheckFrozen())
		{
			List<GameObject> objects = ParentObject.Inventory.GetObjects(UsableForTurret);
			string title = "[Select a weapon to deploy on the turret]\n\n";
			if (objects.Count == 0)
			{
				Popup.Show("You have no missile weapons to deploy.");
			}
			else
			{
				GameObject gameObject = Popup.PickGameObject(title, objects);
				if (gameObject != null)
				{
					string text = XRL.UI.PickDirection.ShowPicker();
					if (text != null)
					{
						Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(text);
						if (cellFromDirection != null)
						{
							if (!cellFromDirection.IsEmpty() || cellFromDirection.HasObjectWithTag("ExcavatoryTerrainFeature"))
							{
								Popup.Show("You can't deploy there!");
							}
							else
							{
								gameObject = gameObject.RemoveOne();
								GameObject gameObject2 = IntegratedWeaponHosts.GenerateTurret(gameObject, ParentObject);
								cellFromDirection.AddObject(gameObject2);
								gameObject2.MakeActive();
								DidXToY("deploy", gameObject2, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
								ParentObject.UseEnergy(10000, "Skill Tinkering Deploy Turret");
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Deploy Turret", "CommandDeployTurret", "Tinkering", null, "\u009d");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
