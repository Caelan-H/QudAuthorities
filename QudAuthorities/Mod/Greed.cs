﻿using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.Core;
using XRL.CharacterBuilds.Qud;
using System.Runtime.CompilerServices;
//using QudAuthorities.Mod;
using Qud.API;
using System.IO;
using System.Diagnostics;
using XRL.World.Effects;

using System.Reflection;
using XRL.World.ZoneBuilders;
using static TBComponent;
using XRL.World.AI.Pathfinding;

using XRL.UI.Framework;
using Battlehub.UIControls;

using XRL.World.Skills;

using XRL.World.Parts;
using XRL.World;
using XRL;

using UnityEngine;
using XRL.EditorFormats.Screen;
using MODNAME.Utilities;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Greed : BaseMutation
    {
        public Guid CorLeonisFirstShiftID;
        public Guid CorLeonisSecondShiftID;
        public Guid CorLeonisThirdShiftID;
        public Guid StillnessOfTimeID;
        public ActivatedAbilityEntry CorLeonisFirstShiftEntry = null;
        public ActivatedAbilityEntry CorLeonisSecondShiftEntry = null;
        public ActivatedAbilityEntry CorLeonisThirdShiftEntry = null;
        public ActivatedAbilityEntry StillnessOfTimeEntry = null;
        public GameObject toBeHealed = null;
        public bool firstShiftOn = false;
        public bool secondShiftOn = false;
        public bool thirdShiftOn = false;
        public bool stillnessOfTimeOn = false;
        public int stillnessCounter = 0;
        public int damageToTake = 0;
        public List<string> Authorities = new List<string>();
        public List<GameObject> stoppedMembers = new List<GameObject>();
        public int AwakeningOdds = 549;
        string WitchFactor = "";
        public Greed()
        {
            DisplayName = "Greed";
            Type = "Authority";
        }

        public override string GetDescription()
        {
            return "The Greed Witch Factor.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("A dark mass hiding within your soul writhes with avaricious intent....\n There is a 1/550" + " chance to awaken another Authority of Greed upon gaining XP. The Authorities are: Cor Leonis and Stillness Of Time.");

            /*
            if (Authorities.Count == 0 || Authorities.Count == 1)
            {
            }
            else
            {
                if (Authorities.Count > 1)
                {
                    return string.Concat("The dark impurity within you that yearned to amass has changed form. You can feel gentle light within your soul where the darkness used to creep into your very being. It is eager to endow others with it's charity. The potential of Greed is fully realized.");
                }
                return string.Concat("Something is wrong with the greed authority count!");
            }

            */

        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "CorLeonisFirstShift");
            Object.RegisterPartEvent(this, "CorLeonisSecondShift");
            Object.RegisterPartEvent(this, "CorLeonisThirdShift");
            Object.RegisterPartEvent(this, "StillnessOfTime");
            base.Register(Object);
        }

        public override bool HandleEvent(ZoneActivatedEvent E)
        {      
            if(Authorities.Contains("StillnessOfTime"))
            {
                stillnessCounter = 0;
                stoppedMembers.Clear();
                stillnessOfTimeOn = false;
            }



            return base.HandleEvent(E);
        }
        
        public override bool HandleEvent(ApplyEffectEvent E)
        {
            if(Authorities.Contains("CorLeonis"))
            {
                if (CorLeonisThirdShiftEntry.ToggleState == true)
                {
                    List<GameObject> listOfPartyMembers = new List<GameObject>();
                    foreach (var entity in ParentObject.CurrentZone.GetObjects())
                    {

                        if (entity.InSamePartyAs(ParentObject) && entity != ParentObject)
                        {
                            listOfPartyMembers.Add(entity);
                        }
                    }
                    if (listOfPartyMembers.Count > 0)
                    {

                        bool canTransfer = false;

                        switch (E.Effect.ClassName)
                        {
                            default:
                                canTransfer = false;
                                break;
                            case "Blaze_Tonic":
                                canTransfer = true;
                                break;
                            case "Emboldened":
                                canTransfer = true;
                                break;
                            case "Frenzied":
                                canTransfer = true;
                                break;
                            case "Metabolizing":
                                canTransfer = true;
                                break;
                            case "Luminous":
                                canTransfer = true;
                                break;
                            case "Hoarshroom_Tonic":
                                canTransfer = true;
                                break;
                            case "HulkHoney_Tonic":
                                canTransfer = true;
                                break;
                            case "Rubbergum_Tonic":
                                canTransfer = true;
                                break;
                            case "Salve_Tonic":
                                canTransfer = true;
                                break;
                            case "ShadeOil_Tonic":
                                canTransfer = true;
                                break;
                            case "Skulk_Tonic":
                                canTransfer = true;
                                break;
                            case "SphynxSalt_Tonic":
                                canTransfer = true;
                                break;
                            case "Ubernostrum_Tonic":
                                canTransfer = true;
                                break;
                            case "Ecstatic":
                                canTransfer = true;
                                break;
                            case "GeometricHeal":
                                canTransfer = true;
                                break;
                            case "Healing":
                                canTransfer = true;
                                break;
                            case "Meditating":
                                canTransfer = true;
                                break;
                            case "Wakeful":
                                canTransfer = true;
                                break;

                        }

                        if (canTransfer == true)
                        {
                            foreach (var member in listOfPartyMembers)
                            {

                                if (member.Effects.Contains(E.Effect))
                                {

                                }
                                else
                                {
                                   EffectDeepCopy(E.Effect, member);
                                }

                            }
                        }




                    }

                    return true;
                }
            }

            



            return base.HandleEvent(E);
        }
        public override bool HandleEvent(BeforeApplyDamageEvent E)
        {

            if (Authorities.Contains("CorLeonis"))
            {
                List<GameObject> listOfPartyMembers = new List<GameObject>();
                if (toBeHealed != null)
                {
                    listOfPartyMembers.Remove(toBeHealed);
                }

                foreach (var entity in ParentObject.CurrentZone.GetObjects())
                {

                    if (entity.InSamePartyAs(ParentObject))
                    {
                        listOfPartyMembers.Add(entity);
                    }
                }

                if (CorLeonisFirstShiftEntry.ToggleState == true)
                {


                }

                if (CorLeonisSecondShiftEntry.ToggleState == true)
                {
                    int original = E.Damage.Amount;
                    int half = (int)Math.Floor((decimal)original * (decimal)(.75));
                    int quarter = (int)Math.Floor((decimal)original * (decimal)(.25));
                    //int half = E.Damage.Amount / 2;
                    E.Damage.Amount = half;
                    BeforeApplyDamageEvent e = E;
                    //string attributes = E.Damage
                    List<GameObject> listOfPartyMembersValid = new List<GameObject>();

                    if (listOfPartyMembers.Count > 0)
                    {
                        foreach (var member in listOfPartyMembers)
                        {
                            if (member.GetHPPercent() <= 50 || member.hitpoints <= half)
                            {

                            }
                            else
                            {

                                listOfPartyMembersValid.Add(member);


                            }
                        }
                    }


                    if (listOfPartyMembersValid.Count > 0)
                    {
                        int dividedDamage = quarter;
                        if (listOfPartyMembers.Count > 3)
                        {
                            dividedDamage = quarter / 3;
                        }
                        else
                        {
                            dividedDamage = quarter / listOfPartyMembersValid.Count;
                        }


                        foreach (var member in listOfPartyMembersValid)
                        {
                            member.TakeDamage(ref dividedDamage);
                        }
                    }
                    else
                    {
                        E.Damage.Amount = original;
                    }

                }



                toBeHealed = null;
            }

            

                return base.HandleEvent(E);
            
        }
        public override bool WantEvent(int ID, int cascade)
        {
            
            //This event rolls for chances to refresh Authority cool downa and to unlock unobtained Authorities.
            if (ID == AuthorityAwakeningGreedEvent.ID)
            {
                if (ParentObject.IsPlayer())
                {
                    bool didGetAuthority = ObtainAuthority();
                }
            }
            if (ID == AwardedXPEvent.ID)
            {
                int a = MODNAME_Random.Next(0, AwakeningOdds);

                if (a == 1)
                {
                    AuthorityAwakeningGreedEvent.Send(ParentObject);
                }


            }
            return true;
        }

      

        public void BeginCorLeonisFirstShift(GameObject target)
        {

            if (target != null && target.HasPart("Brain") && target.InSamePartyAs(ParentObject))
            {
                bool success = false;
                if (ParentObject.IsPlayer())
                {
                    int currentHealth = target.hitpoints;
                    int input = (int)Popup.AskNumber("Give HP | " + target.DisplayName + "has " + target.hitpoints.ToString() + "/" + target.baseHitpoints.ToString() + "HP", 0, 0, ParentObject.hitpoints - 1);
                    if(input == 0 || input >= ParentObject.hitpoints)
                    {
                        success = false;
                    }
                    else
                    {
                        success = true;
                    }

                    
                    if (success == true)
                    {

                        ParentObject.TakeDamage(ref input);
                        target.Heal(input, true, true);
                    }
                    else
                    {
                        if(input >= ParentObject.hitpoints)
                        {
                            IComponent<GameObject>.AddPlayerMessage("You are unable to give your HP because you would die doing so.");
                        }
                        else
                        {
                            IComponent<GameObject>.AddPlayerMessage("You gave them no HP");
                        }

                    }
                }
                else if (target.IsPlayer())
                {

                }

            }
            else
            {
                IComponent<GameObject>.AddPlayerMessage("You chose an invalid target. It must be a party member with a brain.");
            }
        }



        public override bool FireEvent(Event E)
        {

            if (E.ID == "CorLeonisFirstShift")
            {
               

                    Cell cell = PickDestinationCell(80, AllowVis.OnlyVisible, Locked: false, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: false, PickTarget.PickStyle.EmptyCell, null, Snap: true);
                    if (cell == null)
                    {
                        return false;
                    }
                    GameObject gameObject = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
                    if (gameObject != null && gameObject.pBrain == null)
                    {
                        gameObject = null;
                    }
                    if (gameObject == null)
                    {
                        gameObject = cell.GetFirstObjectWithPart("Brain");
                    }


                    if (gameObject == ParentObject)
                    {
                        return false;
                    }
                    if (gameObject == null)
                    {
                        if (ParentObject.IsPlayer())
                        {

                            Popup.ShowFail("There's no target with a mind there.");

                        }
                        return false;
                    }

                    if (gameObject.InSamePartyAs(ParentObject))
                    {
                        UseEnergy(0, "Authority Mutation Cor Leonis: First Shift");
                        BeginCorLeonisFirstShift(gameObject);
                    }

               
            }

            if (E.ID == "CorLeonisSecondShift")
            {
                if (CorLeonisSecondShiftEntry.ToggleState == true)
                {
                    CorLeonisSecondShiftEntry.ToggleState = false;
                    secondShiftOn = false;
                }
                else
                {
                    CorLeonisSecondShiftEntry.ToggleState = true;
                    secondShiftOn = true;

                }

            }

            if (E.ID == "CorLeonisThirdShift")
            {
                if (CorLeonisThirdShiftEntry.ToggleState == true)
                {
                    CorLeonisThirdShiftEntry.ToggleState = false;
                }
                else
                {
                    CorLeonisThirdShiftEntry.ToggleState = true;
                }
            }

            if (E.ID == "StillnessOfTime")
            {

                if (ParentObject.OnWorldMap())
                {
                    Popup.ShowFail("You cannot use this on the world map");
                    return false;
                }
                // Popup.Show("activated stillness");
                List<GameObject> memberList = new List<GameObject>();
                List<GameObject> membersHelping = new List<GameObject>();
                if (secondShiftOn)
                {

                    memberList.Clear();
                    foreach (var entity in ParentObject.CurrentZone.GetObjects())
                    {


                        if (entity.InSamePartyAs(ParentObject))
                        {

                            memberList.Add(entity);
                        }


                    }


                    if (membersHelping.Contains(ThePlayer))
                    {

                        membersHelping.Remove(ThePlayer);
                    }
                    if (memberList.Count > 0)
                    {
                        foreach (var member in memberList)
                        {
                            if (member.GetHPPercent() <= 50)
                            {

                            }
                            else
                            {

                                membersHelping.Add(member);


                            }
                        }
                    }
                    
                    

                    //cooldown = cooldown - (num * 20);

                }

                if (((ParentObject.GetHPPercent() > 30 )))
                {
                    
                    int damage = (int)Math.Floor((decimal)ParentObject.baseHitpoints * (decimal)(.30));
                    
                    
                    ParentObject.TakeDamage(ref damage);
                   
                    stillnessOfTimeOn = true;
                  
                    int cooldown = 300;
              
                    int duration = 3;
              
                    if(membersHelping.Count > 0)
                    {
                        duration++;
                    }

                    foreach (var entity in ParentObject.CurrentZone.GetObjects())
                    {

                        if (entity.HasPart("Brain"))
                        {
                            if (entity == ParentObject)
                            {

                            }
                            else
                            {
                                stoppedMembers.Add(entity);
                                if (entity.HasEffect("Stillness"))
                                {

                                }
                                else
                                {
                                    entity.ApplyEffect(new Stillness(duration));
                                    
                                }

                            }


                        }

                        
                    }

                    //Popup.Show("after apply");

                    //ParentObject.Energy.BaseValue += duration * 1000;
                    CooldownMyActivatedAbility(StillnessOfTimeID, 120);
                    UseEnergy(0, "Authority Stillness Of Time");
                    stillnessCounter = duration;
                    IComponent<GameObject>.AddPlayerMessage("Time has been made still for  " + duration.ToString() + " turns.");
                    XRLCore.ParticleManager.Add("@", ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, (float)Math.Sin((double)(float)(100) * 0.017) / 10, (float)Math.Cos((double)(float)(100) * 0.017) / 10);
                    PlayWorldSound("timestop", .2f, 0f);
                    for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
                    {
                        float num = (float)Stat.RandomCosmetic(4, 14) / 3f;
                        for (int j = 0; j < 360; j++)
                        {
                            XRLCore.ParticleManager.Add("@", ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, (float)Math.Sin((double)(float)j * 0.017) / num, (float)Math.Cos((double)(float)j * 0.017) / num);
                        }
                    }
                    
                  
               }
                else
                {
                    IComponent<GameObject>.AddPlayerMessage("The strain to stop time would kill you, so you decide against it.");
                }
            }





            return base.FireEvent(E);
        }

      

        public override bool HandleEvent(EndTurnEvent E)
        {
            if(stillnessOfTimeOn)
            {
                if(stillnessCounter> 0) 
                {
                    foreach (var entity in ParentObject.CurrentZone.GetObjects())
                    {
                        PlayerTurnPassedEvent.Send(entity);
                    }
                    stillnessCounter--;
                    if(stillnessCounter==0)
                    {
                        IComponent<GameObject>.AddPlayerMessage("Time has now resumed.");
                        stillnessCounter = 0;
                        stoppedMembers.Clear();
                        stillnessOfTimeOn = false;
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    if(stillnessCounter <=0)
                    {
                        
                        stillnessCounter = 0;
                        stoppedMembers.Clear();
                        stillnessOfTimeOn= false;
                    }
                }
               
               
            }
            return base.HandleEvent(E);
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            
            
               ObtainAuthority();
               //ParentObject.GainIntelligence(1);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref CorLeonisFirstShiftID);
            RemoveMyActivatedAbility(ref CorLeonisSecondShiftID);
            RemoveMyActivatedAbility(ref CorLeonisThirdShiftID);
            RemoveMyActivatedAbility(ref StillnessOfTimeID);
            //ParentObject.GainIntelligence(-1);
            RecountEvent.Send(ParentObject);
            return base.Unmutate(GO);
        }

        public void SyncAbilityName_CorLeonisFirstShift()
        {
            
            
        }

        public void SyncAbilityName_CorLeonisSecondShift()
        {
            
        }

        public void SyncAbilityName_CorLeonisThirdShift()
        {
            
        }


        public bool AddAuthority(string name)
        {
            if (name.Equals("CorLeonis"))
            {
                CorLeonisFirstShiftID = AddMyActivatedAbility("Cor Leonis: First Shift", "CorLeonisFirstShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can take a portion of damage for your allies. At any range, select an ally and then enter an amount of HP less than your current total to give to your ally.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisFirstShiftEntry = MyActivatedAbility(CorLeonisFirstShiftID); CorLeonisFirstShiftEntry.DisplayName = "Cor Leonis: First Shift";
                CorLeonisSecondShiftID = AddMyActivatedAbility("Cor Leonis: Second Shift", "CorLeonisSecondShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can have your allies take a portion of damage for you. If toggled active, allies with >50% HP will take 25% of damage inflicted on you in your place. The damage is then divided by the number of allies with >50% HP up to 3.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisSecondShiftEntry = MyActivatedAbility(CorLeonisSecondShiftID); CorLeonisSecondShiftEntry.DisplayName = "Cor Leonis: Second Shift";
                CorLeonisThirdShiftID = AddMyActivatedAbility("Cor Leonis: Third Shift", "CorLeonisThirdShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can synchronize effects from yourself to your allies. The following effects will transfer: Tonics, Emboldened, Frenzied, Metabolizing, Luminous, Sprinting, Adrenal Control, Ecstatic, Geometric Heal, Healing, Meditating.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisThirdShiftEntry = MyActivatedAbility(CorLeonisThirdShiftID); CorLeonisThirdShiftEntry.DisplayName = "Cor Leonis: Third Shift";
                return true;
            }
            if (name.Equals("StillnessOfTime"))
            {
                StillnessOfTimeID = AddMyActivatedAbility("Stillness Of Time", "StillnessOfTime", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can stop time. However, doing so causes strain on your body. The time of all things will be stopped for 3 turns at the cost of 30% of your total HP. If Cor Leonis: Second Shift is active and an ally has >= 50% HP, time will stop for an additional turn.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);StillnessOfTimeEntry = MyActivatedAbility(StillnessOfTimeID); //StillnessOfTimeEntry.DisplayName = "S";*/
                return true;
            }
            return false;
        }

        public bool ObtainAuthority()
        {
            List<string> MissingAuthorities = new List<string>();
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (Authorities.Contains("CorLeonis"))
            {

            }
            else
            {
                MissingAuthorities.Add("CorLeonis");
            }

            if (Authorities.Contains("StillnessOfTime"))
            {

            }
            else
            {
                MissingAuthorities.Add("StillnessOfTime");
            }

            if (MissingAuthorities.Count > 0)
            {
                int a = MODNAME_Random.Next(0, MissingAuthorities.Count -1);
                //Popup.Show(MissingAuthorities[a].ToString());

                switch (MissingAuthorities[a])
                {
                    default:
                        break;
                    case "CorLeonis":
                        AddAuthority(MissingAuthorities[a]);
                        //CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                    case "StillnessOfTime":
                        AddAuthority(MissingAuthorities[a]);
                        //CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;


                }
            }

            return false;
        }

        public void EffectDeepCopy(Effect originalEffect, GameObject target)
        {
            if (target.HasEffectByClass(originalEffect.ClassName))
            {
                target.RemoveEffectByClass(originalEffect.ClassName);
            }

            switch (originalEffect.ClassName)
            {
                case "AshPoison":
                    AshPoison ashpoison = (AshPoison)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new AshPoison(originalEffect.Duration, target));
                    break;
                case "Blaze_Tonic":
                    target.ApplyEffect(new Blaze_Tonic(originalEffect.Duration));
                    break;
                case "Bleeding":
                    Bleeding bleeding = (Bleeding)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Bleeding(bleeding.Damage.ToString(), bleeding.SaveTarget, target, bleeding.Stack));
                    break;
                case "Blind":
                    target.ApplyEffect(new Blind(originalEffect.Duration));
                    break;
                case "Burning":
                    //target.ApplyEffect(new Blind(originalEffect.Duration));
                    ParentObject.pPhysics.Temperature = ParentObject.pPhysics.AmbientTemperature;
                    target.pPhysics.Temperature = target.pPhysics.FlameTemperature;
                    break;
                case "CoatedInPlasma":
                    CoatedInPlasma coatedInPlasma = (CoatedInPlasma)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new CoatedInPlasma(originalEffect.Duration, target));
                    break;
                case "Confused":
                    Confused confused = (Confused)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Confused(originalEffect.Duration, confused.Level, confused.MentalPenalty));
                    break;
                case "Dazed":
                    target.ApplyEffect(new Dazed(originalEffect.Duration));
                    break;
                case "Disoriented":
                    Disoriented disoriented = (Disoriented)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Disoriented(originalEffect.Duration, disoriented.Level));
                    break;
                case "Ecstatic":
                    target.ApplyEffect(new Ecstatic(originalEffect.Duration));
                    break;
                case "Emboldened":
                    Emboldened emboldened = (Emboldened)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Emboldened(originalEffect.Duration, emboldened.Statistic, emboldened.Bonus));
                    break;
                case "Frenzied":
                    Frenzied frenzied = (Frenzied)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Frenzied(originalEffect.Duration, frenzied.QuicknessBonus, frenzied.MaxKillRadiusBonus, frenzied.BerserkDuration, frenzied.BerserkImmediately, frenzied.BerserkOnDealDamage, frenzied.PreferBleedingTarget));
                    break;
                case "Frozen":
                    //target.ApplyEffect(new Blind(originalEffect.Duration));
                    ParentObject.pPhysics.Temperature = ParentObject.pPhysics.AmbientTemperature;
                    target.pPhysics.Temperature = target.pPhysics.FreezeTemperature;
                    break;
                case "FungalSporeInfection":
                    FungalSporeInfection fungalSporeInfection = (FungalSporeInfection)ParentObject.GetEffect(originalEffect.DisplayName);
                    FungalSporeInfection newf = new FungalSporeInfection();
                    newf.InfectionObject = fungalSporeInfection.InfectionObject;
                    newf.Fake = fungalSporeInfection.Fake;
                    newf.TurnsLeft = fungalSporeInfection.TurnsLeft;
                    newf.Damage = fungalSporeInfection.Damage;
                    newf.bSpawned = fungalSporeInfection.bSpawned;
                    newf.Owner = target;
                    target.ApplyEffect(newf);
                    break;
                case "GeometricHeal":
                    GeometricHeal geometricHeal = (GeometricHeal)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new GeometricHeal(geometricHeal.Amount, geometricHeal.Ratio, originalEffect.Duration));
                    break;
                case "Glotrot":
                    Glotrot glotrot = (Glotrot)ParentObject.GetEffect(originalEffect.DisplayName);
                    Glotrot newg = new Glotrot();
                    newg.Stage = glotrot.Stage;
                    newg.Count = glotrot.Count;
                    newg.DrankIck = glotrot.DrankIck;
                    target.ApplyEffect(newg);
                    break;
                case "Healing":
                    target.ApplyEffect(new Healing(originalEffect.Duration));
                    break;
                case "Hoarshroom_Tonic":
                    target.ApplyEffect(new Hoarshroom_Tonic(originalEffect.Duration));
                    break;
                case "Hobbled":
                    target.ApplyEffect(new Hobbled(originalEffect.Duration));
                    break;
                case "HulkHoney_Tonic":
                    target.ApplyEffect(new HulkHoney_Tonic(originalEffect.Duration));
                    break;
                case "Ill":
                    Ill ill = (Ill)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Ill(originalEffect.Duration, ill.Level));
                    break;
                case "Ironshank":
                    Ironshank ironshank = (Ironshank)ParentObject.GetEffect(originalEffect.DisplayName);
                    Ironshank newi = new Ironshank();

                    newi.Count = ironshank.Count;
                    newi.Penalty = ironshank.Penalty;
                    newi.AVBonus = ironshank.AVBonus;
                    newi.DrankCure = ironshank.DrankCure;
                    target.ApplyEffect(newi);
                    break;
                case "Luminous":
                    target.ApplyEffect(new Luminous(originalEffect.Duration));
                    break;
                case "Meditating":
                    Meditating meditating = (Meditating)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Meditating(originalEffect.Duration, meditating.FromResting));
                    break;
                case "Monochrome":
                    target.ApplyEffect(new Monochrome());
                    break;
                case "Omniphase":
                    target.ApplyEffect(new Omniphase(originalEffect.Duration));
                    break;
                case "Paralyzed":
                    Paralyzed paralyzed = (Paralyzed)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Paralyzed(originalEffect.Duration, paralyzed.SaveTarget));
                    break;
                case "PhasePoisoned":
                    PhasePoisoned phasePoisoned = (PhasePoisoned)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new PhasePoisoned(originalEffect.Duration, phasePoisoned.DamageIncrement, phasePoisoned.Level, target));
                    break;
                case "Phased":
                    target.ApplyEffect(new Phased(originalEffect.Duration));
                    break;
                case "Poisoned":
                    Poisoned poisoned = (Poisoned)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Poisoned(originalEffect.Duration, poisoned.DamageIncrement, poisoned.Level, target));
                    break;
                case "PoisonGasPoison":
                    PoisonGasPoison poisonGasPoison = (PoisonGasPoison)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new PoisonGasPoison(originalEffect.Duration, target));
                    break;
                case "Rubbergum_Tonic":
                    target.ApplyEffect(new Rubbergum_Tonic(originalEffect.Duration));
                    break;
                case "Salve_Tonic":
                    target.ApplyEffect(new Salve_Tonic(originalEffect.Duration));
                    break;
                case "ShadeOil_Tonic":
                    target.ApplyEffect(new ShadeOil_Tonic(originalEffect.Duration));
                    break;
                case "Shaken":
                    Shaken shaken = (Shaken)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Shaken(originalEffect.Duration, shaken.Level));
                    break;
                case "ShatterArmor":
                    target.ApplyEffect(new ShatterArmor(originalEffect.Duration));
                    break;
                case "ShatterMentalArmor":
                    target.ApplyEffect(new ShatterMentalArmor(originalEffect.Duration));
                    break;
                case "Shamed":
                    target.ApplyEffect(new Shamed(originalEffect.Duration));
                    break;
                case "Skulk_Tonic":
                    target.ApplyEffect(new Skulk_Tonic(originalEffect.Duration));
                    break;
                case "GlotrotOnset":
                    target.ApplyEffect(new GlotrotOnset());
                    break;
                case "SphynxSalt_Tonic":
                    target.ApplyEffect(new SphynxSalt_Tonic(originalEffect.Duration));
                    break;
                case "SporeCloudPoison":
                    SporeCloudPoison sporeCloudPoison = (SporeCloudPoison)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new SporeCloudPoison(originalEffect.Duration, target));
                    break;
                case "IronshankOnset":
                    target.ApplyEffect(new IronshankOnset());
                    break;
                case "BasiliskPoison":
                    target.ApplyEffect(new BasiliskPoison(originalEffect.Duration, target));
                    break;
                case "LifeDrain":
                    XRL.World.Effects.LifeDrain lifeDrain = (XRL.World.Effects.LifeDrain)ParentObject.GetEffect(originalEffect.DisplayName);
                    XRL.World.Effects.LifeDrain newl = new XRL.World.Effects.LifeDrain();

                    newl.Duration = lifeDrain.Duration;
                    newl.Level = lifeDrain.Level;
                    newl.Damage = lifeDrain.Damage;
                    newl.Drainer = ParentObject;
                    target.ApplyEffect(newl);
                    break;               
                case "Ubernostrum_Tonic":
                    target.ApplyEffect(new Ubernostrum_Tonic(originalEffect.Duration));
                    break;
                case "Wakeful":
                    target.ApplyEffect(new Wakeful(originalEffect.Duration));
                    break;
                case "HulkHoney_Tonic_Allergy":
                    target.ApplyEffect(new HulkHoney_Tonic_Allergy(originalEffect.Duration));
                    break;
                case "Rubbergum_Tonic_Allergy":
                    target.ApplyEffect(new Rubbergum_Tonic_Allergy(originalEffect.Duration));
                    break;

                default:
                    //Popup.Show("Default");
                    //target.ApplyEffect(originalEffect, target);
                    break;


            }
        }

    }
    }




 