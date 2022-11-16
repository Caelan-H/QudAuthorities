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
using QudAuthorities.Mod;
using Qud.API;
using System.IO;
using System.Diagnostics;
using XRL.World.Effects;
using Newtonsoft.Json;
using Steamworks;
using System.Reflection;
using XRL.World.ZoneBuilders;
using static TBComponent;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class ReturnByDeath : BaseMutation
    {

        //Both these are only useful if Return By Death is a use ability
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




        public override bool CanLevel()
        {
            return false;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "BeforeDie");
            Object.RegisterPartEvent(this, "GameStart");
            Object.RegisterPartEvent(this, "GameRestored");


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

        public override bool WantEvent(int ID, int cascade)
        {
            if (ID == AwardedXPEvent.ID)
            {
                CheckpointQueue = true;



               




                //Popup.Show(The.Game.GetCacheDirectory("ZoneCache"), true, true, true, true);
                return false;
            }
            if (ID == AfterGameLoadedEvent.ID)
            {
                CheckpointCheckPass = false;
                The.Core.SaveGame("Primary.sav");
                return false;
            }
            if (ID == EndTurnEvent.ID && CheckpointQueue == true)
            {
                CheckpointQueue = false;
                CheckpointCheckPass = Checkpoint(ParentObject, ref ActivatedSegment);
                //Popup.Show(CheckpointCheckPass.ToString(), true, true, true, true);
                return false;
            }
            if (ID == ZoneActivatedEvent.ID && CheckpointCheckPass == true)
            {
                CheckpointCheckPass = false;
                CopyZone();
                The.Core.SaveGame("Return.sav");
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




            return base.FireEvent(E);
        }


        public static bool OnGameStart(GameObject Object, Guid revertActivatedAbilityID, ref long ActivatedSegment, bool DidInitialize)
        {
            if (DidInitialize == false)
            {
                The.Core.SaveGame("Return.sav");
                return false;
            }

            return true;
        }


        public static bool OnBeforeDie(GameObject Object, Guid revertActivatedAbilityID, ref long ActivatedSegment)
        {
            if (Object.GetStatValue("Hitpoints", 0) <= 0)
            {
                //Popup.Show("", true, true, true, true);
                The.Game.ZoneManager.Release();
                The.Game.ZoneManager.CachedZones.Clear();

                CopyZoneToCache();

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
            System.IO.Directory.CreateDirectory(The.Game.GetCacheDirectory("ZoneCache") );
            System.IO.Directory.CreateDirectory(The.Game.GetCacheDirectory("ZoneCache") + "1");
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


        public static bool Checkpoint(GameObject Object, ref long ActivatedSegment)
        {


            int a = Stat.Random(0, 63);
            if (a == 7)
            {
                //Popup.Show("Checkpoint triggered", true, true, true, true);
               
                return true;
                //The.Core.SaveGame("Return.sav");

                // Qud.API.SaveGameJSON saveFile = CheckpointCreator.MakeJSON();
                //CheckpointCreator.SaveCheckpoint(saveFile,"Return");

                // Popup.Show("Process Ran", true, true, true, true);
                // string ba = saveFile.Name;
                // CheckpointCreator.SaveCheckpoint(saveFile, "Return", "Saving game");
                //Popup.Show(ba, true, true, true, true);
                // Popup.Show("A", true, true, true, true);

            }

            return false;

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

        public static void CopyZone()
        {






            Stopwatch WallTime = new Stopwatch();
            Loading.LoadTask("Saving game", delegate
            {


                WallTime = new Stopwatch();
                WallTime.Start();

                string sourcePath = The.Game.GetCacheDirectory("ZoneCache");
                string targetPath = The.Game.GetCacheDirectory("ZoneCache") + "1";

                // Use Path class to manipulate file and directory paths.
                string sourceFile = "";
                string destFile = "";
                string fileName = "";

                // To copy a folder's contents to a new location:
                // Create a new target folder.
                // If the directory already exists, this method does not create a new directory.
                System.IO.Directory.CreateDirectory(targetPath);


                // To copy all the files in one directory to another directory.
                // Get the files in the source folder. (To recursively iterate through
                // all subfolders under the current directory, see
                // "How to: Iterate Through a Directory Tree.")
                // Note: Check for target path was performed previously
                //       in this code example.
                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] files = System.IO.Directory.GetFiles(sourcePath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        fileName = System.IO.Path.GetFileName(s);
                        destFile = System.IO.Path.Combine(targetPath, fileName);
                        sourceFile = System.IO.Path.Combine(targetPath, fileName);
                        //Popup.Show("The name of this file is: " + fileName, true, true, true, true);

                        if (File.Exists(destFile))
                        {
                           // Popup.Show("The File existed", true, true, true, true);
                            // This path is a file
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
                        
                        //System.IO.File.Copy(s, destFile, true);
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

                // Use Path class to manipulate file and directory paths.
                string sourceFile = "";
                string destFile = "";
                string fileName = "";

                // To copy a folder's contents to a new location:
                // Create a new target folder.
                // If the directory already exists, this method does not create a new directory.
                System.IO.Directory.Delete(targetPath, true);
                System.IO.Directory.CreateDirectory(targetPath);


                // To copy all the files in one directory to another directory.
                // Get the files in the source folder. (To recursively iterate through
                // all subfolders under the current directory, see
                // "How to: Iterate Through a Directory Tree.")
                // Note: Check for target path was performed previously
                //       in this code example.
                if (System.IO.Directory.Exists(sourcePath))
                {
                    string[] files = System.IO.Directory.GetFiles(sourcePath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        fileName = System.IO.Path.GetFileName(s);

                       
                        destFile = System.IO.Path.Combine(targetPath, fileName);
                        sourceFile = System.IO.Path.Combine(targetPath, fileName);

                        /*
                        if (File.Exists(destFile))
                        {
                            // This path is a file
                            if(System.IO.Directory.GetLastAccessTime(sourceFile).Equals(System.IO.Directory.GetLastAccessTime(destFile)))
                            {

                            }
                            else
                            {
                                System.IO.File.Copy(s, destFile, true);
                            }
                            


                        }
                        else
                        {
                            System.IO.File.Copy(s, destFile, true);
                        }
                        */

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