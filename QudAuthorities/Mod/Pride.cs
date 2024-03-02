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
using static System.Net.Mime.MediaTypeNames;
using System.Net.NetworkInformation;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Pride : BaseMutation
    {
        public Guid JudgementID;
        public Guid RewriteID;
        int rewrites;
        public ActivatedAbilityEntry JudgementEntry = null;
        public ActivatedAbilityEntry RewriteEntry = null;
        public bool JudgementOn = false;
        public List<string> Authorities = new List<string>();
        public List<int> damageValues = new List<int>();
        public int AwakeningOdds = 549;
        string WitchFactor = "";
        public Pride()
        {
            DisplayName = "Pride";
            Type = "Authority";
        }

        public override string GetDescription()
        {
            return "The Pride Witch Factor.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("A dark mass hiding within your soul bearing immense pride yearns for domination....\n There is a 1/550" + " chance to awaken another Authority of Pride upon gaining XP. The Authorities are: Judgement and Rewrite.");

            /*
            if (Authorities.Count == 0 || Authorities.Count == 1)
            {
            }
            else
            {
                if (Authorities.Count > 1)
                {
                    return string.Concat("The dark impurity within you that yearned to amass has changed form. You can feel gentle light within your soul where the darkness used to creep into your very being. It is eager to endow others with it's charity. The potential of Pride is fully realized.");
                }
                return string.Concat("Something is wrong with the Pride authority count!");
            }

            */

        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "Judgement");
            Object.RegisterPartEvent(this, "Rewrite");
            base.Register(Object);
        }

        public override bool HandleEvent(BeforeApplyDamageEvent E)
        {
            if(Authorities.Contains("Judgement"))
            {
                if (JudgementEntry.ToggleState == true)
                {
                    int b = Stat.Random(0, 24);

                    if (b == 0 && E.Actor.HasEffect("Judged") == false)
                    {



                        var list = E.Actor.Body.GetParts();
                        Popup.Show("List contents");
                        Popup.Show(list.Count.ToString());

                        foreach (var thing in list)
                        {
                            Popup.Show(thing.Type);
                        }

                        List<int> validLimbs = new List<int>();

                        for (int i = 0; i < list.Count; i++)
                        {
                            switch (list[i].Type)
                            {
                                default:
                                    validLimbs.Add(i);
                                    break;
                                case "Head":

                                    break;
                                case "Body":

                                    break;
                                case "Floating Nearby":

                                    break;
                                case "Thrown Weapon":

                                    break;
                                case "Missile Weapon":

                                    break;
                                case "Ammo":

                                    break;
                                case "Face":

                                    break;

                            }
                        }

                        foreach (var thing in validLimbs)
                        {
                            Popup.Show(thing.ToString());
                        }

                        E.Actor.ApplyEffect(new Judged());
                        int a = Stat.Random(0, validLimbs.Count - 1);
                        Popup.Show(list[a].Type);
                        var limb = list[a];
                        limb.Dismember();
                    }
                }
                else
                {

                }
            }
            else
            {

            }

            

            if(damageValues.Count == 5)
            {
                damageValues.RemoveAt(0);
                damageValues.Add(E.Damage.Amount);
            }
            else
            {
                damageValues.Add(E.Damage.Amount);
            }
            return true;
        }


        
       
        public override bool WantEvent(int ID, int cascade)
        {

            //This event rolls for chances to refresh Authority cool downa and to unlock unobtained Authorities.
            if (ID == AuthorityAwakeningPrideEvent.ID)
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
                    AuthorityAwakeningPrideEvent.Send(ParentObject);
                }

                int b = Stat.Random(0, 54);

                if(b == 1)
                {
                    if(rewrites >= 2)
                    {

                    }
                    else
                    {
                        rewrites++;
                        SyncAbilityName_Rewrite();
                    }
                }


            }
            return true;
        }



        public void BeginRewrite(GameObject target, int indexDamage, int indexEffect, Effect eff)
        {       
            rewrites--;
            SyncAbilityName_Rewrite();
            if (indexDamage != 9999)
            {
                int damage = damageValues[indexDamage];
                damageValues.RemoveAt(indexDamage);
                ParentObject.Heal(damage, false, true);
                target.TakeDamage(ref damage, null, "Rewrite", null, null, ParentObject);
            }
                   
            if(indexEffect != 9999)
            {
                if(eff == null)
                {
                    Popup.Show("NULL PROBLEM");
                }
                //Popup.Show(eff.DisplayName);
                //Popup.Show(eff.Duration.ToString());






                EffectDeepCopy(eff, target);
                ParentObject.RemoveEffect(eff);
                //
                //SyncAbilityName_Rewrite();
            } 
                    
                    
        }



        public override bool FireEvent(Event E)
        {

            if (E.ID == "Rewrite")
            {


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

                if (rewrites == 0)
                {
                    
                    if (ParentObject.IsPlayer())
                    {

                        Popup.ShowFail("You have no Rewrite charges.");

                    }
                    return false;
                }

                if(ParentObject.OnWorldMap())
                {
                    Popup.ShowFail("You cannot use this on the world map");
                    return false;
                }




                List<string> options = new List<string>();              
                foreach (var item in damageValues)
                {
                    options.Add(item.ToString());
                }
                options.Add("None");
                options.Add("Cancel");
                int index = Popup.ShowOptionList("Rewrite: Choose Damage Value", options.ToArray());

                if (options[index].Equals("Cancel"))
                {
                    return base.FireEvent(E);
                }

                List<string> optionsEffects = new List<string>();
                foreach (var item in ParentObject.Effects)
                {
                    switch (item.ClassName)
                    {
                        case "AshPoison":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Blaze_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Bleeding":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Blind":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Burning":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "CoatedInPlasma":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Confused":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Dazed":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Disoriented":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Ecstatic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Emboldened":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Frenzied":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Frozen":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "FungalSporeInfection":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "GeometricHeal":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Glotrot":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Healing":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Hoarshroom_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Hobbled":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "HulkHoney_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Ill":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Ironshank":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Luminous":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Meditating":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Monochrome":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Omniphase":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Paralyzed":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "PhasePoisoned":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Phased":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Poisoned":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "PoisonGasPoison":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Rubbergum_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Salve_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "ShadeOil_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Shaken":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "ShatterArmor":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "ShatterMentalArmor":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Shamed":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Skulk_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "GlotrotOnset":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "SphynxSalt_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "SporeCloudPoison":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "IronshankOnset":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "BasiliskPoison":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "LifeDrain":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;                      
                        case "Ubernostrum_Tonic":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Wakeful":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "HulkHoney_Tonic_Allergy":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        case "Rubbergum_Tonic_Allergy":
                            optionsEffects.Add(item.DisplayName.ToString());
                            break;
                        default:
                            break;
                    }

                    
                }
                optionsEffects.Add("None");
                optionsEffects.Add("Cancel");
                int indexEffect = Popup.ShowOptionList("Rewrite: Choose Effect", optionsEffects.ToArray());

                if (optionsEffects[indexEffect].Equals("Cancel"))
                {
                    return base.FireEvent(E);
                }

                if (options[index].Equals("None"))
                {
                    index = 9999;
                }

                Effect eff = null;

                if (optionsEffects[indexEffect].Equals("None"))
                {
                    indexEffect = 9999;
                }
                else
                {
                    eff = ParentObject.GetEffect(optionsEffects[indexEffect]);
                }


               

                UseEnergy(0, "Authority Mutation Rewrite");
                    BeginRewrite(gameObject, index, indexEffect, eff);
                



            }

            if (E.ID == "Judgement")
            {
                if (JudgementEntry.ToggleState == true)
                {
                    JudgementEntry.ToggleState = false;
                    JudgementOn = false;
                }
                else
                {
                    JudgementEntry.ToggleState = true;
                    JudgementOn = true;

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
            //ParentObject.GainEgo(1);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref JudgementID);
            RemoveMyActivatedAbility(ref RewriteID);
            //ParentObject.GainEgo(-1);
            RecountEvent.Send(ParentObject);
            return base.Unmutate(GO);
        }

        public void SyncAbilityName_Rewrite()
        {
           RewriteEntry.DisplayName = "Rewrite[" + rewrites.ToString() + "/2]";

        }

        public bool AddAuthority(string name)
        {
            if (name.Equals("Judgement"))
            {
                JudgementID = AddMyActivatedAbility("Judgement", "Judgement", "Authority:Pride", "Awakened from the Pride Witchfactor, if toggled on whenever the user it hit there is a 1/25 chance a random body part of your attacker will be dismembered. Afterwards they get the effect Judged, and can no longer suffer the effects of Judgement.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); JudgementEntry = MyActivatedAbility(JudgementID); JudgementEntry.DisplayName = "Judgement";
                return true;
            }
            if (name.Equals("Rewrite"))
            {
                RewriteID = AddMyActivatedAbility("Rewrite", "Rewrite", "Authority:Pride", "Awakened from the Pride Witchfactor, upon use you will be prompted with a list of damage you have taken in the last five turns and afterwards a list of effects you currently have. The chosen damage value will be healed to you and then applied to a target of your choosing along with an effect if chosen. The max amount of charges is 2 and you get charges back at a chance of 1/55 whenever you get xp.", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); RewriteEntry = MyActivatedAbility(RewriteID); rewrites = 2; RewriteEntry.DisplayName = "Rewrite[" + rewrites.ToString() + "/2]";
                return true;
            }
            return false;
        }

        public bool ObtainAuthority()
        {
            List<string> MissingAuthorities = new List<string>();
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (Authorities.Contains("Judgement"))
            {

            }
            else
            {
                MissingAuthorities.Add("Judgement");
            }

            if (Authorities.Contains("Rewrite"))
            {

            }
            else
            {
                MissingAuthorities.Add("Rewrite");
            }

            if (MissingAuthorities.Count > 0)
            {
                int a = Stat.Random(0, MissingAuthorities.Count - 1);
                //Popup.Show(MissingAuthorities[a].ToString());

                switch (MissingAuthorities[a])
                {
                    default:
                        break;
                    case "Judgement":
                        AddAuthority(MissingAuthorities[a]);
                        //CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                    case "Rewrite":
                        AddAuthority(MissingAuthorities[a]);
                        //CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;


                }
            }

            return false;
        }


        public void EffectDeepCopy(Effect originalEffect, GameObject target)
        {
            if (target.HasEffectByClass(originalEffect.ClassName))
            {
                target.RemoveEffectByClass(originalEffect.ClassName);
            }

            switch (originalEffect.ClassName)
            {
                case "AshPoison":
                    AshPoison ashpoison = (AshPoison)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new AshPoison(originalEffect.Duration, target));
                    break;
                case "Blaze_Tonic":
                    target.ApplyEffect(new Blaze_Tonic(originalEffect.Duration));
                    break;
                case "Bleeding":
                    Bleeding bleeding = (Bleeding)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Bleeding(bleeding.Damage.ToString(), bleeding.SaveTarget, target, bleeding.Stack));
                    break;
                case "Blind":
                    target.ApplyEffect(new Blind(originalEffect.Duration));
                    break;
                case "Burning":
                    //target.ApplyEffect(new Blind(originalEffect.Duration));
                    ParentObject.pPhysics.Temperature = ParentObject.pPhysics.AmbientTemperature;
                    target.pPhysics.Temperature = target.pPhysics.FlameTemperature;
                    break;
                case "CoatedInPlasma":
                    CoatedInPlasma coatedInPlasma = (CoatedInPlasma)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new CoatedInPlasma(originalEffect.Duration, target));
                    break;
                case "Confused":
                    Confused confused = (Confused)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Confused(originalEffect.Duration, confused.Level, confused.MentalPenalty));
                    break;
                case "Dazed":
                    target.ApplyEffect(new Dazed(originalEffect.Duration));
                    break;
                case "Disoriented":
                    Disoriented disoriented = (Disoriented)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Disoriented(originalEffect.Duration, disoriented.Level));
                    break;
                case "Ecstatic":
                    target.ApplyEffect(new Ecstatic(originalEffect.Duration));
                    break;
                case "Emboldened":
                    Emboldened emboldened = (Emboldened)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Emboldened(originalEffect.Duration, emboldened.Statistic, emboldened.Bonus));
                    break;
                case "Frenzied":
                    Frenzied frenzied = (Frenzied)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Frenzied(originalEffect.Duration, frenzied.QuicknessBonus, frenzied.MaxKillRadiusBonus, frenzied.BerserkDuration, frenzied.BerserkImmediately, frenzied.BerserkOnDealDamage, frenzied.PreferBleedingTarget));
                    break;
                case "Frozen":
                    //target.ApplyEffect(new Blind(originalEffect.Duration));
                    ParentObject.pPhysics.Temperature = ParentObject.pPhysics.AmbientTemperature;
                    target.pPhysics.Temperature = target.pPhysics.FreezeTemperature;
                    break;
                case "FungalSporeInfection":
                    FungalSporeInfection fungalSporeInfection = (FungalSporeInfection)ParentObject.GetEffect(originalEffect.DisplayName);
                    FungalSporeInfection newf = new FungalSporeInfection();
                    newf.InfectionObject = fungalSporeInfection.InfectionObject;
                    newf.Fake = fungalSporeInfection.Fake;
                    newf.TurnsLeft = fungalSporeInfection.TurnsLeft;
                    newf.Damage = fungalSporeInfection.Damage;
                    newf.bSpawned = fungalSporeInfection.bSpawned;
                    newf.Owner = target;
                    target.ApplyEffect(newf);
                    break;
                case "GeometricHeal":
                    GeometricHeal geometricHeal = (GeometricHeal)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new GeometricHeal(geometricHeal.Amount, geometricHeal.Ratio, originalEffect.Duration));
                    break;
                case "Glotrot":
                    Glotrot glotrot = (Glotrot)ParentObject.GetEffect(originalEffect.DisplayName);
                    Glotrot newg = new Glotrot();
                    newg.Stage = glotrot.Stage;
                    newg.Count = glotrot.Count;
                    newg.DrankIck = glotrot.DrankIck;
                    target.ApplyEffect(newg);
                    break;
                case "Healing":
                    target.ApplyEffect(new Healing(originalEffect.Duration));
                    break;
                case "Hoarshroom_Tonic":
                    target.ApplyEffect(new Hoarshroom_Tonic(originalEffect.Duration));
                    break;
                case "Hobbled":
                    target.ApplyEffect(new Hobbled(originalEffect.Duration));
                    break;
                case "HulkHoney_Tonic":
                    target.ApplyEffect(new HulkHoney_Tonic(originalEffect.Duration));
                    break;
                case "Ill":
                    Ill ill = (Ill)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Ill(originalEffect.Duration, ill.Level));
                    break;
                case "Ironshank":
                    Ironshank ironshank = (Ironshank)ParentObject.GetEffect(originalEffect.DisplayName);
                    Ironshank newi = new Ironshank();

                    newi.Count = ironshank.Count;
                    newi.Penalty = ironshank.Penalty;
                    newi.AVBonus = ironshank.AVBonus;
                    newi.DrankCure = ironshank.DrankCure;
                    target.ApplyEffect(newi);
                    break;
                case "Luminous":
                    target.ApplyEffect(new Luminous(originalEffect.Duration));
                    break;
                case "Meditating":
                    Meditating meditating = (Meditating)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Meditating(originalEffect.Duration, meditating.FromResting));
                    break;
                case "Monochrome":
                    target.ApplyEffect(new Monochrome());
                    break;
                case "Omniphase":
                    target.ApplyEffect(new Omniphase(originalEffect.Duration));
                    break;
                case "Paralyzed":
                    Paralyzed paralyzed = (Paralyzed)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Paralyzed(originalEffect.Duration, paralyzed.SaveTarget));
                    break;
                case "PhasePoisoned":
                    PhasePoisoned phasePoisoned = (PhasePoisoned)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new PhasePoisoned(originalEffect.Duration, phasePoisoned.DamageIncrement, phasePoisoned.Level, target));
                    break;
                case "Phased":
                    target.ApplyEffect(new Phased(originalEffect.Duration));
                    break;
                case "Poisoned":
                    Poisoned poisoned = (Poisoned)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Poisoned(originalEffect.Duration, poisoned.DamageIncrement, poisoned.Level, target));
                    break;
                case "PoisonGasPoison":
                    PoisonGasPoison poisonGasPoison = (PoisonGasPoison)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new PoisonGasPoison(originalEffect.Duration, target));
                    break;
                case "Rubbergum_Tonic":
                    target.ApplyEffect(new Rubbergum_Tonic(originalEffect.Duration));
                    break;
                case "Salve_Tonic":
                    target.ApplyEffect(new Salve_Tonic(originalEffect.Duration));
                    break;
                case "ShadeOil_Tonic":
                    target.ApplyEffect(new ShadeOil_Tonic(originalEffect.Duration));
                    break;
                case "Shaken":
                    Shaken shaken = (Shaken)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new Shaken(originalEffect.Duration, shaken.Level));
                    break;
                case "ShatterArmor":
                    target.ApplyEffect(new ShatterArmor(originalEffect.Duration));
                    break;
                case "ShatterMentalArmor":
                    target.ApplyEffect(new ShatterMentalArmor(originalEffect.Duration));
                    break;
                case "Shamed":
                    target.ApplyEffect(new Shamed(originalEffect.Duration));
                    break;
                case "Skulk_Tonic":
                    target.ApplyEffect(new Skulk_Tonic(originalEffect.Duration));
                    break;
                case "GlotrotOnset":
                    target.ApplyEffect(new GlotrotOnset());
                    break;
                case "SphynxSalt_Tonic":
                    target.ApplyEffect(new SphynxSalt_Tonic(originalEffect.Duration));
                    break;
                case "SporeCloudPoison":
                    SporeCloudPoison sporeCloudPoison = (SporeCloudPoison)ParentObject.GetEffect(originalEffect.DisplayName);
                    target.ApplyEffect(new SporeCloudPoison(originalEffect.Duration, target));
                    break;
                case "IronshankOnset":
                    target.ApplyEffect(new IronshankOnset());
                    break;
                case "BasiliskPoison":
                    target.ApplyEffect(new BasiliskPoison(originalEffect.Duration, target));
                    break;
                case "LifeDrain":
                    XRL.World.Effects.LifeDrain lifeDrain = (XRL.World.Effects.LifeDrain)ParentObject.GetEffect(originalEffect.DisplayName);
                    XRL.World.Effects.LifeDrain newl = new XRL.World.Effects.LifeDrain();

                    newl.Duration = lifeDrain.Duration;
                    newl.Level = lifeDrain.Level;
                    newl.Damage = lifeDrain.Damage;
                    newl.Drainer = ParentObject;
                    target.ApplyEffect(newl);
                    break;                
                case "Ubernostrum_Tonic":
                    target.ApplyEffect(new Ubernostrum_Tonic(originalEffect.Duration));
                    break;
                case "Wakeful":
                    target.ApplyEffect(new Wakeful(originalEffect.Duration));
                    break;
                case "HulkHoney_Tonic_Allergy":
                    target.ApplyEffect(new HulkHoney_Tonic_Allergy(originalEffect.Duration));
                    break;
                case "Rubbergum_Tonic_Allergy":
                    target.ApplyEffect(new Rubbergum_Tonic_Allergy(originalEffect.Duration));
                    break;
                    
                default:
                    //Popup.Show("Default");
                    //target.ApplyEffect(originalEffect, target);
                    break;


            }
        }

    }




}




