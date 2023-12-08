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

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Gluttony : BaseMutation
    {
        public Guid StarEatingID;
        public Guid GluttonousEatingID;
        public ActivatedAbilityEntry StarEatingAbilityEntry = null;
        public ActivatedAbilityEntry GluttonousEatingAbilityEntry = null;
        public List<string> Authorities = new List<string>();
        public int GluttonousUses = 0;
        public int starUses = 0;
        public int AwakeningOdds = 120;
        string WitchFactor = "";
        
        public Gluttony()
        {
            DisplayName = "Gluttony";
            Type = "Authority";
        }

        public override string GetDescription()
        {
            return "The Gluttony Witch Factor. Within it the potential to feast upon the minds of others lurks.";
        }

        public override string GetLevelText(int Level)
        {

            return string.Concat("A starving dark mass hiding within your soul writhes and squirms as it begs to be fed....\n There is a 1/120" + " chance to awaken another Authority of Gluttony.");

            /*
            if(Authorities.Count == 0 || Authorities.Count == 1)
            {
            }
            else
            {
                if (Authorities.Count > 1)
                {
                    return string.Concat("The dark impurity within you that yearned to feast has changed form. You can feel gentle light within your soul where the darkness used to creep into your very being. It awaits more feasting with temperance. The potential of Gluttony is fully realized.");
                }
                return string.Concat("Something is wrong with the gluttony authority count!");
            }
            
            */

        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "StarEating");
            Object.RegisterPartEvent(this, "GluttonousEating");
            Object.RegisterPartEvent(this, "AuthorityAwakening");
            base.Register(Object);
        }

        public override bool WantEvent(int ID, int cascade)
        {

            //This event rolls for chances to refresh Authority cool downa and to unlock unobtained Authorities.
            if (ID == AuthorityAwakeningGluttonyEvent.ID)
            {
                if (ParentObject.IsPlayer())
                {
                    bool didGetAuthority = ObtainAuthority();
                }
                
            }
            if (ID == AwardedXPEvent.ID)
            {
                int a = Stat.Random(0, 5);
                int b = Stat.Random(0, 7);
                int c = Stat.Random(0, 10);
                if (a == 1)
                {
                    AuthorityAwakeningGluttonyEvent.Send(ParentObject);  
                }
                if (b == 5)
                {
                    if(Authorities.Contains("StarEating"))
                    {
                        if(starUses < 2)
                        {
                            
                            starUses++;
                            SyncAbilityName_StarEating();
                        }
                    }
                }
                if (c == 5)
                {
                    if (Authorities.Contains("GluttonousEating"))
                    {
                        if (GluttonousUses < 1)
                        {
                            GluttonousUses++;
                            SyncAbilityName_GluttonousEating();
                        }
                    }
                }
                //Popup.Show("Two files had the same Last Write time", true, true, true, true);
                //
            }
            return true;
        }

        
       

        public override bool FireEvent(Event E)
        {

            

            if (E.ID == "StarEating" && starUses > 0)
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
                UseEnergy(1000, "Mental Mutation Star Eating");
                BeginStarEating(gameObject);
             }
            else
            {
               //Popup.Show("Star Eating isn't ready for use yet");
            }
          
            if (E.ID == "GluttonousEating" && GluttonousUses > 0)
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
           

                if (gameObject == ParentObject && gameObject.IsPlayer())
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
                UseEnergy(1000, "Mental Mutation Gluttonous Eating");
                BeginGluttonousEating(gameObject);
            }
            return base.FireEvent(E);
        }
        public override bool CanLevel()
        {
            return false;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            
            
                ObtainAuthority();
                      
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if(Authorities.Contains("StarEating"))
            {
                RemoveMyActivatedAbility(ref StarEatingID);
            }
            if (Authorities.Contains("GluttonousEating"))
            {
                RemoveMyActivatedAbility(ref StarEatingID);
            }

            RemoveMyActivatedAbility(ref GluttonousEatingID);
            return base.Unmutate(GO);
        }

        public void BeginStarEating(GameObject target)
        {
            
            if (target != null && target.HasPart("Brain") && target.GetMutationNames().Count > 0 && !target.HasEffect("StarEaten") )
            {     
                if (ParentObject.IsPlayer())
                {
                    XRL.World.Parts.Mutations mutations = target.GetPart("Mutations") as XRL.World.Parts.Mutations;                  
                    List<string> options= new List<string>();
                    List<string> skillOptions = new List<string>();
                    foreach (var item in mutations.MutationList)
                    {
                        options.Add(item.Name);
                    }
                    options.Add("Cancel");
                    int index = Popup.ShowOptionList("Star Eating", options.ToArray());
                    bool success = false;
                    if (options[index].Equals("Cancel"))
                    {
                        success = false;
                    }
                    else
                    {
                        BaseMutation mut = mutations.MutationList[index];
                        success = EatStar(target, mut);
                        if(success)
                        {
                            if(mut.DisplayName.Equals("Astral"))
                            {
                                if(target.HasEffectByClass("Phased"))
                                {
                                    target.RemoveEffectByClass("Phased");
                                }
                                
                            }
                        }
                    }
                    
                    if(success == true)
                    {
                        starUses--;
                        
                        SyncAbilityName_StarEating();
                        target.ApplyEffect(new StarEaten(0));
                        IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand eating " + target.its + " mutation!");
                    }
                    else
                    {
                        if (options[index].Equals("Cancel"))
                        {
                            IComponent<GameObject>.AddPlayerMessage("You decided not to eat.");
                        }
                        else
                        {
                            IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand but there was nothing to eat");
                        }
                        
                    }  
                }
                else if (target.IsPlayer())
                {
                    
                }
                
            }
            else
            {
                IComponent<GameObject>.AddPlayerMessage("You tried to eat, but there was nothing to consume");
            }
        }

        

        public void BeginGluttonousEating(GameObject target)
        {

            if (target != null && target.HasPart("Brain") && !target.HasEffect("GluttonousEaten") )
            {


                if (ParentObject.IsPlayer())
                {
                    XRL.World.Parts.GivesRep givesRep = target.GetPart("GivesRep") as XRL.World.Parts.GivesRep;
                    XRL.World.Parts.Brain brain = target.GetPart("Brain") as XRL.World.Parts.Brain;
                   
                    
                    string primaryFaction = target.GetPrimaryFaction();
                    //brain.setFactionMembership(primaryFaction, -100);
                    int Level = (int)Math.Min(10.0, Math.Floor((double)(30 - 1) / 2.0 + 3.0));
                    int Penalty = (int)Math.Min(10.0, Math.Floor((double)(30 - 1) / 2.0 + 3.0));
                    target.ApplyEffect(new GluttonousEaten(0));
                    target.ApplyEffect(new Confused( 8,  Level, Penalty));
                    target.DisplayName = "nameless";
                    int a = Stat.Random(0, 3);
                    GluttonousUses--;
                    SyncAbilityName_GluttonousEating();
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
                    brain.ClearHostileMemory();
                    brain.FactionMembership.Clear();
                    brain.FactionFeelings.Clear();
                    brain.Factions = "";
                    //Popup.Show(brain.FactionFeelings.ToString());
                    IComponent<GameObject>.AddPlayerMessage("You say the name of " + target.t() + " and lick your hand eating " + target.its + " mind!");
                   
                        
                    
                }
                else if (target.IsPlayer())
                {
                    
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

        



        
        public void SyncAbilityName_StarEating()
        {
            

            if (Authorities.Contains("StarEating") )
            {
               
                StarEatingAbilityEntry.DisplayName = "Star Eating(" + (starUses) + " uses)";
            }
            
        }

        public void SyncAbilityName_GluttonousEating()
        {
            

            if (Authorities.Contains("GluttonousEating"))
            {             
                GluttonousEatingAbilityEntry.DisplayName = "Gluttonous Eating(" + (GluttonousUses) + " uses)";
            }
            
        }

        public bool AddAuthority(string name)
        {
            if(name.Equals("StarEating"))
            {
                StarEatingID = AddMyActivatedAbility("Star Eating", "StarEating", "Authority:Gluttony", "Awakened from the Gluttony Witchfactor, you gain a understanding of how to eat the powers of an opponent. At melee range, select an enemy. After doing so, you can select a Mutation to remove permanantly. After doing so, the enemy becomes StarEaten and Star Eating will no longer affect them. There is a 1/7 chance of getting a charge back. Max charge is 2.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); Authorities.Add("StarEating"); starUses = 2; StarEatingAbilityEntry = MyActivatedAbility(StarEatingID); StarEatingAbilityEntry.DisplayName = "Star Eating(" + (starUses) + " uses)"; CheckpointEvent.Send(ParentObject);
                return true;
            }
            if (name.Equals("GluttonousEating"))
            {
                GluttonousEatingID = AddMyActivatedAbility("Gluttonous Eating", "GluttonousEating", "Authority:Gluttony", "Awakened from the Gluttony Witchfactor, you gain a understanding of how to eat the memories of an opponent. At melee range, select an enemy. After doing so, their mind will be wiped and the enemy becomes GluttonousEaten meaning Gluttonous Eating will no longer affect them. The enemy is confused for 8 turns. There is a 1/10 chance when getting xp to get a charge back, and a 1/3 chance to gain random knowledge on use. Max charge is 1.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); Authorities.Add("GluttonousEating"); GluttonousUses = 1; GluttonousEatingAbilityEntry = MyActivatedAbility(GluttonousEatingID); GluttonousEatingAbilityEntry.DisplayName = "Gluttonous Eating(" + (GluttonousUses) + " uses)";
                return true;
            }
            return false;
        }

        public bool ObtainAuthority()
        {
            List<string> MissingAuthorities = new List<string>();
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (Authorities.Contains("GluttonousEating"))
            {

            }
            else
            {
                MissingAuthorities.Add("GluttonousEating");
            }

            if (Authorities.Contains("StarEating"))
            {

            }
            else
            {
                MissingAuthorities.Add("StarEating");
            }

            if (MissingAuthorities.Count > 0)
            {
                int a = Stat.Random(0, MissingAuthorities.Count - 1);
                //Popup.Show(MissingAuthorities[a].ToString());

                switch (MissingAuthorities[a])
                {
                    default:
                        break;
                    case "StarEating":
                        AddAuthority(MissingAuthorities[a]);
                        CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                    case "GluttonousEating":
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




/*
 * 
 * public bool EatSkill(GameObject t, Skill.BaseSkill s)
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
                
return true;

            }
            catch (Exception)
{
    return false;

}
        }
 * 
 *
 * if (E.ID == "LunarEclipse")
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

 * 
 * 
 * public void BeginLunarEclipse(GameObject target)
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
                    
int index = Popup.ShowOptionList("Lunar Eclipse", options.ToArray());
Skill.BaseSkill skill = skills.SkillList[index];
bool success = EatSkill(target, skill);
if (success == true)
{
    //target.ApplyEffect(new StarEaten(0));
    nameUses--;
    SyncAbilityName_NameEating();
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








public void SetSkill(Skill.BaseSkill s)
        {
            eatenSkill = s;
        }
        public Skill.BaseSkill getSkill()
        {
            return eatenSkill;
        }
 */