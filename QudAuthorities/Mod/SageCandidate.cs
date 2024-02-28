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

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class SageCandidate : BaseMutation
    {
        public new Guid ActivatedAbilityID;
        public Guid RevertActivatedAbilityID;
        public int WitchFactorOdds = 549;
        public int WitchfactorCount = 0;
        public int xpEventCount = 0;

        [NonSerialized]
        private long ActivatedSegment;


        public SageCandidate()
        {
            DisplayName = "Sage Candidate";
            Type = "Authority";
        }

        public override string GetDescription()
        {  
                return "You have the capability to hold all Witchfactors within. There is a 1/550 chance when you get xp that you will obtain a new Witchfactor. After your first witch factor you will need to hit levels 10, 18, 24, 32, 40 to gain another.";    
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("");
        }

        public override bool CanLevel()
        {
            return false;
        }
     
        public void XPCount()
        {
            
            Popup.Show("You have obtained xp " + xpEventCount + " times");
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "XPCount");
            base.Register(Object);
        }

        public bool CanGetWitchfactor()
        {
            switch (WitchfactorCount)
            {
                default:
                    break;
                case 0:
                    return true;
                case 1:
                    if(ParentObject.Level >=10)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 2:
                    if (ParentObject.Level >= 18)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 3:
                    if (ParentObject.Level >= 24)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 4:
                    if (ParentObject.Level >= 32)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 5:
                    if (ParentObject.Level >= 40)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case 6:
                    return false;
            }
            return false;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (ID == AwardedXPEvent.ID)
            {


                xpEventCount++;
                int a = Stat.Random(0, WitchFactorOdds);

                if (a == 1)
                {
                    if(CanGetWitchfactor() == true)
                    {
                        bool s = ObtainWitchFactor();
                        if (s) { Popup.Show("You feel the sensation of a sinister impurity take refuge within your soul. You can feel its immense power trying to consume you, but you endure it. Eventually, the dark power settles within becoming a part of you permanantly."); }

                        return true;
                    }
                    
                }
                return false;
            }
            if (ID == RecountEvent.ID)
            {
                WitchfactorCount = 0;
                XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
                if (mutations.HasMutation("Gluttony")) { WitchfactorCount++; };
                if (mutations.HasMutation("Greed")) { WitchfactorCount++; };
                if (mutations.HasMutation("Lust")) { WitchfactorCount++; };
                if (mutations.HasMutation("Sloth")) { WitchfactorCount++; };
                if (mutations.HasMutation("Wrath")) { WitchfactorCount++; };
                if (mutations.HasMutation("Pride")) { WitchfactorCount++; };


                return true;
            }
            return true;
        }

        public override bool FireEvent(Event E)
        {

            if (E.ID == "XPCount")
            {
                XPCount();
            }
            return base.FireEvent(E);
        }


       
        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilityID = AddMyActivatedAbility("XP Count", "XPCount", "Mental Mutation", "Returns the number of times you gained xp while having Sage Candidate.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            return base.Unmutate(GO);
        }

       

       public bool ObtainWitchFactor()
        {
            List<string> MissingWitchFactors = new List<string>();
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (mutations.HasMutation("Greed"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Greed");
            }

            if (mutations.HasMutation("Gluttony"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Gluttony");
            }
            if (mutations.HasMutation("Lust"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Lust");
            }
            if (mutations.HasMutation("Sloth"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Sloth");
            }
            if (mutations.HasMutation("Wrath"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Wrath");
            }
            if (mutations.HasMutation("Pride"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Pride");
            }
            //Popup.Show(MissingWitchFactors.Count.ToString());
            if (MissingWitchFactors.Count> 0) 
            {
                int a = Stat.Random(0, MissingWitchFactors.Count - 1);
                //Popup.Show(MissingWitchFactors[a].ToString());

                switch (MissingWitchFactors[a])
                {
                    default:
                        break;
                    case "Gluttony":
                        mutations.AddMutation("Gluttony", 1);
                        //CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;
                    case "Greed":
                        mutations.AddMutation("Greed", 1);
                        //CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;
                    case "Lust":
                        mutations.AddMutation("Lust", 1);
                        //CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;
                    case "Sloth":
                        mutations.AddMutation("Sloth", 1);
                        //CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;
                    case "Wrath":
                        mutations.AddMutation("Wrath", 1);
                        //CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;
                    case "Pride":
                        mutations.AddMutation("Pride", 1);
                        //CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;


                }
            }


            

            return false;
        }

       
    }
}