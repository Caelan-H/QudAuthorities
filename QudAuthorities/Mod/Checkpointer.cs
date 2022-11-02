using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using Newtonsoft.Json;
using Qud.API;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.UI.ObjectFinderClassifiers;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;

namespace QudAuthorities.Mod
{

    [Serializable]
    public class Checkpointer : XRLGame
    {
        public Checkpointer(TextConsole _Console, ScreenBuffer _Buffer) : base(_Console, _Buffer)
        {
        }

        /*
        public bool SaveGame(string GameName, string message = "Saving game")
        {
            if (!Running)
            {
                return false;
            }
            bool result = true;
            Loading.LoadTask(message, delegate
            {
                MemoryHelper.GCCollectMax();
                try
                {
                    SetIntGameState("FungalVisionLevel", FungalVisionary.VisionLevel);
                    SetIntGameState("GreyscaleLevel", GameManager.Instance.GreyscaleLevel);
                }
                catch (Exception)
                {
                }
                try
                {
                    if (WallTime != null)
                    {
                        _walltime += WallTime.ElapsedTicks;
                        WallTime.Reset();
                        WallTime.Start();
                    }
                    else
                    {
                        WallTime = new Stopwatch();
                        WallTime.Start();
                    }
                    SetIntGameState("NextRandomSeed", Stat.Rnd.Next());
                    XRL.World.GameObject body = Player.Body;
                    SaveGameJSON saveGameJSON = new SaveGameJSON
                    {
                        SaveVersion = 264,
                        GameVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                        ID = GameID,
                        Name = PlayerName,
                        Level = body.Statistics["Level"].Value,
                        GenoSubType = ""
                    };
                    if (body.HasProperty("Genotype"))
                    {
                        saveGameJSON.GenoSubType += body.Property["Genotype"];
                    }
                    if (body.HasProperty("Subtype"))
                    {
                        saveGameJSON.GenoSubType = saveGameJSON.GenoSubType + " " + body.Property["Subtype"];
                    }
                    saveGameJSON.GameMode = GetStringGameState("GameMode", "Classic");
                    RenderEvent renderEvent = body.RenderForUI();
                    saveGameJSON.CharIcon = renderEvent.Tile;
                    saveGameJSON.FColor = renderEvent.GetForegroundColorChar();
                    saveGameJSON.DColor = renderEvent.GetDetailColorChar();
                    saveGameJSON.Location = ZoneManager.GetZoneDisplayName(body.CurrentZone.ZoneID);
                    TimeSpan timeSpan = TimeSpan.FromTicks(_walltime);
                    saveGameJSON.InGameTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                    saveGameJSON.Turn = Turns;
                    saveGameJSON.SaveTime = DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString();
                    saveGameJSON.ModsEnabled = ModManager.GetRunningMods();
                    File.WriteAllText(Path.Combine(GetCacheDirectory(), GameName + ".json"), JsonConvert.SerializeObject(saveGameJSON, Formatting.Indented));
                    if (File.Exists(Path.Combine(GetCacheDirectory(), GameName)))
                    {
                        try
                        {
                            File.Copy(Path.Combine(GetCacheDirectory(), GameName), Path.Combine(GetCacheDirectory(), GameName) + ".bak", overwrite: true);
                        }
                        catch (Exception ex2)
                        {
                            UnityEngine.Debug.LogError("Exception making save backup:" + ex2.ToString());
                        }
                    }
                    using (FileStream stream = File.Create(Path.Combine(GetCacheDirectory(), GameName + ".tmp")))
                    {
                        SerializationWriter serializationWriter = new SerializationWriter(stream, _bSerializePlayer: true);
                        serializationWriter.Write(123457);
                        serializationWriter.Write(264);
                        serializationWriter.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                        serializationWriter.FileVersion = 264;
                        serializationWriter.WriteObject(this);
                        _Player.Save(serializationWriter);
                        serializationWriter.Write(111111);
                        ZoneManager.Save(serializationWriter);
                        serializationWriter.Write(222222);
                        ActionManager.Save(serializationWriter);
                        AbilityManager.Save(serializationWriter);
                        SaveQuests(serializationWriter);
                        serializationWriter.Write(333333);
                        PlayerReputation.Save(serializationWriter);
                        serializationWriter.Write(333444);
                        Examiner.SaveGlobals(serializationWriter);
                        serializationWriter.Write(444444);
                        TinkerItem.SaveGlobals(serializationWriter);
                        serializationWriter.Write(WorldMazes);
                        serializationWriter.Write<string>(Accomplishments);
                        Factions.Save(serializationWriter);
                        sultanHistory.Save(serializationWriter);
                        serializationWriter.Write(555555);
                        Gender.SaveAll(serializationWriter);
                        PronounSet.SaveAll(serializationWriter);
                        serializationWriter.Write(666666);
                        if (ObjectGameState.Count > 0)
                        {
                            serializationWriter.Write(ObjectGameState.Count);
                            foreach (KeyValuePair<string, object> item in ObjectGameState)
                            {
                                if (item.Value is IObjectGamestateCustomSerializer objectGamestateCustomSerializer)
                                {
                                    serializationWriter.Write("~!" + objectGamestateCustomSerializer.GetType().FullName);
                                    serializationWriter.Write(item.Key);
                                    objectGamestateCustomSerializer.GameSave(serializationWriter);
                                }
                                else
                                {
                                    serializationWriter.Write(item.Key);
                                    serializationWriter.WriteObject(item.Value);
                                    if (item.Value is IGamestatePostsave gamestatePostsave)
                                    {
                                        gamestatePostsave.OnGamestatePostsave(this, serializationWriter);
                                    }
                                }
                            }
                        }
                        else
                        {
                            serializationWriter.Write(0);
                        }
                        if (BlueprintsSeen.Count > 0)
                        {
                            serializationWriter.Write(BlueprintsSeen.Count);
                            foreach (string item2 in BlueprintsSeen)
                            {
                                serializationWriter.Write(item2);
                            }
                        }
                        else
                        {
                            serializationWriter.Write(0);
                        }
                        serializationWriter.WriteGameObjects();
                        foreach (IGameSystem system in Systems)
                        {
                            system.SaveGame(serializationWriter);
                        }
                        serializationWriter.AppendTokenTables();
                    }
                    File.Copy(Path.Combine(GetCacheDirectory(), GameName + ".tmp"), Path.Combine(GetCacheDirectory(), GameName), overwrite: true);
                    File.Delete(Path.Combine(GetCacheDirectory(), GameName + ".tmp"));
                }
                catch (Exception ex3)
                {
                    result = false;
                    MetricsManager.LogException("SaveGame", ex3);
                    XRLCore.LogError("Exception during SaveGame it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex3.ToString());
                    if (File.Exists(Path.Combine(GetCacheDirectory(), GameName) + ".bak"))
                    {
                        try
                        {
                            File.Copy(Path.Combine(GetCacheDirectory(), GameName) + ".bak", Path.Combine(GetCacheDirectory(), GameName), overwrite: true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    Popup.Show("There was a fatal exception attempting to save your game. Caves of Qud attempted to recover your prior save. You probably want to close the game and reload your most recent save. It'd be helpful to send the save and logs to support@freeholdgames.com");
                }
            });
            MemoryHelper.GCCollectMax();
            return result;
        }
         */

