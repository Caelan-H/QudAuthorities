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

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Gluttony : BaseMutation
    {
        public new Guid ActivatedAbilityID;
        public new Guid ActivatedAbilityTwoID;
        public new Guid ActivatedAbilityThreeID;
        public Guid RevertActivatedAbilityID;
        public bool DidInit = false;
  
        public Skill.BaseSkill eatenSkill = null;

        [NonSerialized]
        private long ActivatedSegment;


        public Gluttony()
        {
            DisplayName = "Gluttony";
            Type = "Mental";
        }

        public override string GetDescription()
        {
            return "The Gluttony Witch Factor";
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("You can feel an immense hunger within, and it is eager to start feasting....");
        }

        public override bool CanLevel()
        {
            return false;
        }

        public int GetCooldown(int Level)
        {
            return 5;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "StarEating");
            Object.RegisterPartEvent(this, "NameEating");
            //Object.RegisterPartEvent(this, "LunarEclipse");
            base.Register(Object);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (ID == AwardedXPEvent.ID)
            {
                //Popup.Show("Two files had the same Last Write time", true, true, true, true);
                //
            }
            return true;
        }

        public override bool FireEvent(Event E)
        {

            if (E.ID == "LunarEclipse")
            {

                Cell cell = PickDirection(ForAttack: true);
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
                bool flag = false;

                if (gameObject == ParentObject && gameObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to eat your own skill?") == DialogResult.No)
                {
                    return false;
                }
                if (gameObject == null)
                {
                    if (ParentObject.IsPlayer())
                    {
                        if (flag)
                        {
                            Popup.ShowFail("You cannot grasp the mind there sufficiently to sunder it.");
                        }
                        else
                        {
                            Popup.ShowFail("There's no target with a mind there.");
                        }
                    }
                    return false;
                }

                if (gameObject.HasEffect("MemberOfPsychicBattle") || (gameObject.HasPart("SunderMind") && gameObject.GetPart<SunderMind>().activeRounds > 0))
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.ShowFail("That target's star has already been eaten!");
                    }
                    return false;
                }

                CooldownMyActivatedAbility(ActivatedAbilityThreeID, GetCooldown(base.Level));
                UseEnergy(1000, "Mental Mutation Lunar Eclipse");
                BeginLunarEclipse(gameObject);
            }

            if (E.ID == "StarEating")
             {

                Cell cell = PickDirection(ForAttack: true);
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
                bool flag = false;
                
                if (gameObject == ParentObject && gameObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to eat your own ability?") == DialogResult.No)
                {
                    return false;
                }
                if (gameObject == null)
                {
                    if (ParentObject.IsPlayer())
                    {
                        if (flag)
                        {
                            Popup.ShowFail("You cannot grasp the mind there sufficiently to sunder it.");
                        }
                        else
                        {
                            Popup.ShowFail("There's no target with a mind there.");
                        }
                    }
                    return false;
                }
                
                if (gameObject.HasEffect("MemberOfPsychicBattle") || (gameObject.HasPart("SunderMind") && gameObject.GetPart<SunderMind>().activeRounds > 0))
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.ShowFail("That target's star has already been eaten!");
                    }
                    return false;
                }
                
                CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(base.Level));
                UseEnergy(1000, "Mental Mutation Star Eating");
                BeginStarEating(gameObject);
             }

            

            if (E.ID == "NameEating")
            {
                PickDirection(ForAttack: true);
                Cell cell = PickDirection(ForAttack: true);
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
                bool flag = false;

                if (gameObject == ParentObject && gameObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to eat your own name?") == DialogResult.No)
                {
                    return false;
                }
                if (gameObject == null)
                {
                    if (ParentObject.IsPlayer())
                    {
                        if (flag)
                        {
                            Popup.ShowFail("You cannot grasp the mind there sufficiently to sunder it.");
                        }
                        else
                        {
                            Popup.ShowFail("There's no target with a mind there.");
                        }
                    }
                    return false;
                }

                if (gameObject.HasEffect("MemberOfPsychicBattle") || (gameObject.HasPart("SunderMind") && gameObject.GetPart<SunderMind>().activeRounds > 0))
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.ShowFail("That target's star has already been eaten!");
                    }
                    return false;
                }

                CooldownMyActivatedAbility(ActivatedAbilityTwoID, GetCooldown(base.Level));
                UseEnergy(1000, "Mental Mutation Name Eating");
                BeginNameEating(gameObject);
            }
            return base.FireEvent(E);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilityID = AddMyActivatedAbility("Star Eating", "StarEating", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);
            ActivatedAbilityTwoID = AddMyActivatedAbility("Name Eating", "NameEating", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);
            //ActivatedAbilityThreeID = AddMyActivatedAbility("Lunar Eclipse", "LunarEclipse", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            return base.Unmutate(GO);
        }

        public void BeginStarEating(GameObject target)
        {
            
            if (target != null && target.HasPart("Brain") && target.GetMutationNames().Count > 0 && !target.HasEffect("StarEaten"))
            {
                
                
                if (ParentObject.IsPlayer())
                {
                  
                    
                    XRL.World.Parts.Mutations mutations = target.GetPart("Mutations") as XRL.World.Parts.Mutations;
                   // XRL.World.Parts.Skills skills = target.GetPart("Skills") as XRL.World.Parts.Skills;
                   
                    List<string> options= new List<string>();
                    List<string> skillOptions = new List<string>();
                    foreach (var item in mutations.MutationList)
                    {
                        options.Add(item.Name);
                    }
                    /*
                    foreach (var item in skills.SkillList)
                    {
                        options.Add(item.Name);
                    }
                    */
                    int index = Popup.ShowOptionList("Star Eating", options.ToArray());
                    BaseMutation mut = mutations.MutationList[index];
                    bool success = EatStar(target, mut);
                    if(success == true)
                    {
                        target.ApplyEffect(new StarEaten(0));
                        IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand eating " + target.its + " mutation!");
                    }
                    else
                    {
                        IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand but there was nothing to eat");
                    }  
                }
                else if (target.IsPlayer())
                {
                    Popup.Show(ParentObject.T() + " " + The.Player.DescribeDirectionToward(ParentObject) + ParentObject.GetVerb("burrow") + " a channel through the psychic aether and" + ParentObject.GetVerb("begin") + " to sunder your mind!");
                    AutoAct.Interrupt(null, null, ParentObject);
                }
                
            }
            else
            {
                IComponent<GameObject>.AddPlayerMessage("You tried to eat, but there was nothing to consume");
            }
        }

        public void BeginLunarEclipse(GameObject target)
        {
           

            if (target != null && target.HasPart("Brain")  && !target.HasEffect("LunarEclipsed"))
            {


                if (ParentObject.IsPlayer())
                {
                    Popup.Show(target.DisplayName);

                    XRL.World.Parts.Skills skills = target.GetPart("Skills") as XRL.World.Parts.Skills;
                    Popup.Show(target.HasPart("Skills").ToString());
                    Popup.Show(skills.SkillList.Count.ToString());
                    // XRL.World.Parts.Skills skills = target.GetPart("Skills") as XRL.World.Parts.Skills;
                    
                    List<string> options = new List<string>();
                    List<string> skillOptions = new List<string>();
                    foreach (var item in skills.SkillList)
                    {
                        Popup.Show(item.Name);
                        options.Add(item.Name);
                    }
                    /*
                    foreach (var item in skills.SkillList)
                    {
                        options.Add(item.Name);
                    }
                    */
                    int index = Popup.ShowOptionList("Lunar Eclipse", options.ToArray());
                    Skill.BaseSkill skill = skills.SkillList[index];
                    bool success = EatSkill(target, skill);
                    if (success == true)
                    {
                        //target.ApplyEffect(new StarEaten(0));
                        IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand eating " + target.its + " skill!");
                    }
                    else
                    {
                        IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand but there was nothing to eat");
                    }
                }
                else if (target.IsPlayer())
                {
                    Popup.Show(ParentObject.T() + " " + The.Player.DescribeDirectionToward(ParentObject) + ParentObject.GetVerb("burrow") + " a channel through the psychic aether and" + ParentObject.GetVerb("begin") + " to sunder your mind!");
                    AutoAct.Interrupt(null, null, ParentObject);
                }

            }
            else
            {
                IComponent<GameObject>.AddPlayerMessage("You tried to eat, but there was nothing to consume");
            }
        }

        public void BeginNameEating(GameObject target)
        {

            if (target != null && target.HasPart("Brain") && !target.HasEffect("NameEaten"))
            {


                if (ParentObject.IsPlayer())
                {
                    XRL.World.Parts.GivesRep givesRep = target.GetPart("GivesRep") as XRL.World.Parts.GivesRep;
                    XRL.World.Parts.Brain brain = target.GetPart("Brain") as XRL.World.Parts.Brain;
                    string primaryFaction = target.GetPrimaryFaction();
                    //brain.setFactionMembership(primaryFaction, -100);

                    target.ApplyEffect(new NameEaten(0));
                    target.DisplayName = "nameless";
                    int a = Stat.Random(0, 5);

                    if (a == 1)
                    {
                        JournalAPI.RevealRandomSecret();
                    }
                   
                    brain.Hostile= false;
                    /*
                    foreach (var item in Factions.getFactionNames())
                    {
                       
                        brain.setFactionFeeling(item, -1);
                        
                        

                    }
                    */
                    brain.Mindwipe();
                    //Popup.Show(brain.FactionFeelings.ToString());
                    IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand eating " + target.its + " name!");
                   
                        
                    
                }
                else if (target.IsPlayer())
                {
                    Popup.Show(ParentObject.T() + " " + The.Player.DescribeDirectionToward(ParentObject) + ParentObject.GetVerb("burrow") + " a channel through the psychic aether and" + ParentObject.GetVerb("begin") + " to sunder your mind!");
                    AutoAct.Interrupt(null, null, ParentObject);
                }

            }
            else
            {
                IComponent<GameObject>.AddPlayerMessage("You tried to eat, but they are nameless");
            }
        }

        public bool EatStar(GameObject t, BaseMutation m)
        {
            try
            {
                XRL.World.Parts.Mutations mutations = t.GetPart("Mutations") as XRL.World.Parts.Mutations;
                mutations.RemoveMutation(m);
                return true;
                
            }
            catch (Exception)
            {
                return false;
                
            }
        }

        public bool EatSkill(GameObject t, Skill.BaseSkill s)
        {
            try
            {
                XRL.World.Parts.Skills skills = t.GetPart("Skills") as XRL.World.Parts.Skills;
                skills.RemoveSkill(s);
                XRL.World.Parts.Skills playerSkills = ThePlayer.GetPart("Skills") as XRL.World.Parts.Skills;
                
                /*
                if(playerSkills.SkillList.Contains(s))
                {

                }
                else
                {
                    if(eatenSkill != null)
                    {
                        playerSkills.RemoveSkill(eatenSkill);    
                    }
                    playerSkills.AddSkill(s);
                    eatenSkill = s;
                }
                */
                return true;

            }
            catch (Exception)
            {
                return false;

            }
        }



        public void SetSkill(Skill.BaseSkill s)
        {
            eatenSkill = s;
        }
        public Skill.BaseSkill getSkill()
        {
            return eatenSkill;
        }

    }
    
   
     
    
}