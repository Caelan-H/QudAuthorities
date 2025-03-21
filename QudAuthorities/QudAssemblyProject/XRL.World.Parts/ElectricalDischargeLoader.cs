using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, the adjustment to charge used that is
///             implemented by IActivePart (adjusting by power load as a percentage)
///             applies to the charge-based damage/voltage calculations here,
///             multiplied by OverloadFactor; so, for example, if base charge draw
///             is 100, overloaded draw is 400, and OverloadFactor is the default
///             of 1/6, effective draw for purposes of damage and voltage calculation
///             is 100 + ((400 - 100) * (1 / 6)) = 150.
///             </remarks>
[Serializable]
public class ElectricalDischargeLoader : IPoweredPart
{
	public string ProjectileObject;

	public float ChargeFactor = 15f;

	[FieldSaveVersion(227)]
	public float OverloadFactor = 1f / 6f;

	[NonSerialized]
	private static List<GameObjectBlueprint> EnergyCellBlueprints = null;

	[NonSerialized]
	private static Dictionary<string, int> BlueprintMaxCharge = new Dictionary<string, int>(16);

	public ElectricalDischargeLoader()
	{
		ChargeUse = 300;
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
		NameForStatus = "ElectricalDischargeSystem";
	}

	public override bool SameAs(IPart p)
	{
		ElectricalDischargeLoader electricalDischargeLoader = p as ElectricalDischargeLoader;
		if (electricalDischargeLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (electricalDischargeLoader.ChargeFactor != ChargeFactor)
		{
			return false;
		}
		if (electricalDischargeLoader.OverloadFactor != OverloadFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandReloadEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetMissileWeaponProjectileEvent.ID && ID != GetProjectileBlueprintEvent.ID)
		{
			return ID == NeedsReloadEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Blueprint = ProjectileObject;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedsReloadEvent E)
	{
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject) && GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered && ParentObject.IsEquippedProperly())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if (E.Pass >= 3 && (E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			if (!ParentObject.IsEquippedProperly())
			{
				return true;
			}
			bool flag = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered;
			if (flag || (E.NeededReload.Count <= 0 && E.Reloaded.Count <= 0 && !NeedsReloadEvent.Check(E.Actor, this)))
			{
				if (flag)
				{
					E.NeededReload.Add(this);
				}
				if (ParentObject.WantEvent(InventoryActionEvent.ID, MinEvent.CascadeLevel))
				{
					E.TriedToReload.Add(this);
					if (InventoryActionEvent.Check(ParentObject, E.Actor, ParentObject, "ReplaceSocketCell", Auto: false, OwnershipHandled: false, OverrideEnergyCost: true, 0, E.MinimumCharge))
					{
						E.Reloaded.Add(this);
						if (!E.ObjectsReloaded.Contains(ParentObject))
						{
							E.ObjectsReloaded.Add(ParentObject);
						}
						E.EnergyCost(1000);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Blueprint = ProjectileObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject) && E.Understood())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("{{W|").Append('\u0003').Append("}}")
				.Append(GetDamageRoll());
			E.AddTag(stringBuilder.ToString(), -20);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIWantUseWeapon");
		Object.RegisterPartEvent(this, "CheckLoadAmmo");
		Object.RegisterPartEvent(this, "CheckReadyToFire");
		Object.RegisterPartEvent(this, "GenerateIntegratedHostInitialAmmo");
		Object.RegisterPartEvent(this, "GetMissileWeaponStatus");
		Object.RegisterPartEvent(this, "GetNotReadyToFireMessage");
		Object.RegisterPartEvent(this, "LoadAmmo");
		Object.RegisterPartEvent(this, "PrepIntegratedHostToReceiveAmmo");
		Object.RegisterPartEvent(this, "SupplyIntegratedHostWithAmmo");
		base.Register(Object);
	}

	public string GetUnpoweredMessage()
	{
		return ParentObject.The + ParentObject.ShortDisplayName + " merely " + ParentObject.GetVerb("click", PrependSpace: false) + ".";
	}

	public override int GetDraw(QueryDrawEvent E = null)
	{
		int num = base.GetDraw(E);
		if (ParentObject.GetPart("MissileWeapon") is MissileWeapon missileWeapon)
		{
			num *= missileWeapon.AmmoPerAction;
		}
		return num;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckReadyToFire")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return false;
			}
		}
		else if (E.ID == "GetNotReadyToFireMessage")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.SetParameter("Message", GetUnpoweredMessage());
			}
		}
		else if (E.ID == "AIWantUseWeapon")
		{
			if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered)
			{
				EnergyCellSocket pSocket = ParentObject.GetPart<EnergyCellSocket>();
				if (pSocket != null)
				{
					GameObject User = E.GetGameObjectParameter("Object");
					bool Result = false;
					User.Inventory.ForeachObject(delegate(GameObject GO)
					{
						if (!User.IsPlayer() || GO.Understood())
						{
							GO.ForeachPartDescendedFrom(delegate(IEnergyCell P)
							{
								if (P.SlotType == pSocket.SlotType && P.HasCharge(ChargeUse))
								{
									Result = true;
									return false;
								}
								return true;
							});
						}
					});
					if (!Result)
					{
						return false;
					}
				}
			}
		}
		else if (E.ID == "GetMissileWeaponStatus")
		{
			if (!E.HasParameter("Override"))
			{
				StringBuilder Items = E.GetParameter("Items") as StringBuilder;
				EnergyCellSocket part = ParentObject.GetPart<EnergyCellSocket>();
				if (part != null)
				{
					if (part.Cell == null)
					{
						Items.Append(" [{{K|empty}}]");
					}
					else if (!part.Cell.Understood())
					{
						Items.Append(" [?]");
					}
					else
					{
						part.Cell.ForeachPartDescendedFrom(delegate(IEnergyCell P)
						{
							string text2 = P.ChargeStatus();
							if (text2 != null)
							{
								Items.Append(" [").Append(text2).Append("]");
							}
						});
					}
				}
			}
		}
		else if (E.ID == "CheckLoadAmmo")
		{
			if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered)
			{
				if (E.GetGameObjectParameter("Loader").IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(GetUnpoweredMessage(), 'r');
				}
				return false;
			}
		}
		else if (E.ID == "LoadAmmo")
		{
			int value = MyPowerLoadLevel();
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, value))
			{
				if (E.GetGameObjectParameter("Loader").IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(GetUnpoweredMessage(), 'r');
				}
				E.SetParameter("Ammo", null);
				return false;
			}
			if (ProjectileObject != null)
			{
				int activeChargeUse = GetActiveChargeUse(null, value);
				GameObject gameObject = GameObject.create(ProjectileObject);
				DischargeOnHit dischargeOnHit = gameObject.RequirePart<DischargeOnHit>();
				dischargeOnHit.Voltage = GetVoltage(activeChargeUse, value).ToString();
				dischargeOnHit.Damage = GetDamageRoll(activeChargeUse, value).ToString();
				E.SetParameter("Ammo", gameObject);
			}
		}
		else if (E.ID == "PrepIntegratedHostToReceiveAmmo")
		{
			if (ChargeUse > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Host");
				if (gameObjectParameter != null && gameObjectParameter.HasPart("Robot"))
				{
					if (!gameObjectParameter.HasPart("ElectricalPowerTransmission"))
					{
						ElectricalPowerTransmission electricalPowerTransmission = new ElectricalPowerTransmission();
						electricalPowerTransmission.ChargeRate = ((GetChargePerAction() >= 1000) ? 10000 : 1000);
						electricalPowerTransmission.IsConsumer = true;
						gameObjectParameter.AddPart(electricalPowerTransmission);
					}
					if (!gameObjectParameter.HasPart("Capacitor") && !gameObjectParameter.HasTagOrProperty("NoIntegratedHostCapacitor"))
					{
						Capacitor capacitor = new Capacitor();
						capacitor.MaxCharge = ChargeUse * 10;
						capacitor.ChargeRate = ChargeUse;
						capacitor.MinimumChargeToExplode = 0;
						capacitor.ChargeDisplayStyle = null;
						gameObjectParameter.AddPart(capacitor);
					}
				}
				if (!ParentObject.HasPart("IntegratedPowerSystems") && TechModding.ModificationApplicable("ModJacked", ParentObject))
				{
					TechModding.ApplyModification(ParentObject, "ModJacked");
				}
			}
		}
		else if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (ChargeUse > 0)
			{
				if (E.HasFlag("TrackSupply"))
				{
					E.SetFlag("AnySupplyHandler", State: true);
				}
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Host");
				GameObject gameObject2 = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
				if (gameObjectParameter2 != null && gameObject2 != null)
				{
					Inventory inventory = gameObject2.Inventory;
					if (inventory != null)
					{
						if (gameObject2.IsPlayer())
						{
							int num = 0;
							List<GameObject> skip = Event.NewGameObjectList();
							while (true)
							{
								string SlotType = null;
								EnergyCellSocket part2 = ParentObject.GetPart<EnergyCellSocket>();
								if (part2 != null)
								{
									SlotType = part2.SlotType;
								}
								List<string> OptionStrings = new List<string>(16);
								List<object> options = new List<object>(16);
								List<char> keymap = new List<char>(16);
								OptionStrings.Add("none");
								options.Add(null);
								keymap.Add('-');
								char c = 'a';
								inventory.ForeachObject(delegate(GameObject GO)
								{
									if (!skip.Contains(GO) && GO.Understood())
									{
										GO.ForeachPartDescendedFrom(delegate(IEnergyCell P)
										{
											if (SlotType == null || P.SlotType == SlotType)
											{
												OptionStrings.Add(GO.DisplayName);
												options.Add(GO);
												keymap.Add(c);
												char c2 = c;
												c = (char)(c2 + 1);
												return false;
											}
											return true;
										});
									}
								});
								if (options.Count <= 1)
								{
									break;
								}
								if (E.HasFlag("TrackSupply"))
								{
									E.SetFlag("AnySupplies", State: true);
								}
								int num2 = Popup.ShowOptionList("Select " + ((num == 0) ? "one" : "another") + " of your cells to supply " + gameObjectParameter2.the + gameObjectParameter2.ShortDisplayName + " with, if desired.", OptionStrings.ToArray(), keymap.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
								if (num2 < 0 || !(options[num2] is GameObject gameObject3))
								{
									break;
								}
								GameObject gameObject4 = gameObject3.RemoveOne();
								if (gameObjectParameter2.ReceiveObject(gameObject4))
								{
									IComponent<GameObject>.WDidXToYWithZ(gameObject2, "transfer", gameObject4, "to", gameObjectParameter2, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: true, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: true, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
									num++;
									continue;
								}
								Popup.Show(gameObjectParameter2.The + gameObjectParameter2.ShortDisplayName + " cannot take " + gameObject4.the + gameObject4.ShortDisplayName + ".");
								skip.Add(gameObject4);
							}
							if (num > 0)
							{
								gameObject2.UseEnergy(1000, "Ammo Magazine Transfer");
							}
						}
						else
						{
							int desiredCharge = GetDesiredCharge();
							int num3 = GetAccessibleCharge();
							if (num3 < desiredCharge)
							{
								List<GameObject> list = Event.NewGameObjectList();
								inventory.GetObjects(list, FindEnergyCells);
								int num4 = 0;
								while (num3 < desiredCharge && list.Count > 0 && ++num4 < 100)
								{
									GameObject gameObject5 = null;
									int num5 = 0;
									foreach (GameObject item in list)
									{
										int num6 = item.QueryCharge(LiveOnly: false, 0L);
										if (num6 < ChargeUse)
										{
											continue;
										}
										int num7 = num6 - (desiredCharge - num3);
										if (gameObject5 == null)
										{
											gameObject5 = item;
											num5 = num7;
										}
										else if (num7 >= 0)
										{
											if (num7 < num5)
											{
												gameObject5 = item;
												num5 = num7;
											}
										}
										else if (num7 > num5)
										{
											gameObject5 = item;
											num5 = num7;
										}
									}
									if (gameObject5 != null)
									{
										gameObject5 = gameObject5.RemoveOne();
										IComponent<GameObject>.WDidXToYWithZ(gameObject2, "transfer", gameObject5, "to", gameObjectParameter2, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: true, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: true, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
										if (gameObjectParameter2.ReceiveObject(gameObject5))
										{
											num3 += gameObject5.QueryCharge(LiveOnly: false, 0L);
										}
										else
										{
											gameObject5.CheckStack();
										}
										gameObject2.UseEnergy(1000, "Ammo Magazine Transfer");
									}
								}
							}
						}
					}
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo" && ChargeUse > 0)
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter3 != null && gameObjectParameter3.Inventory != null)
			{
				int desiredCharge2 = GetDesiredCharge();
				int num8 = GetAccessibleCharge();
				int num9 = 0;
				while (num8 < desiredCharge2 && ++num9 < 100)
				{
					string text = null;
					int num10 = 0;
					foreach (GameObjectBlueprint energyCellBlueprint in GetEnergyCellBlueprints())
					{
						int num11 = 0;
						if (!BlueprintMaxCharge.ContainsKey(energyCellBlueprint.Name))
						{
							try
							{
								num11 = Convert.ToInt32(energyCellBlueprint.GetPartParameter("EnergyCell", "MaxCharge"));
								BlueprintMaxCharge.Add(energyCellBlueprint.Name, num11);
							}
							catch
							{
								continue;
							}
						}
						else
						{
							num11 = BlueprintMaxCharge[energyCellBlueprint.Name];
						}
						if (num11 < ChargeUse)
						{
							continue;
						}
						int num12 = num11 - (desiredCharge2 - num8);
						if (text == null)
						{
							text = energyCellBlueprint.Name;
							num10 = num12;
						}
						else if (num12 >= 0)
						{
							if (num12 < num10)
							{
								text = energyCellBlueprint.Name;
								num10 = num12;
							}
						}
						else if (num12 > num10)
						{
							text = energyCellBlueprint.Name;
							num10 = num12;
						}
					}
					if (text == null)
					{
						continue;
					}
					GameObject gameObject6 = GameObject.create(text);
					EnergyCell part3 = gameObject6.GetPart<EnergyCell>();
					if (part3 != null)
					{
						part3.Charge = part3.MaxCharge;
						if (gameObjectParameter3.ReceiveObject(gameObject6))
						{
							num8 += part3.Charge;
						}
						else
						{
							gameObject6.Obliterate();
						}
					}
					else
					{
						gameObject6.Obliterate();
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	private bool FindEnergyCells(GameObject obj)
	{
		return obj.HasPartDescendedFrom<IEnergyCell>();
	}

	private List<GameObjectBlueprint> GetEnergyCellBlueprints()
	{
		if (EnergyCellBlueprints == null)
		{
			EnergyCellBlueprints = new List<GameObjectBlueprint>();
			foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
			{
				if (value.HasPart("EnergyCell") && !value.HasTag("ExcludeFromDynamicEncounters") && !value.HasTag("BaseObject") && !string.IsNullOrEmpty(value.GetPartParameter("EnergyCell", "MaxCharge")))
				{
					EnergyCellBlueprints.Add(value);
				}
			}
		}
		return EnergyCellBlueprints;
	}

	private int GetChargePerAction()
	{
		MissileWeapon part = ParentObject.GetPart<MissileWeapon>();
		return ChargeUse * part.AmmoPerAction;
	}

	private int GetDesiredCharge()
	{
		return GetChargePerAction() * ParentObject.GetIntProperty("IntegratedWeaponHostShots", 100);
	}

	private int GetAccessibleCharge()
	{
		int num = ParentObject.QueryCharge(LiveOnly: false, 0L);
		GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
		if (gameObject != null)
		{
			Inventory inventory = gameObject.Inventory;
			if (inventory != null)
			{
				foreach (GameObject @object in inventory.Objects)
				{
					foreach (IEnergyCell item in @object.GetPartsDescendedFrom<IEnergyCell>())
					{
						num += item.GetCharge();
					}
				}
				return num;
			}
		}
		return num;
	}

	public string GetDamageRoll(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		return ElectricalGeneration.GetDischargeDamageRoll(GetEffectiveCharge(ChargeUse, PowerLoadLevel));
	}

	public int GetVoltage(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		return ElectricalGeneration.GetDischargeVoltage(GetEffectiveCharge(ChargeUse, PowerLoadLevel));
	}

	public int GetEffectiveCharge(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		int num = PowerLoadLevel ?? MyPowerLoadLevel();
		float num2 = ChargeUse ?? GetActiveChargeUse(ChargeUse, PowerLoadLevel);
		if (num != 100)
		{
			int activeChargeUse = GetActiveChargeUse(null, 100);
			num2 = (float)activeChargeUse + (num2 - (float)activeChargeUse) * OverloadFactor;
		}
		return (int)Math.Round(num2 * ChargeFactor);
	}
}