        public SaveGameJSON MakeJSON()
        {
            XRL.World.GameObject body = Player.Body;
            SaveGameJSON saveGameJSON = new SaveGameJSON
            {
                SaveVersion = 264,
                GameVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                ID = GameID,
                Name = PlayerName,
                Level = body.Statistics["Level"].Value,
                GenoSubType = ""
            };
            if (body.HasProperty("Genotype"))
            {
                saveGameJSON.GenoSubType += body.Property["Genotype"];
            }
            if (body.HasProperty("Subtype"))
            {
                saveGameJSON.GenoSubType = saveGameJSON.GenoSubType + " " + body.Property["Subtype"];
            }
            saveGameJSON.GameMode = GetStringGameState("GameMode", "Classic");
            RenderEvent renderEvent = body.RenderForUI();
            saveGameJSON.CharIcon = renderEvent.Tile;
            saveGameJSON.FColor = renderEvent.GetForegroundColorChar();
            saveGameJSON.DColor = renderEvent.GetDetailColorChar();
            saveGameJSON.Location = ZoneManager.GetZoneDisplayName(body.CurrentZone.ZoneID);
            TimeSpan timeSpan = TimeSpan.FromTicks(_walltime);
            saveGameJSON.InGameTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            saveGameJSON.Turn = Turns;
            saveGameJSON.SaveTime = DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString();
            saveGameJSON.ModsEnabled = ModManager.GetRunningMods();
            Popup.Show("Made", true, true, true, true);
            return saveGameJSON;
        }
        

