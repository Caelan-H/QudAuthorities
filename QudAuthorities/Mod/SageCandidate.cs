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
        public bool DidInit = false;
        public bool CheckpointQueue = false;
        public bool CheckpointCheckPass = false;
        public int WitchFactorOdds = 140;
        public int WitchfactorCount = 0;

        [NonSerialized]
        private long ActivatedSegment;


        public SageCandidate()
        {
            DisplayName = "Sage Candidate";
            Type = "Mental";
        }

        public override string GetDescription()
        {
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (WitchfactorCount == 6 && mutations.HasMutation("ReturnByDeath"))
            {
                return "Housed within your soul, you have all 7 Witchfactors.";

            }
            else
            {
                return "You have the capability to hold all Witchfactors within. There is a 1/140 chance when you get xp that you will obtain a new Witchfactor.";

            }
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("");
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (ID == AwardedXPEvent.ID)
            {
                
                int a = Stat.Random(0, WitchFactorOdds);

                if (a == 1)
                {
                    Popup.Show("it ran");
                    bool s = ObtainWitchFactor();
                    if(s) { Popup.Show("You feel the sensation of a sinister impurity take refuge within your soul. You can feel its immense power trying to consume you, but you endure it. Eventually, the dark power settles within becoming a part of you permanantly."); }
                    
                    return true;
                }
                return false;
            }
            return true;
        }

        public override bool FireEvent(Event E)
        {
            
                return base.FireEvent(E);
        }


       
        public override bool Mutate(GameObject GO, int Level)
        {
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
            if (mutations.HasMutation("ReturnByDeath"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Return By Death");
            }
            if (mutations.HasMutation("Gluttony"))
            {

            }
            else
            {
                MissingWitchFactors.Add("Gluttony");
            }
            Popup.Show(MissingWitchFactors.Count.ToString());
            if (MissingWitchFactors.Count> 0) 
            {
                int a = Stat.Random(0, MissingWitchFactors.Count - 1);
                Popup.Show(MissingWitchFactors[a].ToString());

                switch (MissingWitchFactors[a])
                {
                    default:
                        break;
                    case "Return By Death":
                        mutations.AddMutation("ReturnByDeath",1);
                        CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;
                    case "Gluttony":
                        mutations.AddMutation("Gluttony", 1);
                        CheckpointEvent.Send(ParentObject);
                        WitchfactorCount++;
                        return true;
                        
                }
            }


            

            return false;
        }

       
    }
}