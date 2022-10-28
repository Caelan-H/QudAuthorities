using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class ReturnByDeath : BaseMutation
    {
        
        //Both these are only useful if Return By Death is a use ability
        public new Guid ActivatedAbilityID; 
        public Guid RevertActivatedAbilityID;

        [NonSerialized]
        private long ActivatedSegment;


        public ReturnByDeath()
        {
            DisplayName = "Return By Death";
            Type = "Mental";
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "BeforeDie");
            Object.RegisterPartEvent(this, "GameRestored");
            Object.RegisterPartEvent(this, "Checkpoint");
            Object.RegisterPartEvent(this, "Return");
            base.Register(Object);
        }

        /*
        public override string GetDescription()
        {
            return "The Authority of Envy";
        }
        */

        /*
        public override string GetLevelText(int Level)
        {
            string Ret = "You are loved.\n";
            return Ret;
        }
        */
        

        public override bool FireEvent(Event E)
        {
            

            if (E.ID == "BeforeDie")
            {
                return OnBeforeDie(ParentObject, RevertActivatedAbilityID, ref ActivatedSegment);
            }
            if (E.ID == "GameRestored")
            {
                GenericDeepNotifyEvent.Send(ParentObject, "PrecognitionGameRestored");
            }
            if (E.ID == "Checkpoint")
            {
                Popup.Show("You activated Save", true, true, true, true);
                Save();
            }
            if (E.ID == "Return")
            {
                Popup.Show("You activated Load", true, true, true, true);
                //Load();
            }
            return base.FireEvent(E);
        }


        public static bool OnBeforeDie(GameObject Object, Guid revertActivatedAbilityID, ref long ActivatedSegment)
        {
            if (Object.GetStatValue("Hitpoints",0) <= 0)
            {
                Popup.Show("Return By Death has Activated", true, true, true, true);
                Load(Object);
                ActivatedSegment = The.Game.Segments + 100;
                return false;
            }
            /*
            if (Object.IsPlayer())
            {
                //AutoAct.Interrupt();
                if (WasPlayer)
                {
                   
                   // if (Popup.ShowYesNo("You sense your imminent demise. Would you like to return to the start of your vision?") == DialogResult.Yes)
                   // {
                        Load(Object);
                        ActivatedSegment = The.Game.Segments + 100;
                        return false;
                   // }
                }
            }
            else if (!Object.IsOriginalPlayerBody() && (!RealityDistortionBased || Object.FireEvent("CheckRealityDistortionUsability")))
            {
                TurnsLeft = 0;
                if (RevertAAID != Guid.Empty)
                {
                    Object.DisableActivatedAbility(RevertAAID);
                }
                if (Object.HasStat("Hitpoints"))
                {
                    ActivatedSegment = The.Game.Segments + 1;
                    Object.hitpoints = HitpointsAtSave;
                    if (Object.pPhysics != null)
                    {
                        Object.pPhysics.Temperature = TemperatureAtSave;
                    }
                    Object.DilationSplat();
                    Object.pPhysics.DidX("swim", "before your eyes", "!", null, Object);
                    return false;
                }
            }
            return true;
            */
            return true;
        }




        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilityID = AddMyActivatedAbility("Save", "Checkpoint", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);
            ActivatedAbilityID = AddMyActivatedAbility("Load", "Return", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);

            return base.Mutate(GO, Level);
        }

        /*
        public override bool ChangeLevel(int NewLevel)
        {
            return true;
        }


        

        public override bool Unmutate(GameObject GO)
        {
            return true;
        }
        */


        //My New Stuff
        public static void Save()
        {
            
            The.Core.SaveGame("Return.sav");
            
        }

        public static void Load(GameObject obj = null)
        {
            Dictionary<string, object> GameState = GetPrecognitionRestoreGameStateEvent.GetFor(obj);
            XRLGame.LoadGame(The.Game.GetCacheDirectory("Return.sav"), ShowPopup: false, GameState);
            /* THIS IS THE ORIGINAL LOAD FROM PRECOG
            GameManager.Instance.gameQueue.queueSingletonTask("PrecognitionEnd", delegate
            {
                XRLGame.LoadGame(The.Game.GetCacheDirectory("Precognition.sav"), ShowPopup: false, GameState);
            });
            */
        }






    }
}