            public SaveGameJSON MakeCheckpoint( string message = "Saving game")
        {
            SaveGameJSON output = new SaveGameJSON();
            string GameName = "Return";

            Loading.LoadTask(message, delegate
            {
                MemoryHelper.GCCollectMax();
                try
                {
                    SetIntGameState("FungalVisionLevel", FungalVisionary.VisionLevel);
                    SetIntGameState("GreyscaleLevel", GameManager.Instance.GreyscaleLevel);
                }
                catch (Exception)
                {
                }
                try
                {
                    if (WallTime != null)
                    {
                        _walltime += WallTime.ElapsedTicks;
                        WallTime.Reset();
                        WallTime.Start();
                    }
                    else
                    {
                        WallTime = new Stopwatch();
                        WallTime.Start();
                    }
                    SetIntGameState("NextRandomSeed", Stat.Rnd.Next());
                    XRL.World.GameObject body = Player.Body;
                    SaveGameJSON saveGameJSON = new SaveGameJSON
                    {
                        SaveVersion = 264,
                        GameVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                        ID = GameID,
                        Name = PlayerName,
                        Level = body.Statistics["Level"].Value,
                        GenoSubType = ""
                    };
                    if (body.HasProperty("Genotype"))
                    {
                        saveGameJSON.GenoSubType += body.Property["Genotype"];
                    }
                    if (body.HasProperty("Subtype"))
                    {
                        saveGameJSON.GenoSubType = saveGameJSON.GenoSubType + " " + body.Property["Subtype"];
                    }
                    saveGameJSON.GameMode = GetStringGameState("GameMode", "Classic");
                    RenderEvent renderEvent = body.RenderForUI();
                    saveGameJSON.CharIcon = renderEvent.Tile;
                    saveGameJSON.FColor = renderEvent.GetForegroundColorChar();
                    saveGameJSON.DColor = renderEvent.GetDetailColorChar();
                    saveGameJSON.Location = ZoneManager.GetZoneDisplayName(body.CurrentZone.ZoneID);
                    TimeSpan timeSpan = TimeSpan.FromTicks(_walltime);
                    saveGameJSON.InGameTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                    saveGameJSON.Turn = Turns;
                    saveGameJSON.SaveTime = DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString();
                    saveGameJSON.ModsEnabled = ModManager.GetRunningMods();
                    /*
                    File.WriteAllText(Path.Combine(GetCacheDirectory(), GameName + ".json"), JsonConvert.SerializeObject(saveGameJSON, Formatting.Indented));
                    if (File.Exists(Path.Combine(GetCacheDirectory(), GameName)))
                    {
                        try
                        {
                            File.Copy(Path.Combine(GetCacheDirectory(), GameName), Path.Combine(GetCacheDirectory(), GameName) + ".bak", overwrite: true);
                        }
                        catch (Exception ex2)
                        {
                            UnityEngine.Debug.LogError("Exception making save backup:" + ex2.ToString());
                        }
                    }
                    */
                    
                    output = saveGameJSON;
                }
                catch (Exception)
                {
                }
            });
            MemoryHelper.GCCollectMax();
            Popup.Show(output.GameMode, true, true, true, true);
            return output;
        }
       
       

