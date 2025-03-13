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
using System.Collections.ObjectModel;
using MODNAME.Utilities;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Wrath : BaseMutation
    {
        public Guid UnjustWorldID;
        public Guid SoulwashingID;
        public int soulwashes = 0;
        public ActivatedAbilityEntry UnjustWorldEntry = null;
        public ActivatedAbilityEntry SoulwashingEntry = null;
        public GameObject toBeHealed = null;
        public bool UnjustWorldOn = false;
        public List<string> Authorities = new List<string>();
        public int AwakeningOdds = 549;
        string WitchFactor = "";
        public Wrath()
        {
            DisplayName = "Wrath";
            Type = "Authority";
        }

        public override string GetDescription()
        {
            return "The Wrath Witch Factor.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("A dark mass hiding within your soul writhes with unbound rage and madness....\n There is a 1/550" + " chance to awaken another Authority of Wrath upon gaining XP. The Authorities are: Unjust World and Soulwash.");

            /*
            if (Authorities.Count == 0 || Authorities.Count == 1)
            {
            }
            else
            {
                if (Authorities.Count > 1)
                {
                    return string.Concat("The dark impurity within you that yearned to amass has changed form. You can feel gentle light within your soul where the darkness used to creep into your very being. It is eager to endow others with it's charity. The potential of Wrath is fully realized.");
                }
                return string.Concat("Something is wrong with the Wrath authority count!");
            }

            */

        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "UnjustWorld");
            Object.RegisterPartEvent(this, "Soulwash");        
            base.Register(Object);
        }

        public override bool HandleEvent(AttackerDealingDamageEvent E)
        {
            int damage = E.Damage.Amount;
            if(Authorities.Contains("UnjustWorld"))
            {
                if (UnjustWorldEntry.ToggleState == true)
                {
                    int toHeal = E.Damage.Amount;
                    //Popup.Show("heal");
                    E.Object.Heal(toHeal, false, true);
                    E.Damage.Amount = 0;
                    return true;
                }
                else
                {
                    E.Damage.Amount = damage;
                    return true;
                }
            }

            return true;
        }


        public override bool HandleEvent(ApplyEffectEvent E)
        {


            



            return true;
        }
       
        public override bool WantEvent(int ID, int cascade)
        {

            //This event rolls for chances to refresh Authority cool downa and to unlock unobtained Authorities.
            if (ID == AuthorityAwakeningWrathEvent.ID)
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
                    AuthorityAwakeningWrathEvent.Send(ParentObject);
                }

                int b = MODNAME_Random.Next(0, 75);

                if(b == 1)
                {
                    if(soulwashes >= 2)
                    {

                    }
                    else
                    {
                        soulwashes++;
                        SyncAbilityName_Soulwashing();
                    }
                }


            }
            return true;
        }



        public void BeginSoulwash()
        {
            bool worked = false;
            foreach (var entity in ParentObject.CurrentZone.GetObjects())
            {
                if (entity != null && entity.HasPart("Brain"))
                {

                    //int dist = entity.DistanceTo(ParentObject);
                   // if (dist <= 6)
                   // {
                        
                            if (entity.IsHostileTowards(ParentObject) == true && entity.InSamePartyAs(ParentObject) == false && entity != ParentObject)
                            {
                                worked = true;
                                
                                if (!entity.HasEffect("Terrified")) { entity.ApplyEffect(new Terrified(5, ParentObject)); }
                                if (!entity.HasEffect("Dazed")) { entity.ApplyEffect(new Dazed(5)); }
                                if (!entity.HasEffect("Disoriented")) { entity.ApplyEffect(new Disoriented(5, 2)); }
                                if (!entity.HasEffect("Hobbled")) { entity.ApplyEffect(new Hobbled(5)); }
                                if (!entity.HasEffect("Shamed")) { entity.ApplyEffect(new Shamed(5)); }
                                if (!entity.HasEffect("Shaken")) { entity.ApplyEffect(new Shaken(5, 2)); }
                            }
                        
                   // }

                   
                }
            }

            if(worked == true)
            {
                soulwashes--;
                SyncAbilityName_Soulwashing();
            }
        }



        public override bool FireEvent(Event E)
        {

            if (E.ID == "Soulwash")
            {

                /*
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
                */
                if (soulwashes <= 0)
                {
                    
                    if (ParentObject.IsPlayer())
                    {

                        Popup.ShowFail("You have no Soulwash charges.");

                    }
                    return false;
                }
                
                if (ParentObject.OnWorldMap())
                {
                    Popup.ShowFail("You cannot use this on the world map");
                    return false;
                }

                UseEnergy(1000, "Authority Mutation Cor Leonis: First Shift");
                    BeginSoulwash();
                



            }

            if (E.ID == "UnjustWorld")
            {
                if (UnjustWorldEntry.ToggleState == true)
                {
                    UnjustWorldEntry.ToggleState = false;
                    UnjustWorldOn = false;
                }
                else
                {
                    UnjustWorldEntry.ToggleState = true;
                    UnjustWorldOn = true;

                }

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
            //ParentObject.GetStat("Strength").BaseValue += 1;
            //ParentObject.GetStat("Agility").BaseValue += 1;
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref UnjustWorldID);
            RemoveMyActivatedAbility(ref SoulwashingID);
            //ParentObject.GetStat("Strength").BaseValue -= 1;
            //ParentObject.GetStat("Agility").BaseValue -= 1;
            
            RecountEvent.Send(ParentObject);
            return base.Unmutate(GO);
        }

        public void SyncAbilityName_Soulwashing()
        {
           SoulwashingEntry.DisplayName = "Soulwash[" + soulwashes.ToString() + "/2]";

        }

        public void SyncAbilityName_CorLeonisSecondShift()
        {

        }

        public void SyncAbilityName_CorLeonisThirdShift()
        {

        }


        public bool AddAuthority(string name)
        {
            if (name.Equals("UnjustWorld"))
            {
                UnjustWorldID = AddMyActivatedAbility("Unjust World", "UnjustWorld", "Authority:Wrath", "Awakened from the Wrath Witchfactor, you've awakened the ability to use your boundless rage into mending the world. When toggled on, damage dealt by the user will instead heal the target.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); UnjustWorldEntry = MyActivatedAbility(UnjustWorldID); UnjustWorldEntry.DisplayName = "Unjust World";
                return true;
            }
            if (name.Equals("Soulwashing"))
            {
                soulwashes = 2;
                SoulwashingID = AddMyActivatedAbility("Soulwash", "Soulwash", "Authority:Wrath", "Awakened from the Wrath Witchfactor, you become aware of a method to bathe a target's soul with madness. On use, all hostile entities towards the user will be inflicted with Terrified, Dazed, Disoriented, Hobbled, Shamed, and Shaken for 5 turns. The max amount of charges is 2 and you get charges back at a chance of 1/75 whenever you get xp.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true); SoulwashingEntry = MyActivatedAbility(SoulwashingID);  SoulwashingEntry.DisplayName = "Soulwash[" + soulwashes.ToString() + "/2]";
                return true;
            }
            return false;
        }

        public bool ObtainAuthority()
        {
            List<string> MissingAuthorities = new List<string>();
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (Authorities.Contains("UnjustWorld"))
            {

            }
            else
            {
                MissingAuthorities.Add("UnjustWorld");
            }

            if (Authorities.Contains("Soulwashing"))
            {

            }
            else
            {
                MissingAuthorities.Add("Soulwashing");
            }

            if (MissingAuthorities.Count > 0)
            {
                int a = MODNAME_Random.Next(0, MissingAuthorities.Count - 1);
                //Popup.Show(MissingAuthorities[a].ToString());

                switch (MissingAuthorities[a])
                {
                    default:
                        break;
                    case "UnjustWorld":
                        AddAuthority(MissingAuthorities[a]);
                        //CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                    case "Soulwashing":
                        AddAuthority(MissingAuthorities[a]);
                        //CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;


                }
            }

            return false;
        }

    }
}




