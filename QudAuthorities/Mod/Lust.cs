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
using System.CodeDom;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Lust : BaseMutation
    {
        public Guid FacelessBrideID;
        public Guid HeavensFeelRessurectionID;
        public Guid HeavensFeelSoulCaptureID;
        public Guid HeavensFeelCheckSoulID;
        public string UndeadID;
        public ActivatedAbilityEntry FacelessBrideEntry = null;
        public ActivatedAbilityEntry HeavensFeelRessurectionEntry = null;
        public ActivatedAbilityEntry HeavensFeelSoulCaptureEntry = null;
        public ActivatedAbilityEntry HeavensFeelCheckSoulEntry = null;
        public bool facelessBrideActive = false;
        public string FacelessBrideFaction = "";
        public bool hasRessurectedAlly = false;
        public List<string> Authorities = new List<string>();
        public GameObject capturedSoul = null;
        public GameObject capturedSoulDeepCopy = null;
        public int AwakeningOdds = 119;
        public int soulDecay = 0;
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
            return string.Concat("A dark mass hiding within your soul writhes with deep yearning....\n There is a 1/120" + " chance to awaken another Authority of Lust. The Authorities are: Heaven's Feel and Faceless Bride. Reputation +50 with all factions.");

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
            Object.RegisterPartEvent(this, "HeavensFeelRessurection");
            Object.RegisterPartEvent(this, "HeavensFeelSoulCapture");
            Object.RegisterPartEvent(this, "HeavensFeelCheckSoul");
            base.Register(Object);
        }

       
        public override bool WantEvent(int ID, int cascade)
        {
            
            //This event rolls for chances to refresh Authority cool downa and to unlock unobtained Authorities.
            if (ID == AuthorityAwakeningLustEvent.ID)
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
                    AuthorityAwakeningLustEvent.Send(ParentObject);
                }


            }
            return true;
        }

        public override bool HandleEvent(AttackerDealingDamageEvent E)
        {
            //Popup.Show("AttackerDealingDamageEvent!" + " The attack was: " + E.Actor.ToString() + " made with a " + E.Weapon.ToString());
            if(E.Damage.Amount >= E.Object.hitpoints)
            {
                //Popup.Show(E.Object.DisplayName + " has SoulCaptured?: " + E.Object.HasEffect("SoulCaptured").ToString());

                if (E.Object.HasEffect("SoulCaptured"))
                {
                    IComponent<GameObject>.AddPlayerMessage("You have absorbed the soul of " + E.Object.DisplayName);
                    capturedSoul = E.Object.DeepCopy();
                    capturedSoul.MakeInactive();
                    //Popup.Show(E.Object.DisplayName + " has been soul captured.");
                    soulDecay = 0;
                }
                return true;
            }
            return true;
        }

        /*
        public override bool HandleEvent(KilledEvent E)
        {
            Popup.Show(E.Dying.DisplayName + " has SoulCaptured?: " + E.Dying.HasEffect("SoulCaptured").ToString());
           
            if(E.Dying.HasEffectByClass("SoulCaptured"))
            {
                capturedSoul = E.Dying.DeepCopy();
                capturedSoul.MakeInactive();         
                Popup.Show(E.Dying.DisplayName + " has been soul captured.");
                soulDecay = 0;
            }
            return true;
        }
        */
        public void BeginFacelessBride(GameObject target)
        {
            string faction = target.GetPrimaryFaction();
            bool worked = true;
            
            if (faction == null || Factions.getFactionNames().Contains(faction) == false || target.HasEffect("GluttonousEaten"))
            {
                IComponent<GameObject>.AddPlayerMessage("Your target was factionless and you were unable to use your Authority");
            }
            else
            {
                if (!facelessBrideActive)
                {
                    IComponent<GameObject>.AddPlayerMessage("All members of the faction " + Factions.get(faction).DisplayName + " have their minds distorted and think you look like them due to your Authority.");
                    XRLCore.Core.Game.PlayerReputation.modify(faction, 100);
                    facelessBrideActive = true;
                    FacelessBrideFaction = faction;
                }
                else
                {
                    if(faction.Equals(FacelessBrideFaction))
                    {
                        IComponent<GameObject>.AddPlayerMessage("Your target is already under the effects of Faceless Bride.");
                        worked = false;
                    }
                    else
                    {
                        XRLCore.Core.Game.PlayerReputation.modify(FacelessBrideFaction, -75);
                        XRLCore.Core.Game.PlayerReputation.modify(faction, 100);
                        IComponent<GameObject>.AddPlayerMessage("All members of the faction " + Factions.get(faction).DisplayName + " have their minds distorted and think you look like them due to your Authority. The distortion over the minds of faction " + Factions.get(FacelessBrideFaction).DisplayName + " has been undone.");
                        facelessBrideActive = true;
                        FacelessBrideFaction = faction;
                    }
                    
                }
                if(worked)
                {
                    CooldownMyActivatedAbility(FacelessBrideID, 300);
                    FacelessBrideEntry.Description = "Awakened from the Lust Witchfactor, you gain a understanding of a way you can take make others perceive you in their perferred form. Pick a target at melee range and you will get +100 reputation with them. If you already had a faction under the effect of Faceless Bride, the effect is removed on the former faction. You are currently using Faceless Bride on the faction:" + Factions.get(FacelessBrideFaction).DisplayName + ".";

                }
                else
                {

                }
            }
            
            
        }


        public override bool HandleEvent(ZoneActivatedEvent E)
        {
            
            if(capturedSoulDeepCopy.HasPart("Brain") && capturedSoulDeepCopy != null)
            {
                XRL.World.Parts.Brain brain = capturedSoulDeepCopy.GetPart("Brain") as XRL.World.Parts.Brain;
                
                brain.Staying = false;
            }
           


            return base.HandleEvent(E);
        }

        public void BeginHeavensFeelRessurection(GameObject target)
        {
            //Popup.Show("BeginHeavensFeelRessurection");
            Cell cell = PickDirection(ForAttack: true);
            GameObject gameObject;
            bool canRes = true;
            string failString = "";

            if(capturedSoulDeepCopy == null)
            {
                if(cell.IsEmpty() == false)
                {
                    canRes = false;
                    failString = "Cell must be empty for ressurection if you do not have an undead summoned.";
                }
            }
            else
            {
                
                gameObject = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
                if (capturedSoulDeepCopy.IsAlive == true && !capturedSoulDeepCopy.DisplayName.Equals("*PooledObject"))
                {
                    if(gameObject == capturedSoulDeepCopy)
                    {
                        capturedSoulDeepCopy.Die();
                        canRes= true;
                    }
                    else
                    {
                        failString = "Cell must be occupied by the current undead in order to perform ressurection.";
                        canRes = false;
                    }
                    
                }
                else
                {
                    if (cell.IsEmpty() == false)
                    {
                        canRes = false;
                        failString = "Cell must be empty for ressurection if you do not have an undead summoned.";
                    }
                }
            }

            if(canRes== false)
            {
                Popup.ShowFail(failString);
            }
            else
            {
                if(capturedSoulDeepCopy != null)
                {
                    capturedSoulDeepCopy.Destroy("", Silent: false, Obliterate: false, "");
                    capturedSoulDeepCopy = null;
                }
                capturedSoulDeepCopy = capturedSoul.DeepCopy();
                capturedSoulDeepCopy.ForeachEquipmentAndCybernetics(Unequip);
                capturedSoulDeepCopy.Inventory.Clear();
                capturedSoulDeepCopy.Heal(10000);
                capturedSoulDeepCopy.ApplyEffect(new Undead());
                capturedSoulDeepCopy.MakeActive();
                capturedSoulDeepCopy.SetPartyLeader(ParentObject);
                capturedSoulDeepCopy.BecomeCompanionOf(ParentObject);
                cell.AddObject(capturedSoulDeepCopy);
            }
        }

        public void Unequip(GameObject equipment)
        {
            equipment.ForceUnequipAndRemove();
        }

        public void BeginHeavensFeelSoulCapture(GameObject target)
        {
            //Popup.Show("Soul Capture Began");
            if(target.HasEffect("Undead"))
            {
                IComponent<GameObject>.AddPlayerMessage("You synchronize your undead with your captured soul");
                if(capturedSoul != null)
                {
                    capturedSoul.Destroy("", Silent: false, Obliterate: false, "");
                    capturedSoul = null;
                }
                capturedSoul = target.DeepCopy();
                capturedSoul.MakeInactive();
            }
            else
            {
                if (target.HasEffect("SoulCaptured") == false)
                {
                    IComponent<GameObject>.AddPlayerMessage("Your target has been marked for soul capture");
                    target.ApplyEffect(new SoulCaptured());
                }
                else
                {
                    IComponent<GameObject>.AddPlayerMessage("The target's soul has been marked for soul capture already");
                }
            }
            

        }



        public override bool FireEvent(Event E)
        {

            if (E.ID == "FacelessBride")
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
                if (gameObject.HasPart("Brain"))
                {
                    XRL.World.Parts.Brain brain = gameObject.GetPart("Brain") as XRL.World.Parts.Brain;
                    if (brain.Mobile == true)
                    {
                        //UseEnergy(1000, "Authority Heaven's Feel Soul Capture");
                        BeginFacelessBride(gameObject);
                    }
                    else
                    {
                        Popup.ShowFail("Soul Capture only works on mobile enemies.");
                    }
                }
                else
                {
                    Popup.ShowFail("Soul Capture only works on enemies with Brains.");
                }


            }

            if (E.ID == "HeavensFeelRessurection")
            {

                if(capturedSoul == null)
                {
                    Popup.Show("You do not have a soul captured");
                }
                else
                {

                    BeginHeavensFeelRessurection(capturedSoulDeepCopy);
                }
            
                

                
            }

            if (E.ID == "HeavensFeelCheckSoul")
            {
                
                if(capturedSoul != null)
                {
                    Popup.Show("Currently the soul you captured is that of " + capturedSoul.DisplayName);
                }
                else
                {
                    Popup.Show("You don't have a captured soul.");
                }    
                

            }


            if (E.ID == "HeavensFeelSoulCapture")
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
                if(gameObject.HasPart("Brain"))
                {
                    XRL.World.Parts.Brain brain = gameObject.GetPart("Brain") as XRL.World.Parts.Brain;
                    if(brain.Mobile == true)
                    {
                        UseEnergy(1000, "Authority Heaven's Feel Soul Capture");
                        BeginHeavensFeelSoulCapture(gameObject);
                    }
                    else
                    {
                        Popup.ShowFail("Soul Capture only works on mobile enemies.");
                    }
                }
                else
                {
                    Popup.ShowFail("Soul Capture only works on enemies with Brains.");
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
            
           foreach (var item in Factions.getFactionNames())
            {             
                
                XRLCore.Core.Game.PlayerReputation.modify(item, 50,false);
            }
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref FacelessBrideID);
            RemoveMyActivatedAbility(ref HeavensFeelRessurectionID);
            RemoveMyActivatedAbility(ref HeavensFeelSoulCaptureID);
            RemoveMyActivatedAbility(ref HeavensFeelCheckSoulID);
            foreach (var faction in Factions.getFactionNames())
            {
                XRLCore.Core.Game.PlayerReputation.modify(faction, -50);
            }
            return base.Unmutate(GO);
        }


        public bool AddAuthority(string name)
        {
            if (name.Equals("HeavensFeel"))
            {
                HeavensFeelRessurectionID = AddMyActivatedAbility("Heaven's Feel: Ressurection", "HeavensFeelRessurection", "Authority:Lust", "Awakened from the Lust Witchfactor, you gain a understanding of a way you can reanimate the soul you are currently holding with Heaven's Feel: Soul Capture. Select a space in melee range and they will be summoned in that space. If you already have an undead, you must use ressurection on the cell the undead is currently in to replace them with your currently held soul. The former undead will be killed and the new one will take their place on the space. You may only have one undead at a time. The same soul can be used to resummon the undead endlessly.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); HeavensFeelRessurectionEntry = MyActivatedAbility(HeavensFeelRessurectionID); HeavensFeelRessurectionEntry.DisplayName = "Heaven's Feel: Ressurection"; Authorities.Add("HeavensFeel");
                HeavensFeelSoulCaptureID = AddMyActivatedAbility("Heaven's Feel: Soul Capture", "HeavensFeelSoulCapture", "Authority:Lust", "Awakened from the Lust Witchfactor, you gain a understanding of a way you can make a claim on the soul of another living thing. The target will get the effect SoulCapture, and on their death you will recieve their soul. SoulCapture will not work on non-mobile enemies.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); HeavensFeelSoulCaptureEntry = MyActivatedAbility(HeavensFeelSoulCaptureID); HeavensFeelSoulCaptureEntry.DisplayName = "Heaven's Feel: Soul Capture";
                HeavensFeelCheckSoulID = AddMyActivatedAbility("Heaven's Feel: Check Soul", "HeavensFeelCheckSoul", "Authority:Lust", "View information about the currently captured soul.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); HeavensFeelCheckSoulEntry = MyActivatedAbility(HeavensFeelCheckSoulID); HeavensFeelCheckSoulEntry.DisplayName = "Heaven's Feel: Check Soul";
                return true;
            }
            if (name.Equals("FacelessBride"))
            {
                FacelessBrideID = AddMyActivatedAbility("Faceless Bride", "FacelessBride", "Authority:Lust", "Awakened from the Lust Witchfactor, you gain a understanding of a way you can take make others perceive you in their perferred form. Pick a target at melee range and you will get +100 reputation with them. If you already had a faction under the effect of Faceless Bride, the effect is removed on the former faction. You are not currently using Faceless Bride.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); FacelessBrideEntry = MyActivatedAbility(FacelessBrideID); FacelessBrideEntry.DisplayName = "Faceless Bride"; Authorities.Add("FacelessBride");
                return true;
            }
            return false;
        }

        public bool ObtainAuthority()
        {
            List<string> MissingAuthorities = new List<string>();
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (Authorities.Contains("HeavensFeel"))
            {

            }
            else
            {
                MissingAuthorities.Add("HeavensFeel");
            }

            if (Authorities.Contains("FacelessBride"))
            {

            }
            else
            {
                MissingAuthorities.Add("FacelessBride");
            }

            if (MissingAuthorities.Count > 0)
            {
                int a = Stat.Random(0, MissingAuthorities.Count - 1);
                //Popup.Show(MissingAuthorities[a].ToString());

                switch (MissingAuthorities[a])
                {
                    default:
                        break;
                    case "HeavensFeel":
                        AddAuthority(MissingAuthorities[a]);
                        CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                    case "FacelessBride":
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




 