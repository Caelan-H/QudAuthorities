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
    class Lust : BaseMutation
    {
        public Guid FacelessBrideID;
        public Guid UndyingLoveRessurectionID;
        public Guid UndyingLoveSoulCaptureID;
        public Guid UndyingLoveCheckSoulID;
        public ActivatedAbilityEntry FacelessBrideEntry = null;
        public ActivatedAbilityEntry UndyingLoveRessurectionEntry = null;
        public ActivatedAbilityEntry UndyingLoveSoulCaptureEntry = null;
        public ActivatedAbilityEntry UndyingLoveCheckSoulEntry = null;
        public bool facelessBrideActive = false;
        public bool hasRessurectedAlly = false;
        public GameObject capturedSoul = null;
        public List<string> Authorities = new List<string>();
        public int AwakeningOdds = 120;
        string WitchFactor = "";
        public Lust()
        {
            DisplayName = "Lust";
            Type = "Authority";
        }

        public override string GetDescription()
        {
            return "The Lust Witch Factor.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("A dark mass hiding within your soul writhes with deep yearning....\n There is a 1/120" + " chance to awaken another Authority of Lust.");

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
            Object.RegisterPartEvent(this, "FacelessBride");
            Object.RegisterPartEvent(this, "UndyingLoveRessurection");
            Object.RegisterPartEvent(this, "UndyingLoveSoulCapture");
            Object.RegisterPartEvent(this, "UndyingLoveCheckSoul");
            base.Register(Object);
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

        public override bool HandleEvent(KilledEvent E)
        {
           
            if(E.Dying.HasEffect("SoulCaptured"))
            {
                capturedSoul = E.Dying.DeepCopy();
                capturedSoul.Heal(capturedSoul.hitpoints);
                Popup.Show(E.Dying.DisplayName);
            }
                
                
          
            

            return true;
        }

            public void BeginFacelessBride(GameObject target)
        {

            
        }

        public void BeginUndyingLoveRessurection(GameObject target)
        {


        }

        public void BeginUndyingLoveSoulCapture(GameObject target)
        {
            Popup.Show("Soul Capture Began");
            if(target.HasEffect("SoulCaptured") == false)
            {
                target.ApplyEffect(new SoulCaptured(ParentObject));
            }

        }



        public override bool FireEvent(Event E)
        {

            if (E.ID == "FacelessBride")
            {
               

                   

               
            }

            if (E.ID == "UndyingLoveRessurection")
            {

                Cell cell = PickDirection(ForAttack: true);
                if(cell.IsEmpty() == true && capturedSoul != null)
                {
                    capturedSoul.MakeActive();

                    //capturedSoul.Heal(5);
                    
                    
                    Popup.Show("Tried ressurection");
                    //GameObject newBody = GameObject.create(capturedSoul.GetSpecies());
                    cell.AddObject(capturedSoul);

                }
            }

            if (E.ID == "UndyingLoveCheckSoul")
            {
                if(capturedSoul != null)
                {
                    Popup.Show("Current the soul you captured is that of " + capturedSoul.DisplayName);
                }
                else
                {
                    Popup.Show("You don't have a captured soul.");
                }    
                

            }


            if (E.ID == "UndyingLoveSoulCapture")
            {
                
                Cell cell = PickDirection(ForAttack: false);
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
                //UseEnergy(1000, "Authority Undying Love Soul Capture");
                
                
                
                BeginUndyingLoveSoulCapture(gameObject);
            }



            return base.FireEvent(E);
        }

      

       

        public override bool CanLevel()
        {
            return false;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            int a = Stat.Random(0, 1);
            a = 0;
            if (a == 0) { FacelessBrideID = AddMyActivatedAbility("Faceless Bride", "FacelessBride", "Authority:Lust", "Awakened from the Lust Witchfactor, you gain a understanding of a way you can take on the appearance of a target.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); FacelessBrideEntry = MyActivatedAbility(FacelessBrideID); FacelessBrideEntry.DisplayName = "Faceless Bride"; }
            if (a == 0) { UndyingLoveRessurectionID = AddMyActivatedAbility("Undying Love: Ressurection", "UndyingLoveRessurection", "Authority:Lust", "Awakened from the Lust Witchfactor, you gain a understanding of a way you can reanimate the soul you are currently holding with Undying Love: Soul Capture.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); UndyingLoveRessurectionEntry = MyActivatedAbility(UndyingLoveRessurectionID); UndyingLoveRessurectionEntry.DisplayName = "Undying Love: Ressurection"; }
            if (a == 0) { UndyingLoveSoulCaptureID = AddMyActivatedAbility("Undying Love: Soul Capture", "UndyingLoveSoulCapture", "Authority:Lust", "Awakened from the Lust Witchfactor, you gain a understanding of a way you can make a claim on the soul of another living thing. The target will get the effect SoulCapture, and on their death you will recieve their soul.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); UndyingLoveSoulCaptureEntry = MyActivatedAbility(UndyingLoveSoulCaptureID); UndyingLoveSoulCaptureEntry.DisplayName = "Undying Love: Soul Capture"; }
            if (a == 0) { UndyingLoveCheckSoulID = AddMyActivatedAbility("Undying Love: Check Soul", "UndyingLoveCheckSoul", "Authority:Lust", "View information about the currently captured soul.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); UndyingLoveCheckSoulEntry = MyActivatedAbility(UndyingLoveCheckSoulID); UndyingLoveCheckSoulEntry.DisplayName = "Undying Love: Check Soul"; }

            //ActivatedAbilityThreeID = AddMyActivatedAbility("Lunar Eclipse", "LunarEclipse", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref FacelessBrideID);
            RemoveMyActivatedAbility(ref UndyingLoveRessurectionID);
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




 