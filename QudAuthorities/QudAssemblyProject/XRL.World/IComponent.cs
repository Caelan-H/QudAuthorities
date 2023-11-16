using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Genkit;
using Newtonsoft.Json;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public abstract class IComponent<T>
{
	[NonSerialized]
	public static Dictionary<FieldInfo, FieldSaveVersion> fieldSaveVersionInfo = new Dictionary<FieldInfo, FieldSaveVersion>();

	public static long frameTimerMS => XRLCore.FrameTimer.ElapsedMilliseconds;

	[JsonIgnore]
	public Cell currentCell => GetBasisCell();

	public static long currentTurn => The.Game.Turns;

	public static long wallTime => The.Game.WallTime.ElapsedMilliseconds;

	public static XRLCore TheCore => The.Core;

	public static XRLGame TheGame => The.Game;

	public static GameObject ThePlayer => The.Player;

	public static Cell ThePlayerCell => The.Player?.CurrentCell;

	public static string ThePlayerMythDomain => The.Game.GetStringGameState("ThePlayerMythDomain", "glass");

	public static bool TerseMessages
	{
		get
		{
			if (The.Game.Player.Messages.Terse)
			{
				return true;
			}
			return false;
		}
	}

	[JsonIgnore]
	public bool juiceEnabled => Options.UseOverlayCombatEffects;

	public bool IsHidden
	{
		get
		{
			if (GetBasisGameObject()?.GetPart("Hidden") is Hidden hidden)
			{
				return !hidden.Found;
			}
			return false;
		}
	}

	[JsonIgnore]
	public ActivatedAbilities MyActivatedAbilities => GetBasisGameObject()?.ActivatedAbilities;

	[JsonIgnore]
	public bool OnWorldMap => GetAnyBasisCell()?.OnWorldMap() ?? false;

	[JsonIgnore]
	public virtual string DebugName
	{
		get
		{
			GameObject basisGameObject = GetBasisGameObject();
			if (basisGameObject != null)
			{
				return basisGameObject.DebugName + ":" + GetType().Name;
			}
			return GetType().Name;
		}
	}

	public virtual bool HandleEvent(AIBoredEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ActorGetNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AddedToInventoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustTotalWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustVisibilityRadiusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AdjustWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterAddSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterAfterThrownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterGameLoadedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterInventoryActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterMentalDefendEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterObjectCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterRemoveSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterThrownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AfterZoneBuiltEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowHugeHandsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowInventoryStackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AllowTradeWithNoInventoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AnimateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ApplyEffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttackerDealingDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttackerDealtDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AttemptToLandEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AutoexploreObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AwardXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AwardedXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(AwardingXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeAddSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeAfterThrownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeApplyDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDetonateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeDieEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeMentalDefendEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeRemoveSkillEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeRenderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeRenderLateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeTookDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeUnequippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeforeZoneBuiltEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginMentalDefendEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeginTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BeingConsumedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BlocksRadarEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BodyPositionChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BootSequenceAbortedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BootSequenceDoneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(BootSequenceInitializedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanAcceptObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanApplyEffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeDismemberedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeModdedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeNamedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeReplicatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanBeTradedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanFireAllMissileWeaponsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanGiveDirectionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanHaveConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanJoinPartyLeaderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanReceiveEmpathyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanReceiveTelepathyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanSmartUseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanStartConversationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanTemperatureReturnToAmbientEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CanTradeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CarryingCapacityChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CellChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ChargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ChargeUsedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAnythingToCleanEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAnythingToCleanWithEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAnythingToCleanWithNearbyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckAttackableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckExistenceSupportEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckGasCanAffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckOverburdenedOnStrengthUpdateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckPaintabilityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckSpawnMergeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckTileChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CheckUsesChargeWhileEquippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CleanItemsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CollectBroadcastChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandReloadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandSmartUseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(CommandTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ContainsAnyBlueprintEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ContainsBlueprintEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ContainsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DamageConstantAdjustedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DamageDieSizeAdjustedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DefendMeleeHitEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DerivationCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DropOnDeathEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DroppedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EarlyBeforeDeathRemovalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EffectAppliedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EffectForceAppliedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EffectRemovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EncounterChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EndActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EndSegmentEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EndTurnEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnterCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnteredCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnteringCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnvironmentalUpdateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EquippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EquipperEquippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExtraHostilePerceptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ExtremitiesMovedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FellDownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FindObjectByIdEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FinishChargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FinishRechargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ForceApplyEffectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(FrozeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GeneralAmnestyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericDeepNotifyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericDeepQueryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericNotifyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GenericQueryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetActivationPhaseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAttackerHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAutoCollectDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetAvailableComputePowerEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCleaningItemsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCleaningItemsNearbyEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetContentsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCookingActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCooldownEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetCriticalThresholdEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDebugInternalsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDefenderHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetDisplayNameEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetEnergyCostEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetExtraPhysicalFeaturesEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetExtrinsicValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFirefightingPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFixedMissileSpreadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetFreeDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetGenderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetHostileWalkRadiusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetIntrinsicValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetIntrinsicWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetInventoryCategoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetItemElementsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetKineticResistanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLevelUpDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLevelUpPointsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLevelUpSkillPointsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetLostChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMatterPhaseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileStatusColorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetModRarityWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetMutationTermEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetNamingBestowalChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetNamingChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetOverloadChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPointsOfInterestEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPowerLoadLevelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPrecognitionRestoreGameStateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPreferredLiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetProjectileObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetPsychicGlimmerEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRandomBuyChimericBodyPartRollsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRandomBuyMutationCountEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRealityStabilizationPenetrationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRebukeLevelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRespiratoryAgentPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRitualSifrahSetupEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetRunningBehaviorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetScanTypeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetShieldBlockPreferenceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSlottedInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSocialSifrahSetupEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSpecialEffectChanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSpringinessEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSprintDurationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetStorableDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetSwimmingPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetThrowProfileEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTinkeringBonusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTonicCapacityEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTradePerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetUtilityScoreEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWadingPerformanceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualCostEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualLiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWaterRitualSellSecretBehaviorEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetWeaponHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GiveDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GlimmerChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GravitationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HasBeenReadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HasFlammableEquipmentEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HasFlammableEquipmentOrInventoryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IActOnItemEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IActualEffectCheckEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IAdjacentNavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IBootSequenceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeConsumptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IChargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDeathEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDerivationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IDestroyObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IEffectCheckEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IFinalChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IHitDiceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IInitialChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ILiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IMentalAttackEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(INavigationWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IObjectCellInteractionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IObjectCreationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IPowerCordEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IRemoveFromContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IRenderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IReplicationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ISaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ITravelEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IValueEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IWeightEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IXPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IZoneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IdleQueryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ImplantedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InductionChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InterruptAutowalkEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(InventoryActionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsExplosiveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsMutantEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsOverloadableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsRepairableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsRootedInPlaceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsTrueKinEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(JoinedPartyLeaderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(KilledEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(KilledPlayerEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LateBeforeApplyDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeaveCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeavingCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeftCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LiquidMixedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MakeTemporaryEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModificationAppliedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyAttackingSaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyBitCostEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ModifyOriginatingSaveEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(MovementModeChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(NeedPartSupportEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(NeedsReloadEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectEnteredCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectEnteringCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectGoingProneEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectLeavingCellEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectStartedFlyingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ObjectStoppedFlyingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OkayToDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OnDeathRemovalEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OnDestroyObjectEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OnQuestAddedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OwnerGetShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(OwnerGetUnknownShortDescriptionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PartSupportEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PollForHealingLocationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerCordChargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerCordFinishChargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerCordQueryChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerCordTestChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerCordUseChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PreferTargetEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PreventSmartUseEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ProducesLiquidEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ProjectileMovingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryBroadcastDrawEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryChargeProductionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryChargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryDrawEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryEquippableListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryInductionChargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QueryRechargeStorageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(QuerySlotListEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RadiatesHeatAdjacentEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RadiatesHeatEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RealityStabilizeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RechargeAvailableEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RefreshTileEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RemoveFromContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RepaintedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RepairCriticalFailureEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RepairedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ReplaceInContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ReplicaCreatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ReputationChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RespiresEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ShouldDescribeStatBonusEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StackCountChangedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StatChangeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(StripContentsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SuspendingEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SyncRenderEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(SynchronizeExistenceEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TakenEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TestChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ThawedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TookDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TookEnvironmentalDamageEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TransparentToEMPEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TravelSpeedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(TryRemoveFromContextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UnequippedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UnimplantedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UseChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UseDramsEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UseHealingLocationEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(UsingChargeEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(VaporizedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WasDerivedFromEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(WasReplicatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneActivatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneBuiltEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneDeactivatedEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(ZoneThawedEvent E)
	{
		return true;
	}

	public abstract T GetComponentBasis();

	public virtual GameObject GetBasisGameObject()
	{
		return GetComponentBasis() as GameObject;
	}

	public virtual Cell GetBasisCell()
	{
		return (GetComponentBasis() as Cell) ?? FindBasisCell();
	}

	private Cell FindBasisCell()
	{
		return GetBasisGameObject()?.CurrentCell;
	}

	public virtual Cell GetAnyBasisCell()
	{
		return (GetComponentBasis() as Cell) ?? FindAnyBasisCell();
	}

	private Cell FindAnyBasisCell()
	{
		return GetBasisGameObject()?.GetCurrentCell();
	}

	public virtual Zone GetBasisZone()
	{
		return (GetComponentBasis() as Zone) ?? FindBasisZone();
	}

	private Zone FindBasisZone()
	{
		return GetBasisCell()?.ParentZone;
	}

	public virtual Zone GetAnyBasisZone()
	{
		return (GetComponentBasis() as Zone) ?? FindAnyBasisZone();
	}

	private Zone FindAnyBasisZone()
	{
		return GetAnyBasisCell()?.ParentZone;
	}

	public virtual bool handleDispatch(MinEvent e)
	{
		MetricsManager.LogError("base IComponent::handleDispatch called for " + e.GetType().Name);
		return true;
	}

	public bool hasProperty(string property)
	{
		return GetBasisGameObject()?.HasProperty(property) ?? false;
	}

	public void LogInEditor(string s)
	{
	}

	public void LogWarningInEditor(string s)
	{
	}

	public void LogErrorInEditor(string s)
	{
	}

	public void Log(string s)
	{
		MetricsManager.LogInfo(s);
	}

	public void LogWarning(string s)
	{
		MetricsManager.LogWarning(s);
	}

	public void LogError(string s)
	{
		MetricsManager.LogError(s);
	}

	public void LogError(string s, Exception ex)
	{
		MetricsManager.LogError(s, ex);
	}

	public float ConTarget(GameObject target = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return -1f;
		}
		if (target == null)
		{
			target = basisGameObject.GetHostilityTarget();
			if (target == null)
			{
				return -1f;
			}
		}
		if (target.IsPlayer())
		{
			return 1f;
		}
		if (!basisGameObject.HasStat("Level"))
		{
			return 1f;
		}
		if (!target.HasStat("Level"))
		{
			return 0f;
		}
		int num = target.Stat("Level");
		if (target.GetTagOrStringProperty("Role", "Minion") == "Minion")
		{
			num /= 4;
		}
		if (basisGameObject.HasStat("Hitpoints"))
		{
			if (basisGameObject.hitpoints < basisGameObject.BaseStat("Hitpoints") / 2)
			{
				num *= 2;
			}
			if (basisGameObject.hitpoints < basisGameObject.BaseStat("Hitpoints") / 4)
			{
				num *= 2;
			}
		}
		return (float)num / (float)basisGameObject.Stat("Level");
	}

	public float ConTarget(Event E)
	{
		return ConTarget(E.GetGameObjectParameter("Target"));
	}

	public CombatJuiceEntryWorldSound GetJuiceWorldSound(string clip, float Volume = 0.5f, float PitchVariance = 0f, float Delay = 0f)
	{
		if (!string.IsNullOrEmpty(clip) && Options.Sound)
		{
			Cell anyBasisCell = GetAnyBasisCell();
			if (anyBasisCell == null)
			{
				return null;
			}
			if (!anyBasisCell.ParentZone.IsActive())
			{
				return null;
			}
			Cell thePlayerCell = ThePlayerCell;
			if (thePlayerCell == null)
			{
				return null;
			}
			bool occluded = !anyBasisCell.IsVisible();
			if (thePlayerCell.ParentZone.IsWorldMap() || (float)anyBasisCell.PathDistanceTo(thePlayerCell) <= 40f * Volume)
			{
				if (Zone.SoundMapDirty)
				{
					anyBasisCell.ParentZone.UpdateSoundMap();
				}
				int num = Zone.SoundMap.GetCostAtPoint(anyBasisCell.Pos2D);
				if (num == int.MaxValue)
				{
					num = Zone.SoundMap.GetCostFromPointDirection(anyBasisCell.location, Zone.SoundMap.GetLowestCostDirectionFrom(anyBasisCell.Pos2D));
				}
				if (num == 9999)
				{
					num = anyBasisCell.PathDistanceTo(thePlayerCell);
				}
				return new CombatJuiceEntryWorldSound(clip, num, occluded, Volume, PitchVariance, Delay);
			}
		}
		return null;
	}

	public void PlayWorldSound(string clip, float Volume = 0.5f, float PitchVariance = 0f, bool combat = false, Cell SourceCell = null)
	{
		if (clip == null || !Options.Sound)
		{
			return;
		}
		if (SourceCell == null)
		{
			SourceCell = GetAnyBasisCell();
			if (SourceCell == null)
			{
				return;
			}
		}
		SourceCell.PlayWorldSound(clip, Volume, PitchVariance, combat);
	}

	public static void PlayUISound(string clip, float Volume = 1f, bool combat = false)
	{
		if (!string.IsNullOrEmpty(clip) && Options.Sound && (!combat || Options.UseCombatSounds))
		{
			SoundManager.PlaySound(clip, 0f, Volume);
		}
	}

	public virtual bool FireEvent(Event E)
	{
		return true;
	}

	public bool FireEvent(Event E, IEvent ParentEvent)
	{
		bool result = FireEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	public bool IsDay()
	{
		return Calendar.IsDay();
	}

	public bool IsNight()
	{
		return !Calendar.IsDay();
	}

	public static void AddPlayerMessage(string Message, string Color = null, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public static void AddPlayerMessage(string Message, char Color, bool Capitalize = true)
	{
		MessageQueue.AddPlayerMessage(Message, Color, Capitalize);
	}

	public bool IsPlayer()
	{
		GameObject player = The.Player;
		if (player != null)
		{
			return GetBasisGameObject() == player;
		}
		return false;
	}

	public void Reveal()
	{
		(GetBasisGameObject()?.GetPart("Hidden") as Hidden)?.Reveal();
	}

	public Cell PickDirection(string Label = null, GameObject POV = null)
	{
		return PickDirection(ForAttack: true, Label, POV);
	}

	public string PickDirectionS(string Label = null, GameObject POV = null)
	{
		return PickDirectionS(ForAttack: true, POV, Label);
	}

	public string PickDirectionS(bool ForAttack, GameObject POV = null, string Label = null)
	{
		if (POV == null)
		{
			GameObject basisGameObject = GetBasisGameObject();
			POV = basisGameObject.Equipped ?? basisGameObject.InInventory ?? basisGameObject;
			if (POV == null)
			{
				return null;
			}
		}
		if (POV.IsSelfControlledPlayer())
		{
			return XRL.UI.PickDirection.ShowPicker(Label);
		}
		GameObject hostilityTarget = POV.GetHostilityTarget();
		if (hostilityTarget != null)
		{
			return POV.CurrentCell.GetDirectionFromCell(hostilityTarget.CurrentCell);
		}
		return null;
	}

	public Cell PickDirection(bool ForAttack, string Label = null, GameObject POV = null)
	{
		if (POV == null)
		{
			GameObject basisGameObject = GetBasisGameObject();
			POV = basisGameObject.Equipped ?? basisGameObject.InInventory ?? basisGameObject;
			if (POV == null)
			{
				return null;
			}
		}
		if (POV.IsSelfControlledPlayer())
		{
			string text = XRL.UI.PickDirection.ShowPicker(Label);
			if (text != null)
			{
				return POV.CurrentCell.GetCellFromDirection(text);
			}
		}
		else
		{
			GameObject hostilityTarget = POV.GetHostilityTarget();
			if (hostilityTarget != null)
			{
				Cell cell = POV.CurrentCell;
				return cell.GetCellFromDirection(cell.GetDirectionFromCell(hostilityTarget.CurrentCell));
			}
		}
		return null;
	}

	public bool HasPropertyOrTag(string TagName)
	{
		return GetBasisGameObject()?.HasPropertyOrTag(TagName) ?? false;
	}

	public bool HasTag(string TagName)
	{
		return GetBasisGameObject()?.HasTag(TagName) ?? false;
	}

	public string GetPropertyOrTag(string Name, string Default = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return Default;
		}
		return basisGameObject.GetPropertyOrTag(Name, Default);
	}

	public string GetTag(string TagName, string Default = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return Default;
		}
		return basisGameObject.GetTag(TagName, Default);
	}

	public List<Cell> PickCloud(int Radius)
	{
		GameObject Me = GetBasisGameObject();
		if (Me == null)
		{
			return null;
		}
		List<Cell> adjacentCells = Me.CurrentCell.GetAdjacentCells(Radius);
		if (!Me.IsSelfControlledPlayer())
		{
			foreach (Cell item in adjacentCells)
			{
				if (item.HasObjectWithPart("Combat", (GameObject GO) => !Me.IsHostileTowards(GO) && Me.PhaseMatches(GO)))
				{
					return null;
				}
			}
			return adjacentCells;
		}
		return adjacentCells;
	}

	public List<Cell> PickLine(int Length, AllowVis VisLevel, Predicate<GameObject> objectTest = null, bool IgnoreSolid = false, bool IgnoreLOS = false, bool RequireCombat = true, GameObject Attacker = null, GameObject Projectile = null, string Label = null, bool Snap = false)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return Event.NewCellList();
		}
		Cell cell = basisGameObject.CurrentCell;
		if (cell.IsGraveyard())
		{
			return Event.NewCellList();
		}
		Zone parentZone = cell.ParentZone;
		if (parentZone == null)
		{
			return Event.NewCellList();
		}
		if (basisGameObject.IsSelfControlledPlayer())
		{
			Cell cell2 = cell;
			if (Snap && basisGameObject.GetTotalConfusion() <= 0)
			{
				GameObject gameObject = basisGameObject.Target ?? basisGameObject.GetNearestVisibleObject(Hostile: true, IncludeSolid: !IgnoreSolid, IgnoreLOS: IgnoreLOS, SearchPart: RequireCombat ? "Combat" : "Physics");
				if (gameObject != null)
				{
					cell2 = gameObject.CurrentCell ?? cell2;
				}
			}
			Cell cell3 = PickTarget.ShowPicker(PickTarget.PickStyle.Line, Length, 999, cell2.X, cell2.Y, Locked: true, VisLevel, null, objectTest, null, null, Label);
			List<Cell> list = Event.NewCellList();
			if (cell3 == null)
			{
				return null;
			}
			{
				foreach (Point item in Zone.Line(cell.X, cell.Y, cell3.X, cell3.Y))
				{
					list.Add(parentZone.GetCell(item.X, item.Y));
				}
				return list;
			}
		}
		GameObject hostilityTarget = basisGameObject.GetHostilityTarget();
		if (hostilityTarget != null)
		{
			if (objectTest != null && !objectTest(hostilityTarget))
			{
				return null;
			}
			Cell cell4 = hostilityTarget.CurrentCell;
			int x = cell.X - Length;
			int x2 = cell.X + Length;
			int y = cell.Y - Length;
			int y2 = cell.Y + Length;
			parentZone.Constrain(ref x, ref y, ref x2, ref y2);
			List<Cell> list2 = Event.NewCellList();
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					list2.Clear();
					List<Point> list3 = Zone.Line(cell.X, cell.Y, i, j);
					bool flag = false;
					foreach (Point item2 in list3)
					{
						if ((item2.X != cell.X || item2.Y != cell.Y) && item2.X > 0 && item2.Y > 0 && item2.X < parentZone.Width && item2.Y < parentZone.Height)
						{
							Cell cell5 = parentZone.GetCell(item2.X, item2.Y);
							list2.Add(cell5);
							if (cell5 == cell4)
							{
								flag = true;
							}
						}
					}
					if (!flag)
					{
						continue;
					}
					bool flag2 = false;
					for (int k = 0; k < list2.Count; k++)
					{
						if (flag2)
						{
							break;
						}
						if (!IgnoreSolid && list2[k] != cell4 && list2[k].IsSolidFor(Projectile, Attacker))
						{
							flag2 = true;
							break;
						}
						foreach (GameObject item3 in list2[k].LoopObjectsWithPart("Combat"))
						{
							if (!basisGameObject.IsHostileTowards(item3) && item3.PhaseMatches(basisGameObject))
							{
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						return list2;
					}
				}
			}
		}
		return null;
	}

	public List<Cell> PickCone(int Length, int Angle, AllowVis VisLevel, Predicate<GameObject> objectTest = null, string Label = null)
	{
		GameObject Me = GetBasisGameObject();
		if (Me == null || Me.IsInGraveyard())
		{
			return null;
		}
		if (Me.IsSelfControlledPlayer())
		{
			Cell cell = Me.CurrentCell;
			Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.Cone, Angle, Length, cell.X, cell.Y, Locked: false, VisLevel, null, objectTest, null, null, Label);
			if (cell2 == null)
			{
				return null;
			}
			List<Location2D> cone = XRL.Rules.Geometry.GetCone(Location2D.get(cell.X, cell.Y), Location2D.get(cell2.X, cell2.Y), Length, Angle);
			List<Cell> list = new List<Cell>();
			{
				foreach (Location2D item in cone)
				{
					list.Add(cell2.ParentZone.GetCell(item.x, item.y));
				}
				return list;
			}
		}
		GameObject hostilityTarget = Me.GetHostilityTarget();
		if (hostilityTarget != null)
		{
			if (objectTest != null && !objectTest(hostilityTarget))
			{
				return null;
			}
			Cell cell3 = Me.CurrentCell;
			Cell cell4 = hostilityTarget.CurrentCell;
			Zone parentZone = cell3.ParentZone;
			List<Location2D> cone2 = XRL.Rules.Geometry.GetCone(cell3.location, cell4.location, Length, Angle);
			List<Cell> list2 = new List<Cell>();
			foreach (Location2D item2 in cone2)
			{
				list2.Add(parentZone.GetCell(item2.x, item2.y));
			}
			list2.Remove(cell3);
			if (list2.Contains(cell4))
			{
				foreach (Cell item3 in list2)
				{
					if (item3.HasObjectWithPart("Combat", (GameObject GO) => !Me.IsHostileTowards(GO) && Me.PhaseMatches(GO)))
					{
						return null;
					}
				}
				return list2;
			}
		}
		return null;
	}

	public List<Cell> PickBurst(int Radius, int Range, bool bLocked, AllowVis VisLevel, string Label = null)
	{
		GameObject Me = GetBasisGameObject();
		try
		{
			if (Me == null)
			{
				return null;
			}
			if (Me.IsSelfControlledPlayer())
			{
				Cell cell = Me.CurrentCell;
				if (cell == null)
				{
					return null;
				}
				Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.Burst, Radius, Range, cell.X, cell.Y, bLocked, VisLevel, null, null, null, null, Label);
				if (cell2 == null)
				{
					return null;
				}
				List<Cell> Ret = new List<Cell>();
				Ret.Add(cell2);
				List<Cell> list = new List<Cell>(8 * Radius);
				for (int i = 0; i < Radius; i++)
				{
					int j = 0;
					for (int count = Ret.Count; j < count; j++)
					{
						Cell cell3 = Ret[j];
						if (list.CleanContains(cell3))
						{
							continue;
						}
						list.Add(cell3);
						cell3.ForeachAdjacentCell(delegate(Cell AC)
						{
							if (AC != null && !Ret.Contains(AC))
							{
								Ret.Add(AC);
							}
						});
					}
				}
				return Ret;
			}
			GameObject hostilityTarget = Me.GetHostilityTarget();
			if (hostilityTarget != null)
			{
				Cell cell4 = Me.CurrentCell;
				if (cell4 == null)
				{
					return null;
				}
				Zone parentZone = cell4.ParentZone;
				if (parentZone == null)
				{
					return null;
				}
				Cell cell5 = hostilityTarget.CurrentCell;
				if (cell5 == null || cell5.ParentZone != parentZone)
				{
					return null;
				}
				int x = cell4.X - Range;
				int x2 = cell4.X + Range;
				int y = cell4.Y - Range;
				int y2 = cell4.Y + Range;
				parentZone.Constrain(ref x, ref y, ref x2, ref y2);
				for (int k = x; k <= x2; k++)
				{
					for (int l = y; l <= y2; l++)
					{
						List<Cell> Cells = new List<Cell>((Radius * 2 + 1) * (Radius * 2 + 1)) { parentZone.GetCell(k, l) };
						List<Cell> list2 = new List<Cell>((Radius * 2 + 1) * (Radius * 2 + 1));
						for (int m = 0; m < Radius; m++)
						{
							int n = 0;
							for (int count2 = Cells.Count; n < count2; n++)
							{
								Cell cell6 = Cells[n];
								if (list2.Contains(cell6))
								{
									continue;
								}
								list2.Add(cell6);
								cell6.ForeachLocalAdjacentCell(delegate(Cell AC)
								{
									if (!Cells.Contains(AC))
									{
										Cells.Add(AC);
									}
								});
							}
						}
						if (!Cells.Contains(cell5) || Cells.Contains(cell4))
						{
							continue;
						}
						foreach (Cell item in Cells)
						{
							if (item.HasObjectWithPart("Combat", (GameObject GO) => !Me.IsHostileTowards(GO) && Me.PhaseMatches(GO)))
							{
								return null;
							}
						}
						return Cells;
					}
				}
			}
			return null;
		}
		catch (Exception x3)
		{
			if (Me == null)
			{
				MetricsManager.LogException("PickBurst(null)", x3);
			}
			else if (Me.IsPlayer())
			{
				MetricsManager.LogException("PickBurst(player)", x3);
			}
			else
			{
				MetricsManager.LogException("PickBurst(" + Me.DebugName + ")", x3);
			}
			return null;
		}
	}

	public List<Cell> PickCircle(int Radius, int Range, bool bLocked, AllowVis bAllowNonvis, string Label = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject.IsSelfControlledPlayer())
		{
			Cell cell = basisGameObject.CurrentCell;
			if (cell == null)
			{
				return null;
			}
			Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.Circle, Radius, Range, cell.X, cell.Y, bLocked, bAllowNonvis, null, null, null, null, Label);
			if (cell2 == null)
			{
				return null;
			}
			List<Cell> list = new List<Cell>();
			list.Add(cell2);
			int x = cell2.X;
			int y = cell2.Y;
			int x2 = cell2.X - Radius;
			int x3 = cell2.X + Radius;
			int y2 = cell2.Y - Radius;
			int y3 = cell2.Y + Radius;
			cell2.ParentZone.Constrain(ref x2, ref y2, ref x3, ref y3);
			for (int i = x2; i <= x3; i++)
			{
				for (int j = y2; j <= y3; j++)
				{
					if (Math.Sqrt((i - x) * (i - x) + (j - y) * (j - y)) <= (double)Radius)
					{
						list.Add(cell2.ParentZone.GetCell(i, j));
					}
				}
			}
			return list;
		}
		GameObject target = basisGameObject.Target;
		if (target != null)
		{
			Cell cell3 = target.CurrentCell;
			if (cell3 != null)
			{
				List<Cell> localAdjacentCells = cell3.GetLocalAdjacentCells(1);
				if (!localAdjacentCells.Contains(cell3))
				{
					localAdjacentCells.Add(cell3);
				}
				return localAdjacentCells;
			}
		}
		return null;
	}

	public List<Cell> PickField(int Cells, GameObject Actor = null, string What = null, bool ReturnNullForAbort = false, bool RequireVisibility = false)
	{
		GameObject gameObject = Actor ?? GetBasisGameObject();
		if (gameObject == null)
		{
			return null;
		}
		if (gameObject.IsSelfControlledPlayer())
		{
			if (string.IsNullOrEmpty(What))
			{
				return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, "Wall", StartAdjacent: false, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
			}
			return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, What, StartAdjacent: false, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
		}
		return null;
	}

	public List<Cell> PickFieldAdjacent(int Cells, GameObject Actor = null, string What = null, bool ReturnNullForAbort = false, bool RequireVisibility = false)
	{
		GameObject gameObject = Actor ?? GetBasisGameObject();
		if (gameObject == null)
		{
			return null;
		}
		if (gameObject.IsSelfControlledPlayer())
		{
			if (string.IsNullOrEmpty(What))
			{
				return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, "Wall", StartAdjacent: true, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
			}
			return PickTarget.ShowFieldPicker(Cells, 1, gameObject.CurrentCell.X, gameObject.CurrentCell.Y, What, StartAdjacent: true, ReturnNullForAbort, AllowDiagonals: false, AllowDiagonalStart: true, RequireVisibility);
		}
		return null;
	}

	public Cell PickDestinationCell(int Range, AllowVis Vis, bool Locked = true, bool IgnoreSolid = false, bool IgnoreLOS = false, bool RequireCombat = true, PickTarget.PickStyle Style = PickTarget.PickStyle.EmptyCell, string Label = null, bool Snap = false, Predicate<GameObject> ExtraVisibility = null)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject.IsSelfControlledPlayer())
		{
			Cell cell = basisGameObject.CurrentCell;
			if (Snap && basisGameObject.GetTotalConfusion() <= 0)
			{
				GameObject gameObject = basisGameObject.Target ?? basisGameObject.GetNearestVisibleObject(Hostile: true, IncludeSolid: !IgnoreSolid, IgnoreLOS: IgnoreLOS, SearchPart: RequireCombat ? "Combat" : "Physics", Radius: 80, ExtraVisibility: ExtraVisibility);
				if (gameObject != null)
				{
					cell = gameObject.CurrentCell ?? cell;
				}
			}
			if (cell != null)
			{
				return PickTarget.ShowPicker(Style, 1, Range, cell.X, cell.Y, Locked, Vis, ExtraVisibility, null, null, null, Label);
			}
		}
		else
		{
			GameObject hostilityTarget = basisGameObject.GetHostilityTarget();
			if (hostilityTarget != null)
			{
				return hostilityTarget.CurrentCell;
			}
		}
		return null;
	}

	public bool DoIHaveAMissileWeapon()
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return false;
		}
		List<GameObject> missileWeapons = basisGameObject.GetMissileWeapons();
		if (missileWeapons != null && missileWeapons.Count > 0)
		{
			foreach (GameObject item in missileWeapons)
			{
				if (item.GetPart("MissileWeapon") is MissileWeapon missileWeapon && missileWeapon.ReadyToFire())
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Dictionary<string, int> MapFromString(string raw)
	{
		string[] array = raw.Split(',');
		Dictionary<string, int> dictionary = new Dictionary<string, int>(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(':');
			if (array3.Length != 2)
			{
				throw new Exception("bad element in string map: " + text);
			}
			string key = array3[0];
			int value = Convert.ToInt32(array3[1]);
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	public bool LiquidAvailable(string LiquidID, int Amount = 1, bool impureOkay = true)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return false;
		}
		return basisGameObject.GetFreeDrams(LiquidID, null, null, null, impureOkay) >= Amount;
	}

	public bool ConsumeLiquid(string LiquidID, int Amount = 1, bool impureOkay = true)
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return false;
		}
		if (impureOkay)
		{
			return basisGameObject.UseImpureDrams(Amount, LiquidID);
		}
		return basisGameObject.UseDrams(Amount, LiquidID);
	}

	public static void EmitMessage(GameObject what, string Msg, bool FromDialog = false, bool UsePopup = false)
	{
		Messaging.EmitMessage(what, Msg, FromDialog, UsePopup);
	}

	public void EmitMessage(GameObject what, StringBuilder Msg, bool FromDialog = false, bool UsePopup = false)
	{
		EmitMessage(what, Msg.ToString(), FromDialog, UsePopup);
	}

	public void EmitMessage(string Msg, bool FromDialog = false, bool UsePopup = false)
	{
		EmitMessage(GetBasisGameObject(), Msg, FromDialog, UsePopup);
	}

	public void EmitMessage(StringBuilder Msg, bool FromDialog = false, bool UsePopup = false)
	{
		EmitMessage(Msg.ToString(), FromDialog, UsePopup);
	}

	protected static string ConsequentialColor(GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		return ColorCoding.ConsequentialColor(ColorAsGoodFor, ColorAsBadFor);
	}

	protected static char ConsequentialColorChar(GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		return ColorCoding.ConsequentialColorChar(ColorAsGoodFor, ColorAsBadFor);
	}

	public static void XDidY(GameObject what, string verb, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, GameObject SubjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidY(what, verb, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, SubjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidX(string verb, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, GameObject SubjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		XDidY(GetBasisGameObject(), verb, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, SubjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void XDidYToZ(GameObject what, string verb, string preposition, GameObject obj, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.XDidYToZ(what, verb, preposition, obj, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void XDidYToZ(GameObject what, string verb, GameObject obj, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		XDidYToZ(what, verb, null, obj, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToY(string verb, string preposition, GameObject obj, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		XDidYToZ(GetBasisGameObject(), verb, preposition, obj, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToY(string verb, GameObject obj, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool IndefiniteObject = false, bool IndefiniteObjectForOthers = false, bool PossessiveObject = false, GameObject SubjectPossessedBy = null, GameObject ObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		XDidYToZ(GetBasisGameObject(), verb, null, obj, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, IndefiniteObject, IndefiniteObjectForOthers, PossessiveObject, SubjectPossessedBy, ObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void WDidXToYWithZ(GameObject what, string verb, string directPreposition, GameObject directObject, string indirectPreposition, GameObject indirectObject, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool indefiniteDirectObject = false, bool indefiniteIndirectObject = false, bool indefiniteDirectObjectForOthers = false, bool indefiniteIndirectObjectForOthers = false, bool possessiveDirectObject = false, bool possessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject directObjectPossessedBy = null, GameObject indirectObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(what, verb, directPreposition, directObject, indirectPreposition, indirectObject, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, indefiniteDirectObject, indefiniteIndirectObject, indefiniteDirectObjectForOthers, indefiniteIndirectObjectForOthers, possessiveDirectObject, possessiveIndirectObject, SubjectPossessedBy, directObjectPossessedBy, indirectObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public static void WDidXToYWithZ(GameObject what, string verb, GameObject directObject, string indirectPreposition, GameObject indirectObject, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool indefiniteDirectObject = false, bool indefiniteIndirectObject = false, bool indefiniteDirectObjectForOthers = false, bool indefiniteIndirectObjectForOthers = false, bool possessiveDirectObject = false, bool possessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject directObjectPossessedBy = null, GameObject indirectObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		Messaging.WDidXToYWithZ(what, verb, null, directObject, indirectPreposition, indirectObject, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, indefiniteDirectObject, indefiniteIndirectObject, indefiniteDirectObjectForOthers, indefiniteIndirectObjectForOthers, possessiveDirectObject, possessiveIndirectObject, SubjectPossessedBy, directObjectPossessedBy, indirectObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToYWithZ(string verb, string directPreposition, GameObject directObject, string indirectPreposition, GameObject indirectObject, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool indefiniteDirectObject = false, bool indefiniteIndirectObject = false, bool indefiniteDirectObjectForOthers = false, bool indefiniteIndirectObjectForOthers = false, bool possessiveDirectObject = false, bool possessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject directObjectPossessedBy = null, GameObject indirectObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		WDidXToYWithZ(GetBasisGameObject(), verb, directPreposition, directObject, indirectPreposition, indirectObject, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, indefiniteDirectObject, indefiniteIndirectObject, indefiniteDirectObjectForOthers, indefiniteIndirectObjectForOthers, possessiveDirectObject, possessiveIndirectObject, SubjectPossessedBy, directObjectPossessedBy, indirectObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public void DidXToYWithZ(string verb, GameObject directObject, string indirectPreposition, GameObject indirectObject, string extra = null, string terminalPunctuation = null, string Color = null, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, bool UseFullNames = false, bool IndefiniteSubject = false, bool indefiniteDirectObject = false, bool indefiniteIndirectObject = false, bool indefiniteDirectObjectForOthers = false, bool indefiniteIndirectObjectForOthers = false, bool possessiveDirectObject = false, bool possessiveIndirectObject = false, GameObject SubjectPossessedBy = null, GameObject directObjectPossessedBy = null, GameObject indirectObjectPossessedBy = null, GameObject MessageActor = null, bool DescribeSubjectDirection = false, bool DescribeSubjectDirectionLate = false, bool AlwaysVisible = false, bool FromDialog = false, bool UsePopup = false, GameObject UseVisibilityOf = null)
	{
		WDidXToYWithZ(GetBasisGameObject(), verb, null, directObject, indirectPreposition, indirectObject, extra, terminalPunctuation, Color, ColorAsGoodFor, ColorAsBadFor, UseFullNames, IndefiniteSubject, indefiniteDirectObject, indefiniteIndirectObject, indefiniteDirectObjectForOthers, indefiniteIndirectObjectForOthers, possessiveDirectObject, possessiveIndirectObject, SubjectPossessedBy, directObjectPossessedBy, indirectObjectPossessedBy, MessageActor, DescribeSubjectDirection, DescribeSubjectDirectionLate, AlwaysVisible, FromDialog, UsePopup, UseVisibilityOf);
	}

	public bool IsBroken()
	{
		return GetBasisGameObject()?.IsBroken() ?? false;
	}

	public bool IsRusted()
	{
		return GetBasisGameObject()?.IsRusted() ?? false;
	}

	public bool IsEMPed()
	{
		return GetBasisGameObject()?.IsEMPed() ?? false;
	}

	public static bool Visible(GameObject obj)
	{
		return obj?.IsVisible() ?? false;
	}

	public bool Visible()
	{
		return Visible(GetBasisGameObject());
	}

	public ActivatedAbilities GetMyActivatedAbilities(GameObject who = null)
	{
		return (who ?? GetBasisGameObject())?.ActivatedAbilities;
	}

	public ActivatedAbilityEntry MyActivatedAbility(Guid ID, GameObject who = null)
	{
		return (who ?? GetBasisGameObject())?.GetActivatedAbility(ID);
	}

	public Guid AddMyActivatedAbility(string Name, string Command, string Class, string Description = null, string Icon = "\a", string DisabledMessage = null, bool Toggleable = false, bool DefaultToggleState = false, bool ActiveToggle = false, bool IsAttack = false, bool IsRealityDistortionBased = false, bool Silent = false, bool AIDisable = false, bool AlwaysAllowToggleOff = true, bool AffectedByWillpower = true, bool TickPerTurn = false, int Cooldown = -1, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return Guid.Empty;
			}
		}
		return who.AddActivatedAbility(Name, Command, Class, Description, Icon, DisabledMessage, Toggleable, DefaultToggleState, ActiveToggle, IsAttack, IsRealityDistortionBased, Silent, AIDisable, AlwaysAllowToggleOff, AffectedByWillpower, TickPerTurn, Distinct: false, Cooldown);
	}

	public bool RemoveMyActivatedAbility(ref Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.RemoveActivatedAbility(ref ID);
	}

	public bool EnableMyActivatedAbility(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.EnableActivatedAbility(ID);
	}

	public bool DisableMyActivatedAbility(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.DisableActivatedAbility(ID);
	}

	public bool ToggleMyActivatedAbility(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.ToggleActivatedAbility(ID);
	}

	public bool IsMyActivatedAbilityToggledOn(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityToggledOn(ID);
	}

	public bool IsMyActivatedAbilityCoolingDown(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityCoolingDown(ID);
	}

	public int GetMyActivatedAbilityCooldown(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return -1;
			}
		}
		return who.GetActivatedAbilityCooldown(ID);
	}

	public int GetMyActivatedAbilityCooldownTurns(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return -1;
			}
		}
		return who.GetActivatedAbilityCooldownTurns(ID);
	}

	public string GetMyActivatedAbilityCooldownDescription(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return "";
			}
		}
		return who.GetActivatedAbilityCooldownDescription(ID);
	}

	public bool CooldownMyActivatedAbility(Guid ID, int Turns, GameObject who = null, string tags = null, bool Involuntary = false)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.CooldownActivatedAbility(ID, Turns, tags, Involuntary);
	}

	public bool TakeMyActivatedAbilityOffCooldown(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.TakeActivatedAbilityOffCooldown(ID);
	}

	public bool IsMyActivatedAbilityUsable(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityUsable(ID);
	}

	public bool IsMyActivatedAbilityAIUsable(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityAIUsable(ID);
	}

	public bool IsMyActivatedAbilityAIDisabled(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.IsActivatedAbilityAIDisabled(ID);
	}

	public bool IsMyActivatedAbilityVoluntarilyUsable(Guid ID, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		if (!who.IsPlayer())
		{
			return IsMyActivatedAbilityAIUsable(ID);
		}
		return IsMyActivatedAbilityUsable(ID);
	}

	public bool SetMyActivatedAbilityDisplayName(Guid ID, string DisplayName, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.SetActivatedAbilityDisplayName(ID, DisplayName);
	}

	public bool SetMyActivatedAbilityDisabledMessage(Guid ID, string DisabledMessage, GameObject who = null)
	{
		if (who == null)
		{
			who = GetBasisGameObject();
			if (who == null)
			{
				return false;
			}
		}
		return who.SetActivatedAbilityDisabledMessage(ID, DisabledMessage);
	}

	public void FlushWeightCaches()
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject != null)
		{
			basisGameObject.FlushWeightCaches();
			basisGameObject.FlushContextWeightCaches();
		}
	}

	public void FlushNavigationCaches()
	{
		GetAnyBasisCell()?.ClearNavigationCache();
	}

	public void FlushWantTurnTickCache()
	{
		GetBasisGameObject()?.FlushWantTurnTickCache();
	}

	public int GasDensity()
	{
		GameObject basisGameObject = GetBasisGameObject();
		if (basisGameObject == null)
		{
			return 0;
		}
		if (!(basisGameObject.GetPart("Gas") is Gas gas))
		{
			return 0;
		}
		return gas.Density;
	}

	public int GasDensityStepped(int Step = 5)
	{
		return StepValue(GasDensity(), Step);
	}

	public int StepValue(int Value, int Step = 5)
	{
		int num = Value % Step;
		if (num == 0)
		{
			return Value;
		}
		if (num >= Step / 2)
		{
			return Value - num + Step;
		}
		return Value - num;
	}

	public int GetEpistemicStatus()
	{
		return GetBasisGameObject()?.GetEpistemicStatus() ?? 2;
	}

	public void AddLight(int r, LightLevel Level = LightLevel.Light, bool Force = false)
	{
		Cell basisCell = GetBasisCell();
		basisCell?.ParentZone.AddLight(basisCell.X, basisCell.Y, r, Level, Force);
	}

	public static int PowerLoadBonus(int Load, int Baseline = 100, int Divisor = 150)
	{
		if (Load > Baseline)
		{
			return (Load - Baseline) / Divisor;
		}
		return 0;
	}

	public virtual int MyPowerLoadBonus(int Load = int.MinValue, int Baseline = 100, int Divisor = 150)
	{
		if (Load == int.MinValue)
		{
			Load = GetBasisGameObject()?.GetPowerLoadLevel() ?? 100;
		}
		return PowerLoadBonus(Load, Baseline, Divisor);
	}

	public virtual int MyPowerLoadLevel()
	{
		return GetBasisGameObject()?.GetPowerLoadLevel() ?? 100;
	}

	public bool PerformMentalAttack(Mental.Attack Handler, GameObject Attacker, GameObject Defender, GameObject Source = null, string Command = null, string Dice = null, int Type = 0, int Magnitude = int.MinValue, int Penetrations = int.MinValue, int AttackModifier = 0, int DefenseModifier = 0)
	{
		return Mental.PerformAttack(Handler, Attacker, Defender, Source ?? GetBasisGameObject(), Command, Dice, Type, Magnitude, Penetrations, AttackModifier, DefenseModifier);
	}

	public static void ConstrainTier(ref int tier)
	{
		Tier.Constrain(ref tier);
	}

	public static int ConstrainTier(int tier)
	{
		return Tier.Constrain(tier);
	}

	public virtual bool WantTurnTick()
	{
		return false;
	}

	public virtual void TurnTick(long TurnNumber)
	{
	}

	public virtual bool WantTenTurnTick()
	{
		return false;
	}

	public virtual void TenTurnTick(long TurnNumber)
	{
	}

	public virtual bool WantHundredTurnTick()
	{
		return false;
	}

	public virtual void HundredTurnTick(long TurnNumber)
	{
	}

	public virtual bool WantEvent(int ID, int cascade)
	{
		return false;
	}

	public virtual bool HandleEvent(MinEvent E)
	{
		return true;
	}
}
