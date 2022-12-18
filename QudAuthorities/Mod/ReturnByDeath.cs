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
    class ReturnByDeath : BaseMutation
    {
        public new Guid ActivatedAbilityID;
        public Guid RevertActivatedAbilityID;
        public bool DidInit = false;
        public bool CheckpointQueue = false;
        public bool CheckpointCheckPass = false;

        [NonSerialized]
        private long ActivatedSegment;


        public ReturnByDeath()
        {
            DisplayName = "Return By Death";
            Type = "Mental";
        }

        public override string GetDescription()
        {
            return "The Authority bestowed upon you to overcome death.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("Dying will return you to a previous checkpoint. Checkpoints are generated every time you get XP by rolling 1d64.");
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "BeforeDie");
            Object.RegisterPartEvent(this, "GameStart");
            Object.RegisterPartEvent(this, "GameRestored");
            Object.RegisterPartEvent(this, "DeathCount");
            base.Register(Object);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (ID == AwardedXPEvent.ID)
            {
               
                CheckpointQueue = true;
                return false;
            }
            if (ID == AfterGameLoadedEvent.ID)
            {
                CheckpointCheckPass = false;
                The.Core.SaveGame("Primary.sav");
                XRL.World.ZoneManager.ActivateBrainHavers(XRL.World.ZoneManager.instance.ActiveZone);
                return false;
            }
            if (ID == EndTurnEvent.ID && CheckpointQueue == true)
            {
                CheckpointQueue = false;
                CheckpointCheckPass = Checkpoint(ParentObject, ref ActivatedSegment);
                return false;
            }
            if (ID == ZoneActivatedEvent.ID && CheckpointCheckPass == true)
            {
                if (!(File.Exists(The.Game.GetCacheDirectory("Return.sav"))))
                {
                    The.Core.SaveGame("Return.sav");
                }

                CheckpointCheckPass = false;
                CopyZone();
                The.Core.SaveGame("Return.sav");
                return false;
            }
            if (ID == ZoneActivatedEvent.ID )
            {
                if (!(File.Exists(The.Game.GetCacheDirectory("Return.sav"))))
                {
                    CopyZone();
                    The.Core.SaveGame("Return.sav");
                }
                return false;
            }
            return true;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "GameStart")
            {
                if (OnGameStart(ParentObject, RevertActivatedAbilityID, ref ActivatedSegment, DidInit) == false)
                {
                    DidInit = true;
                }
                return OnGameStart(ParentObject, RevertActivatedAbilityID, ref ActivatedSegment, DidInit);
            }
            if (E.ID == "BeforeDie")
            {
                return OnBeforeDie(ParentObject, RevertActivatedAbilityID, ref ActivatedSegment);
            }
            if (E.ID == "GameRestored")
            {
                GenericDeepNotifyEvent.Send(ParentObject, "PrecognitionGameRestored");
            }
            if (E.ID == "DeathCount")
            {  
                DeathCount();
            }
                return base.FireEvent(E);
        }


        public static bool OnGameStart(GameObject Object, Guid revertActivatedAbilityID, ref long ActivatedSegment, bool DidInitialize)
        {
            if (DidInitialize == false)
            {
                System.IO.Directory.CreateDirectory(The.Game.GetCacheDirectory("ZoneCache"));
                System.IO.Directory.CreateDirectory(The.Game.GetCacheDirectory("ZoneCache") + "1");
               
                The.Core.SaveGame("Return.sav");
                return false;
            }
            return true;
        }

        public static void DeathCount()
        {
            string filePath = The.Game.GetCacheDirectory();
            string count = System.IO.File.ReadAllText(filePath + "\\DeathCount.txt");
            Popup.Show(count, true, true, true, true);
        }


        public static bool OnBeforeDie(GameObject Object, Guid revertActivatedAbilityID, ref long ActivatedSegment)
        {
            if (Object.GetStatValue("Hitpoints", 0) <= 0)
            {
                CopyZoneToCache();
                Load(Object);
                ActivatedSegment = The.Game.Segments + 100;
                return false;
            }
            return true;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            System.IO.Directory.CreateDirectory(The.Game.GetCacheDirectory("ZoneCache"));
            System.IO.Directory.CreateDirectory(The.Game.GetCacheDirectory("ZoneCache") + "1");
            ActivatedAbilityID = AddMyActivatedAbility("Death Count", "DeathCount", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false);
            string filePath = The.Game.GetCacheDirectory();
            System.IO.File.WriteAllText(filePath + "\\DeathCount.txt", "0");
            // The.Core.SaveGame("Return.sav");
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            System.IO.Directory.Delete(The.Game.GetCacheDirectory("ZoneCache") + "1", true);
            return base.Unmutate(GO);
        }

        public static bool Checkpoint(GameObject Object, ref long ActivatedSegment)
        {
            
            int a = Stat.Random(0, 63);
            if (a == 7)
            {
                return true;
            }
            return false;
        }

        public static void Load(GameObject obj = null)
        {
            XRLGame.LoadGame(The.Game.GetCacheDirectory("Return.sav"), ShowPopup: false, null);
        }

        public static void CopyZone()
        {
            Stopwatch WallTime = new Stopwatch();
            Loading.LoadTask("Saving game", delegate
            {
                WallTime = new Stopwatch();
                WallTime.Start();

                string sourcePath = The.Game.GetCacheDirectory("ZoneCache");
                string targetPath = The.Game.GetCacheDirectory("ZoneCache") + "1";

                string sourceFile = "";
                string destFile = "";
                string fileName = "";

                System.IO.Directory.CreateDirectory(targetPath);

                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] files = System.IO.Directory.GetFiles(sourcePath);

                    foreach (string s in files)
                    {
                        fileName = System.IO.Path.GetFileName(s);
                        destFile = System.IO.Path.Combine(targetPath, fileName);
                        sourceFile = System.IO.Path.Combine(targetPath, fileName);

                        if (File.Exists(destFile))
                        {
                            if (System.IO.Directory.GetLastWriteTime(sourceFile).Equals(System.IO.Directory.GetLastWriteTime(destFile)) == true)
                            {
                              //  Popup.Show("Two files had the same Last Write time", true, true, true, true);
                            }
                            else
                            {
                                System.IO.File.Copy(s, destFile, true);
                                //Popup.Show("Two files had same name but different Write Time", true, true, true, true);
                            }
                        }
                        else
                        {
                            System.IO.File.Copy(s, destFile, true);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Source path does not exist!");
                }
            });
            WallTime.Stop();
        }

        public static void CopyZoneToCache()
        {
            Stopwatch WallTime = new Stopwatch();
            Loading.LoadTask("Loading game", delegate
            {

                WallTime = new Stopwatch();
                WallTime.Start();

                string sourcePath = The.Game.GetCacheDirectory("ZoneCache") + "1";
                string targetPath = The.Game.GetCacheDirectory("ZoneCache");
                string filePath = The.Game.GetCacheDirectory();
                string count = System.IO.File.ReadAllText(filePath + "\\DeathCount.txt");
                int countCoversion = int.Parse(System.IO.File.ReadAllText(filePath + "\\DeathCount.txt"));
                countCoversion++;

          
                string sourceFile = "";
                string destFile = "";
                string fileName = "";

               
                System.IO.Directory.Delete(targetPath, true);
                System.IO.Directory.CreateDirectory(targetPath);
                System.IO.File.WriteAllText(filePath + "\\DeathCount.txt",countCoversion.ToString());
             
                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] files = System.IO.Directory.GetFiles(sourcePath);

                    foreach (string s in files)
                    {
                        
                        fileName = System.IO.Path.GetFileName(s);
                        destFile = System.IO.Path.Combine(targetPath, fileName);
                        sourceFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(s, destFile, true);
                    }
                }
                else
                {
                    Console.WriteLine("Source path does not exist!");
                }
            });
            WallTime.Stop();       
        }
    }
}