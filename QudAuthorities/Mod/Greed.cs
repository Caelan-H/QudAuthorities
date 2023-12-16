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
        public int AwakeningOdds = 199;
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
            return string.Concat("A dark mass hiding within your soul writhes with avaricious intent....\n There is a 1/200" + " chance to awaken another Authority of Greed. The Authorities are: Cor Leonis and Stillness Of Time. Intelligence +1.");

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
                int a = Stat.Random(0, AwakeningOdds);

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

                if (((ParentObject.GetHPPercent() > 30 ) || ParentObject.GetHPPercent() > 15 && membersHelping.Count > 0 && secondShiftOn))
                {
                    
                    int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.30));
                    
                    
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
                    IComponent<GameObject>.AddPlayerMessage("There are  " + duration.ToString() + " turns left of Stillness");
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
               ParentObject.GainIntelligence(1);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref CorLeonisFirstShiftID);
            RemoveMyActivatedAbility(ref CorLeonisSecondShiftID);
            RemoveMyActivatedAbility(ref CorLeonisThirdShiftID);
            RemoveMyActivatedAbility(ref StillnessOfTimeID);
            ParentObject.GainIntelligence(-1);
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
                CorLeonisSecondShiftID = AddMyActivatedAbility("Cor Leonis: Second Shift", "CorLeonisSecondShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can have your allies take a portion of damage for you. If toggled active, allies with >50% HP will take 25% of damage you take for you. The damage is then divided by the number of allies with >50% HP up to 3.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisSecondShiftEntry = MyActivatedAbility(CorLeonisSecondShiftID); CorLeonisSecondShiftEntry.DisplayName = "Cor Leonis: Second Shift";
                CorLeonisThirdShiftID = AddMyActivatedAbility("Cor Leonis: Third Shift", "CorLeonisThirdShift", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can synchronize effects from yourself to your allies.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); CorLeonisThirdShiftEntry = MyActivatedAbility(CorLeonisThirdShiftID); CorLeonisThirdShiftEntry.DisplayName = "Cor Leonis: Third Shift";
                return true;
            }
            if (name.Equals("StillnessOfTime"))
            {
                StillnessOfTimeID = AddMyActivatedAbility("Stillness Of Time", "StillnessOfTime", "Authority:Greed", "Awakened from the Greed Witchfactor, you gain a understanding of a way you can stop time. However, doing so causes strain on your body. The time of all things will be stopped for 3 turns at the cost of 30% of your total HP or 15% if Cor Leonis: Second Shift is active as long as you have greater than 30% HP and greater than 15% HP respectively.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);/* StillnessOfTimeEntry = MyActivatedAbility(StillnessOfTimeID); StillnessOfTimeEntry.DisplayName = "S";*/
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
                int a = Stat.Random(0, MissingAuthorities.Count - 1);
                //Popup.Show(MissingAuthorities[a].ToString());

                switch (MissingAuthorities[a])
                {
                    default:
                        break;
                    case "CorLeonis":
                        AddAuthority(MissingAuthorities[a]);
                        CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                    case "StillnessOfTime":
                        AddAuthority(MissingAuthorities[a]);
                        CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;


                }
            }

            return false;
        }

    }
    }




 