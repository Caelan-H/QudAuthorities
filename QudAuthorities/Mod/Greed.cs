using System;
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
using Newtonsoft.Json;
using Steamworks;
using System.Reflection;
using XRL.World.ZoneBuilders;
using static TBComponent;
using XRL.World.AI.Pathfinding;
using NUnit.Framework.Constraints;
using XRL.UI.Framework;
using Battlehub.UIControls;
using static UnityEngine.GraphicsBuffer;
using XRL.World.Skills;

using XRL.World.Parts;
using XRL.World;
using XRL;
using NUnit.Framework;
using UnityEngine;
using XRL.EditorFormats.Screen;

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
        public int AwakeningOdds = 120;
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
            return string.Concat("A dark mass hiding within your soul writhes with avaricious intent....\n There is a 1/120" + " chance to awaken another Authority of Greed.");

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
            
                stillnessCounter = 0;
                stoppedMembers.Clear();
                stillnessOfTimeOn = false;
            
            
            return base.HandleEvent(E);
        }
        
        public override bool HandleEvent(ApplyEffectEvent E)
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
                    if (E.Effect.ClassName == "Wading" || E.Effect.ClassName == "Flying" || E.Effect.ClassName == "Beguiled" || E.Effect.ClassName == "Dominated" || E.Effect.ClassName == "MemberOfPsychicBattle" || E.Effect.ClassName == "Lost" || E.Effect.ClassName == "Overburdened" || E.Effect.ClassName == "Phasing" || E.Effect.ClassName == "PhasedWhileStuck" || E.Effect.ClassName == "PhasePoisoned" || E.Effect.ClassName == "Prone" || E.Effect.ClassName == "Submerged" || E.Effect.ClassName == "Stuck" || E.Effect.ClassName == "Swimming" || E.Effect.ClassName == "LongbladeEffect_EnGarde" || E.Effect.ClassName == "LongbladeStance_Aggressive" || E.Effect.ClassName == "LongbladeStance_Defensive" || E.Effect.ClassName == "LongbladeStance_Dueling")
                    {

                    }
                    else
                    {
                        foreach (var member in listOfPartyMembers) { member.ApplyEffect(E.Effect); }
                    }
                    
                }
                
                return true;
            }

            

            return false;
        }
        public override bool HandleEvent(BeforeApplyDamageEvent E)
        {          
            List<GameObject> listOfPartyMembers = new List<GameObject>();
            if(toBeHealed != null)
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

            if(CorLeonisSecondShiftEntry.ToggleState == true) 
            {
                int original = E.Damage.Amount;
                int half = E.Damage.Amount / 2;
                E.Damage.Amount = half;
                BeforeApplyDamageEvent e = E;
                //string attributes = E.Damage
                List<GameObject> listOfPartyMembersValid = new List<GameObject>();
                
                if(listOfPartyMembers.Count > 0) 
                {
                    foreach (var member in listOfPartyMembers)
                    {
                        if (member.GetHPPercent() <= 50)
                        {

                        }
                        else
                        {
                            
                                listOfPartyMembersValid.Add(member);
                            
                           
                        }
                    }
                }
                

                if(listOfPartyMembersValid.Count > 0) 
                {
                    int dividedDamage = half;
                    if (listOfPartyMembers.Count > 4)
                    {
                        dividedDamage = E.Damage.Amount / 4;
                    }
                    else
                    {
                        dividedDamage = E.Damage.Amount / listOfPartyMembersValid.Count;
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

            return base.HandleEvent(E);
        }
        public override bool WantEvent(int ID, int cascade)
        {
            
            //This event rolls for chances to refresh Authority cool downa and to unlock unobtained Authorities.
            if (ID == AuthorityAwakeningGreedEvent.ID)
            {
                //if (!Authorities.Contains("StarEating")) { StarEatingID = AddMyActivatedAbility("Star Eating", "StarEating", "Authority", "Awakened from the Greed Witchfactor, you gain a understanding of how to eat the powers of an opponent. At melee range, select an enemy. After doing so, you can select a Mutation to remove permanantly. After doing so, the enemy becomes StarEaten and Star Eating will no longer affect them. There is a 1/7 chance of getting a charge back. Max charge is 2.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); Authorities.Add("StarEating"); starUses = 2; StarEatingAbilityEntry = MyActivatedAbility(StarEatingID); StarEatingAbilityEntry.DisplayName = "Star Eating(" + (starUses) + " uses)"; CheckpointEvent.Send(ParentObject); }               
            }
            if (ID == AwardedXPEvent.ID)
            {
                int a = Stat.Random(0, AwakeningOdds);

                if (a == 1)
                {
                    //if(!Authorities.Contains("StarEating")) { StarEatingID = AddMyActivatedAbility("Star Eating", "StarEating", "Authority", "Awakened from the Greed Witchfactor, you gain a understanding of how to eat the powers of an opponent. At melee range, select an enemy. After doing so, you can select a Mutation to remove permanantly. After doing so, the enemy becomes StarEaten and Star Eating will no longer affect them. There is a 1/7 chance of getting a charge back. Max charge is 2.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); Authorities.Add("StarEating"); starUses = 2; StarEatingAbilityEntry = MyActivatedAbility(StarEatingID); StarEatingAbilityEntry.DisplayName = "Star Eating(" + (starUses) + " uses)"; CheckpointEvent.Send(ParentObject); }

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
                }
                else
                {
                    CorLeonisSecondShiftEntry.ToggleState = true;

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
               if(((ParentObject.GetHPPercent() > 75 && secondShiftOn == false) || (ParentObject.GetHPPercent() > 37 && secondShiftOn == true)))
               {
                    int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.75));
                    ParentObject.TakeDamage(ref damage);
                    stillnessOfTimeOn = true;
                    int cooldown = 300;
                    int duration = 3;
                    if(CorLeonisSecondShiftEntry.ToggleState == true)
                    {
                        List<GameObject> memberList = new List<GameObject>();
                        memberList.Clear();
                        foreach (var entity in ParentObject.CurrentZone.GetObjects())
                        {
                            
                            
                                if (entity.InSamePartyAs(ParentObject))
                                {
                                    
                                    memberList.Add(entity);
                                }
                            
                            
                        }
                        
                        List<GameObject> membersHelping = new List<GameObject>();
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
                        int num = membersHelping.Count;
                        if (num > 4)
                        {
                            num = 4;
                        }

                        duration = duration+ ( num / 2);
                 
                        //cooldown = cooldown - (num * 20);

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
          
                    //ParentObject.Energy.BaseValue += duration * 1000;
                    CooldownMyActivatedAbility(StillnessOfTimeID, 0);
                    UseEnergy(0, "Authority Stillness Of Time");
                    stillnessCounter = duration;
                    IComponent<GameObject>.AddPlayerMessage("There are  " + duration.ToString() + " turns left of Stillness");
                    XRLCore.ParticleManager.Add("@", ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, (float)Math.Sin((double)(float)(100) * 0.017) / 10, (float)Math.Cos((double)(float)(100) * 0.017) / 10);
                    PlayWorldSound("timestop", .2f, 0f, combat: false);
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
            int a = Stat.Random(0, 1);
            a = 0;
            if (a == 0) { CorLeonisFirstShiftID = AddMyActivatedAbility("Cor Leonis: First Shift", "CorLeonisFirstShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can take a portion of damage for your allies.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisFirstShiftEntry = MyActivatedAbility(CorLeonisFirstShiftID); CorLeonisFirstShiftEntry.DisplayName = "Cor Leonis: First Shift"; }
            if (a == 0) { CorLeonisSecondShiftID = AddMyActivatedAbility("Cor Leonis: Second Shift", "CorLeonisSecondShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can have your allies take a portion of damage for you.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisSecondShiftEntry = MyActivatedAbility(CorLeonisSecondShiftID); CorLeonisSecondShiftEntry.DisplayName = "Cor Leonis: Second Shift"; }
            if (a == 0) { CorLeonisThirdShiftID = AddMyActivatedAbility("Cor Leonis: Third Shift", "CorLeonisThirdShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can synchronize effects from yourself to your allies.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisThirdShiftEntry = MyActivatedAbility(CorLeonisThirdShiftID); CorLeonisThirdShiftEntry.DisplayName = "Cor Leonis: Third Shift"; }
            if (a == 0) { StillnessOfTimeID = AddMyActivatedAbility("Stillness Of Time", "StillnessOfTime", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can stop time. However, doing so places extreme strain on your body. \n The time of all things will be stopped for 3 turns at the cost of 75%", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);/* StillnessOfTimeEntry = MyActivatedAbility(StillnessOfTimeID); StillnessOfTimeEntry.DisplayName = "S";*/ }

            //ActivatedAbilityThreeID = AddMyActivatedAbility("Lunar Eclipse", "LunarEclipse", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref CorLeonisFirstShiftID);
            RemoveMyActivatedAbility(ref CorLeonisSecondShiftID);
            RemoveMyActivatedAbility(ref CorLeonisThirdShiftID);
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

    }
    }




 