        public bool SaveCheckpoint(SaveGameJSON saveGameJSON,string GameName, string message = "Saving game")
        {
            if (!Running)
            {
                return false;
            }
            bool result = true;
            Loading.LoadTask(message, delegate
            {
                MemoryHelper.GCCollectMax();
                
                try
                {
                   
                    File.WriteAllText(Path.Combine(GetCacheDirectory(), GameName + ".json"), JsonConvert.SerializeObject(saveGameJSON, Formatting.Indented));
                    if (File.Exists(Path.Combine(GetCacheDirectory(), GameName)))
                    {
                        try
                        {
                            File.Copy(Path.Combine(GetCacheDirectory(), GameName), Path.Combine(GetCacheDirectory(), GameName) + ".bak", overwrite: true);
                        }
                        catch (Exception ex2)
                        {
                            UnityEngine.Debug.LogError("Exception making save backup:" + ex2.ToString());
                        }
                    }
                    using (FileStream stream = File.Create(Path.Combine(GetCacheDirectory(), GameName + ".tmp")))
                    {
                        SerializationWriter serializationWriter = new SerializationWriter(stream, _bSerializePlayer: true);
                        serializationWriter.Write(123457);
                        serializationWriter.Write(264);
                        serializationWriter.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                        serializationWriter.FileVersion = 264;
                        serializationWriter.WriteObject(this);
                        _Player.Save(serializationWriter);
                        serializationWriter.Write(111111);
                        ZoneManager.Save(serializationWriter);
                        serializationWriter.Write(222222);
                        ActionManager.Save(serializationWriter);
                        AbilityManager.Save(serializationWriter);
                        SaveQuests(serializationWriter);
                        serializationWriter.Write(333333);
                        PlayerReputation.Save(serializationWriter);
                        serializationWriter.Write(333444);
                        Examiner.SaveGlobals(serializationWriter);
                        serializationWriter.Write(444444);
                        TinkerItem.SaveGlobals(serializationWriter);
                        serializationWriter.Write(WorldMazes);
                        serializationWriter.Write<string>(Accomplishments);
                        Factions.Save(serializationWriter);
                        sultanHistory.Save(serializationWriter);
                        serializationWriter.Write(555555);
                        Gender.SaveAll(serializationWriter);
                        PronounSet.SaveAll(serializationWriter);
                        serializationWriter.Write(666666);
                        if (ObjectGameState.Count > 0)
                        {
                            serializationWriter.Write(ObjectGameState.Count);
                            foreach (KeyValuePair<string, object> item in ObjectGameState)
                            {
                                if (item.Value is IObjectGamestateCustomSerializer objectGamestateCustomSerializer)
                                {
                                    serializationWriter.Write("~!" + objectGamestateCustomSerializer.GetType().FullName);
                                    serializationWriter.Write(item.Key);
                                    objectGamestateCustomSerializer.GameSave(serializationWriter);
                                }
                                else
                                {
                                    serializationWriter.Write(item.Key);
                                    serializationWriter.WriteObject(item.Value);
                                    if (item.Value is IGamestatePostsave gamestatePostsave)
                                    {
                                        gamestatePostsave.OnGamestatePostsave(this, serializationWriter);
                                    }
                                }
                            }
                        }
                        else
                        {
                            serializationWriter.Write(0);
                        }
                        if (BlueprintsSeen.Count > 0)
                        {
                            serializationWriter.Write(BlueprintsSeen.Count);
                            foreach (string item2 in BlueprintsSeen)
                            {
                                serializationWriter.Write(item2);
                            }
                        }
                        else
                        {
                            serializationWriter.Write(0);
                        }
                        serializationWriter.WriteGameObjects();
                        foreach (IGameSystem system in Systems)
                        {
                            system.SaveGame(serializationWriter);
                        }
                        serializationWriter.AppendTokenTables();
                    }
                    File.Copy(Path.Combine(GetCacheDirectory(), GameName + ".tmp"), Path.Combine(GetCacheDirectory(), GameName), overwrite: true);
                    File.Delete(Path.Combine(GetCacheDirectory(), GameName + ".tmp"));
                }
                catch (Exception ex3)
                {
                    result = false;
                    MetricsManager.LogException("SaveGame", ex3);
                    XRLCore.LogError("Exception during SaveGame it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex3.ToString());
                    if (File.Exists(Path.Combine(GetCacheDirectory(), GameName) + ".bak"))
                    {
                        try
                        {
                            File.Copy(Path.Combine(GetCacheDirectory(), GameName) + ".bak", Path.Combine(GetCacheDirectory(), GameName), overwrite: true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    Popup.Show("There was a fatal exception attempting to save your game. Caves of Qud attempted to recover your prior save. You probably want to close the game and reload your most recent save. It'd be helpful to send the save and logs to support@freeholdgames.com");
                }
            });
            MemoryHelper.GCCollectMax();
            return result;
        }

    }
}



