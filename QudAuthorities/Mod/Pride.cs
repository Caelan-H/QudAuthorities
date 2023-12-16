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

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Pride : BaseMutation
    {
        public Guid JudgementID;
        public Guid RewriteID;
        int rewrites = 0;
        public ActivatedAbilityEntry JudgementEntry = null;
        public ActivatedAbilityEntry RewriteEntry = null;
        public bool JudgementOn = false;
        public List<string> Authorities = new List<string>();
        public List<int> damageValues = new List<int>();
        public int AwakeningOdds = 699;
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
            return string.Concat("A dark mass hiding within your soul bearing immense pride yearns for domination....\n There is a 1/700" + " chance to awaken another Authority of Pride. The Authorities are: Judgement and Rewrite. Ego +1.");

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
            if(JudgementEntry.ToggleState == true)
            {
                int b = Stat.Random(0, 19);

                if(b == 0 && E.Actor.HasEffect("Judged") == false)
                {
                    
                    var limb = E.Actor.GetRandomConcreteBodyPart();
                    E.Actor.ApplyEffect(new Judged());
                    limb.Dismember();
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


        public override bool HandleEvent(ApplyEffectEvent E)
        {


            



            return false;
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

                int b = Stat.Random(0, 12);

                if(b == 1)
                {
                    if(rewrites >= 3)
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



        public void BeginRewrite(GameObject target, int indexDamage, int indexEffect)
        {       
            rewrites--;
            SyncAbilityName_Rewrite();
            if (indexDamage != 9999)
            {
               ParentObject.Heal(damageValues[indexDamage], false, true);
               int damage = damageValues[indexDamage];
               damageValues.RemoveAt(indexDamage);
               target.TakeDamage(ref damage, null, "Rewrite", null, null, ParentObject);
            }
                   
            if(indexEffect != 9999)
            {
                target.ApplyEffect(ParentObject.Effects[indexEffect]);
                ParentObject.RemoveEffect(ParentObject.Effects[indexEffect]);
                SyncAbilityName_Rewrite();
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

                        Popup.ShowFail("You have no Soulwash charges.");

                    }
                    return false;
                }



                List<string> options = new List<string>();              
                foreach (var item in damageValues)
                {
                    options.Add(item.ToString());
                }
                options.Add("None");
                int index = Popup.ShowOptionList("Rewrite: Choose Damage Value", options.ToArray());
         

                List<string> optionsEffects = new List<string>();
                foreach (var item in ParentObject.Effects)
                {
                    options.Add(item.ToString());
                }
                optionsEffects.Add("None");
                int indexEffect = Popup.ShowOptionList("Rewrite: Choose Effect", optionsEffects.ToArray());

                if (options[index].Equals("None"))
                {
                    index = 9999;
                }

                if (optionsEffects[indexEffect].Equals("None"))
                {
                    indexEffect = 9999;
                }




                UseEnergy(0, "Authority Mutation Rewrite");
                    BeginRewrite(gameObject, index, indexEffect);
                



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
            ParentObject.GainEgo(1);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref JudgementID);
            RemoveMyActivatedAbility(ref RewriteID);
            ParentObject.GainEgo(-1);
            RecountEvent.Send(ParentObject);
            return base.Unmutate(GO);
        }

        public void SyncAbilityName_Rewrite()
        {
           RewriteEntry.DisplayName = "Rewrite[" + rewrites.ToString() + "/3]";

        }

        public bool AddAuthority(string name)
        {
            if (name.Equals("Judgement"))
            {
                JudgementID = AddMyActivatedAbility("Judgement", "Judgement", "Authority:Pride", "Awakened from the Pride Witchfactor, if toggled on whenever the user it hit there is a 5% chance a random body part of your attacker will be dismembered. Afterwards they get the effect Judged, and can no longer suffer the effects of Judgement.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); JudgementEntry = MyActivatedAbility(JudgementID); JudgementEntry.DisplayName = "Judgement";
                return true;
            }
            if (name.Equals("Rewrite"))
            {
                RewriteID = AddMyActivatedAbility("Rewrite", "Rewrite", "Authority:Pride", "Awakened from the Pride Witchfactor, upon use you will be prompted with a list of damage you have taken in the last five turns and afterwards a list of effects you currently have. The chosen damage value will be healed to you and then applied to a target of your choosing along with an effect if chosen. The max amount of charges is 3 and you get charges back at a chance of 10% whenever you get xp.", "\u000e", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); RewriteEntry = MyActivatedAbility(RewriteID); rewrites = 3; RewriteEntry.DisplayName = "Rewrite[" + rewrites.ToString() + "/3]";
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
                        CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                    case "Rewrite":
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




