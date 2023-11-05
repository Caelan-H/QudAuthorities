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

        [NonSerialized]
        private long ActivatedSegment;


        public SageCandidate()
        {
            DisplayName = "Sage Candidate";
            Type = "Mental";
        }

        public override string GetDescription()
        {
            return "You have the capability to hold all Witchfactors within";
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
                
                int a = Stat.Random(0, 3);

                if (a == 1)
                {
                    Popup.Show("it ran");
                    bool s = ObtainWitchFactor();
                    if(s) { Popup.Show("You feel the sensation of a strange black force take refuge within."); }
                    
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
                        return true;
                    case "Gluttony":
                        mutations.AddMutation("Gluttony", 1);
                        CheckpointEvent.Send(ParentObject);
                        return true;
                        
                }
            }


            

            return false;
        }

       
    }
}