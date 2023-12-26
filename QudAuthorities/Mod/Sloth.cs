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
using static System.Net.Mime.MediaTypeNames;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Sloth : BaseMutation
    {
        public Guid InvisibleProvidenceID;     
        public ActivatedAbilityEntry InvisibleProvidenceEntry = null;       
        public List<string> Authorities = new List<string>();
        public int buildUp = 0;
        public int range = 4;
        public int level = 0;
        public int IP_XP = 0;
        public int AwakeningOdds = 699;      
        string WitchFactor = "";
        public Sloth()
        {
            DisplayName = "Sloth";
            Type = "Authority";
        }

        public override string GetDescription()
        {
            return "The Sloth Witch Factor.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Concat("A dark mass hides within your soul desperate for rest....\n The Authorities are: Invisible Providence. Willpower +1.");

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
            Object.RegisterPartEvent(this, "InvisibleProvidence");
            base.Register(Object);
        }

       
        public override bool WantEvent(int ID, int cascade)
        {
            
            //This event rolls for chances to refresh Authority cooldown and to unlock unobtained Authorities.
            if (ID == AuthorityAwakeningSlothEvent.ID)
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
                    //AuthorityAwakeningSlothEvent.Send(ParentObject);
                }

                int b = Stat.Random(0, 49);

                if (a == 1)
                {
                    if(buildUp > 0)
                    {
                        buildUp--;
                        string tier = getTier();
                        InvisibleProvidenceEntry.DisplayName = "Invisible Providence:[" + buildUp.ToString() + "/20|" + tier + "]";
                        //SynchronizeIP();
                    }
                }


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

        public string getTier()
        {
            if (buildUp >= 18)
            {
                return "T6";
               
            }
            else
            {
                if (buildUp >= 15)
                {
                    return "T5";
                }
                else
                {
                    if (buildUp >= 12)
                    {
                        return "T4";
                    }
                    else
                    {
                        if (buildUp >= 9)
                        {
                            return "T3";
                        }
                        else
                        {
                            if (buildUp >= 6)
                            {

                                return "T2";
                            }
                            else
                            {
                                if (buildUp >= 3)
                                {
                                    return "T1";
                                }
                                else
                                {
                                    return "T0";
                                }
                            }
                        }
                    }
                }
            }
        }

        public void BeginInvisibleProvidence(GameObject target)
        {
            int damage = 15 + ParentObject.Level;
            target.TakeDamage(ref damage, null, "Invisible Providence", null, null, ParentObject);
            Kickback();
            if(buildUp < 20)
            {
                buildUp++;
                string tier = getTier();              
                InvisibleProvidenceEntry.DisplayName = "Invisible Providence:[" + buildUp.ToString() + "/20|" + tier +"]";
            }
            
        }

        public bool canUse()
        {
            if (buildUp == 0 || buildUp == 1 || buildUp == 2)
            {
                return true;
            }
            else if (buildUp == 3 || buildUp == 4 || buildUp == 5)
            {
                if (ParentObject.GetHPPercent() > 30)
                {
                    return true;
                }
                else
                {
                    return false;
                }           
            }
            else if (buildUp == 6 || buildUp == 7 || buildUp == 8)
            {
                if (ParentObject.GetHPPercent() > 40)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (buildUp == 9 || buildUp == 10 || buildUp == 11)
            {
                if (ParentObject.GetHPPercent() > 45)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (buildUp == 12 || buildUp == 13 || buildUp == 14)
            {
                if (ParentObject.GetHPPercent() > 50)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (buildUp == 15 || buildUp == 16 || buildUp == 17)
            {
                if (ParentObject.GetHPPercent() > 55)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (buildUp == 18 || buildUp == 19 || buildUp == 20)
            {
                if (ParentObject.GetHPPercent() > 60)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


       


        public override bool FireEvent(Event E)
        {

            if( E.ID.Equals("InvisibleProvidence"))
            {
                Cell cell = PickDestinationCell(range, AllowVis.OnlyVisible, Locked: false, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: false, PickTarget.PickStyle.EmptyCell, null, Snap: true);
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

                        Popup.ShowFail("You can't use Invisible Providence on nothing.");

                    }
                    return false;
                }

                if (ParentObject.OnWorldMap())
                {
                    Popup.ShowFail("You cannot use this on the world map");
                    return false;
                }

                bool temp = canUse();
                if(temp == true)
                {
                    UseEnergy(500, "Authority Mutation InvisibleProvidence");
                    BeginInvisibleProvidence(gameObject);
                }
                else
                {
                    Popup.ShowFail("You attempt to use Invisible Providence but the Sloth Witchfactor stirs and refuses to lend you its strength fearing the death of its host.");
                }
               
                    
                
                

                
                

            }



            return base.FireEvent(E);
        }

        public bool Kickback()
        {
            if (buildUp >= 18)
            {
                int a = Stat.Random(0, 11);
                DamageOrBleed(1, 6);
                T6Kickback(a);
                return true;
            }
            else
            {
                if (buildUp >= 15)
                {
                    int a = Stat.Random(0, 11);
                    DamageOrBleed(1, 5);
                    T5Kickback(a);
                    return true;
                }
                else
                {
                    if (buildUp >= 12)
                    {
                        int a = Stat.Random(0, 11);
                        DamageOrBleed(1, 4);
                        T4Kickback(a);
                        return true;
                    }
                    else
                    {
                        if (buildUp >= 9)
                        {
                            int a = Stat.Random(0, 11);
                            DamageOrBleed(1, 3);
                            T3Kickback(a);
                            return true;
                        }
                        else
                        {
                            if (buildUp >= 6)
                            {
                                int a = Stat.Random(0, 11);
                                DamageOrBleed(1, 2);
                                T2Kickback(a);
                                return true;
                            }
                            else
                            {
                                if (buildUp >= 3)
                                {
                                    int a = Stat.Random(0, 11);
                                    DamageOrBleed(1, 1);
                                    T1Kickback(a);
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

           
            

            
        }

        public void DamageOrBleed(int x, int tier)
        {
            int damage = 0; 
            

            
            if (x == 0)
            {

            }
            else
            {
                if (tier == 1)
                {
                    damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.30));
                }
                else if (tier == 2)
                {
                    damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.40));
                }
                else if (tier == 3)
                {
                    damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.45));
                }
                else if (tier == 4)
                {
                    damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.50));
                }
                else if (tier == 5)
                {
                    damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.55));
                }
                else
                {
                    damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.60));
                }
            }

            ParentObject.TakeDamage(ref damage);
            IComponent<GameObject>.AddPlayerMessage("Your brain trembles as you take " + damage.ToString() + " damage from overstimulus as a consequence of Invisible Providence.");

        }

        public void T1Kickback(int val)
        {
            //public Bleeding(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
            int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.10));
            if(damage < 1)
            {
                damage = 1;
            }
            switch (val)
            {
                default:
                    break;
                case 0:
                    ApplyKickback(0, 1, 20);
                    break;
                case 1:
                    ApplyKickback(0, 1, 20);
                    break;
                case 2:
                    ApplyKickback(0, 1, 20);
                    break;
                case 3:
                    ApplyKickback(0, 1, 20);
                    break;
                case 4:
                    ApplyKickback(0, 1, 20);
                    break;
                case 5:
                    ApplyKickback(0, 1, 20);
                    break;
                case 6:
                    
                    break;
                case 7:
                    
                    break;
                case 8:
                    
                    break;    
                case 9:
                    
                    break;                  
                case 10:

                    break;
                case 11:

                    break;
            }
        }

        public void T2Kickback(int val)
        {
            //public Bleeding(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
            int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.15));
            int minorDamage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.05));
            if (damage < 1)
            {
                damage = 1;
            }
            switch (val) //Bleed(50%) + Ill(33.3%) + Nothing(16.7%)
            {
                default:
                    break;
                case 0:
                    ApplyKickback(0, 1, 20);
                    break;
                case 1:
                    ApplyKickback(0, 1, 20);
                    break;
                case 2:
                    ApplyKickback(0, 1, 20);
                    break;
                case 3:
                    ApplyKickback(0, 1, 20);
                    break;
                case 4:
                    ApplyKickback(0, 1, 20);
                    break;
                case 5:
                    ApplyKickback(0, 1, 20);
                    break;
                case 6:
                    ApplyKickback(2, 10, 20);
                    break;
                case 7:
                    ApplyKickback(2, 10, 20);
                    break;
                case 8:
                    ApplyKickback(2, 10, 20);
                    break;
                case 9:
                    ApplyKickback(2, 10, 20);
                    break;
                case 10:
                    
                    break;
                case 11:
                    
                    break;
             
            }
        }

        public void T3Kickback(int val)
        {
            //public Bleeding(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
            int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.20));
            int minorDamage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.05));
            if (damage < 1)
            {
                damage = 1;
            }
            switch (val) //Bleed(50%) + Ill(50%)
            {
                default:
                    ApplyKickback(0, 1, 25);
                    break;
                case 0:
                    ApplyKickback(0, 1, 25);
                    break;
                case 1:
                    ApplyKickback(0, 1, 25);
                    break;
                case 2:
                    ApplyKickback(0, 1, 25);
                    break;
                case 3:
                    ApplyKickback(0, 1, 25);
                    break;
                case 4:
                    ApplyKickback(0, 1, 25);
                    break;
                case 5:
                    ApplyKickback(0, 1, 25);
                    break;
                case 6:
                    ApplyKickback(2, 20, 20);
                    break;
                case 7:
                    ApplyKickback(2, 20, 20);
                    break;
                case 8:
                    ApplyKickback(2, 20, 20);
                    break;
                case 9:
                    ApplyKickback(2, 20, 20);
                    break;
                case 10:
                    ApplyKickback(2, 20, 20);
                    break;
                case 11:
                    ApplyKickback(2, 20, 20);
                    break;
              

            }
        }

        public void T4Kickback(int val)
        {
            //public Bleeding(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
            int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.25));
            int minorDamage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.05));
            if (damage < 1)
            {
                damage = 1;
            }
            switch (val) //Bleed(50%) + Vomit+Ill(25%) + Confusion(12.75%) + Ill(8.3%)
            {
                default:                  
                    break;
                case 0:
                    ApplyKickback(0, 2, 20);
                    break;
                case 1:
                    ApplyKickback(0, 2, 20);
                    break;
                case 2:
                    ApplyKickback(0, 2, 20);
                    break;
                case 3:
                    ApplyKickback(0, 2, 20);
                    break;
                case 4:
                    ApplyKickback(0, 2, 20);
                    break;
                case 5:
                    ApplyKickback(0, 2, 20);
                    break;
                case 6:
                    ApplyKickback(2, 25, 25);
                    ApplyKickback(3, 1, 20);
                    break;
                case 7:
                    ApplyKickback(2, 25, 25);
                    ApplyKickback(3, 1, 20);
                    break;
                case 8:
                    ApplyKickback(2, 25, 25);
                    ApplyKickback(3, 1, 20);
                    break;
                case 9:
                    ApplyKickback(2, 25, 25);
                    break;
                case 10:
                    ApplyKickback(4, 1, 7);
                    break;
                case 11:
                    ApplyKickback(4, 1, 7);
                    break;


            }


        }

        public void T5Kickback(int val)
        {
            //public Bleeding(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
            int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.25));
            int minorDamage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.05));
            if (damage < 1)
            {
                damage = 1;
            }
            switch (val) //Bleed(50%) + Vomit+Ill(25%) + Confusion(25%)
            {
                default:
                    break;
                case 0:
                    ApplyKickback(0, 2, 25);
                    break;
                case 1:
                    ApplyKickback(0, 2, 25);
                    break;
                case 2:
                    ApplyKickback(0, 2, 25);
                    break;
                case 3:
                    ApplyKickback(0, 2, 25);
                    break;
                case 4:
                    ApplyKickback(0, 2, 25);
                    break;
                case 5:
                    ApplyKickback(0, 2, 25);
                    break;
                case 6:
                    ApplyKickback(2, 30, 20);
                    ApplyKickback(3, 1, 20);
                    break;
                case 7:
                    ApplyKickback(2, 30, 20);
                    ApplyKickback(3, 1, 20);
                    break;
                case 8:
                    ApplyKickback(2, 30, 20);
                    ApplyKickback(3, 1, 20);
                    break;
                case 9:
                    ApplyKickback(4, 1, 7);
                    break;
                case 10:
                    ApplyKickback(4, 1, 7);
                    break;
                case 11:
                    ApplyKickback(4, 1, 7);
                    break;


            }


        }

        public void T6Kickback(int val)
        {
            //public Bleeding(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
            int damage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.25));
            int minorDamage = (int)Math.Floor((decimal)ParentObject.hitpoints * (decimal)(.05));
            if (damage < 1)
            {
                damage = 1;
            }
            switch (val) //Bleed[50%]
            {
                default:
                    break;
                case 0:
                    ApplyKickback(0, 2, 30);
                    break;
                case 1:
                    ApplyKickback(0, 2, 30);
                    break;
                case 2:
                    ApplyKickback(0, 2, 30);
                    break;
                case 3:
                    ApplyKickback(0, 2, 30);
                    break;
                case 4:
                    ApplyKickback(0, 2, 30);
                    break;
                case 5:
                    ApplyKickback(0, 2, 30);
                    break;
                case 6:
                    ApplyKickback(2, 1, 20);
                    ApplyKickback(3, 1, 20);
                    break;
                case 7:
                    ApplyKickback(2, 1, 20);
                    ApplyKickback(3, 1, 20);
                    break;
                case 8:
                    ApplyKickback(2, 1, 20);
                    ApplyKickback(3, 1, 20);
                    break;
                case 9:
                    ApplyKickback(4, 1, 7);
                    break;
                case 10:
                    ApplyKickback(4, 1, 7);
                    break;
                case 11:
                    ApplyKickback(5, 1, 7);

                    break;


            }


        }

        public bool ApplyKickback(int prompt, int dam, int save)
        {
            if(prompt == 0)
            {
                ParentObject.ApplyEffect(new Bleeding(dam.ToString(), save, ParentObject, false));

                //IComponent<GameObject>.AddPlayerMessage("The strain of Invisible Providence causes you to bleed from your nose and ears as your brain trembles in pain");
                return true;
            }
            else if(prompt == 1)
            {
                ParentObject.TakeDamage(ref dam);
                IComponent<GameObject>.AddPlayerMessage("Your brain trembles as you take " + dam.ToString() + " damage from overstimulus as a consequence of Invisible Providence.");
                return true;
            }
            else if(prompt == 2) 
            {
                ParentObject.ApplyEffect(new Ill(dam));
                ParentObject.TakeDamage(ref dam);
                IComponent<GameObject>.AddPlayerMessage("You become ill for the next 10 turns as backlash from your authority");
                return true;
            }
            else if(prompt == 3)
            {
                //XRL.World.Parts.Mutations mutations = target.GetPart("Mutations") as XRL.World.Parts.Mutations;
                XRL.World.Parts.Stomach stomach = ParentObject.GetPart("Stomach") as XRL.World.Parts.Stomach;
                stomach.GetHungry();
                ParentObject.FireEvent(new Event("AddWater", "Amount", -30000, "Forced", 1, "External", 1));
                //LiquidVolume.getLiquid("putrid").Drank(null, 0, ParentObject, new StringBuilder());
                //ParentObject.TakeDamage(ref dam);
                //IComponent<GameObject>.XDidY(ParentObject, "vomit", null, "!", null, null, ParentObject);
                IComponent<GameObject>.AddPlayerMessage("You hurl the contents of your stomach violently due to the strain of using your authority.");
            }
            else if(prompt == 4)
            {
                //public Confused(int Duration, int Level, int MentalPenalty)
                int Level = (int)Math.Min(10.0, Math.Floor((double)(30 - 1) / 2.0 + 3.0));
                int Penalty = (int)Math.Min(10.0, Math.Floor((double)(30 - 1) / 2.0 + 3.0));
                ParentObject.ApplyEffect(new Confused(dam, Level, Penalty));
                IComponent<GameObject>.AddPlayerMessage("You become confused as Invisible Providence causes you to have a seizure");
            }
            else if (prompt == 5)
            {

                ParentObject.ApplyEffect(new Monochrome());
                IComponent<GameObject>.AddPlayerMessage("Your excessive use of your authority gives you a seizure and your ability to see color is lost.");
            }
            return false;
        }

        /*
        public override bool HandleEvent(EndTurnEvent E)
        {
            if(ParentObject.HasPart("SageCandidate"))
            {
                int a = Stat.Random(0, 150);
                if(a == 1)
                {
                    if(buildUp > 0)
                    {
                        buildUp--;
                    }
                }
            }
            return false;
        }
        */



        public override bool CanLevel()
        {
            return false;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ParentObject.GainWillpower(1);

            ObtainAuthority();
            
          
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref InvisibleProvidenceID);
            ParentObject.GainWillpower(-1);
            RecountEvent.Send(ParentObject);
            return base.Unmutate(GO);
        }


        public bool AddAuthority(string name)
        {
            
            if (name.Equals("InvisibleProvidence"))
            {
                InvisibleProvidenceID = AddMyActivatedAbility("Invisible Providence", "InvisibleProvidence", "Authority:Sloth", "Awakened from the Sloth Witchfactor, you've become aware of an extendable telekinetic hand made of shadow unseen to others that you can summon to attack an enemy for (15 + your level) damage. The hand cannot be seen by enemies and is physically transiant, meaning it will ignore both AV and DV. Each time Invisible Providence is used, the user gains 1 point of buildup and in total can have 0-20 buildup at a time. Upon obtaining XP, there is a 1/50 chance of removing a point of buildup. There are 6 tiers of buildup in the following ranges: T0[0-2], T1[3-5], T2[6-8], T3[9-11], T4[12-14], T5[15-17], T6[18-20]. Usage of Invisible Providence costs T0[0%]/T1[30%]/T2[40%]/T3[45%]/T4[50%]/T5[55%]/T6[60%] of your max HP. An additional affect will be applied to player; the following listing is the tier, effects and chances: T0[Nothing], T1[Bleed(50%)/Nothing(50%)], T2[Bleed(50%)/Ill(33.3%)/Nothing(16.7%)], T3[Bleed(50%)/Ill(50%)], T4[Bleed(50%)/Vomit+Ill(25%)/Confusion(12.75%)/Ill(8.3%)], T5[Bleed(50%)/Vomit+Ill(25%)/Confusion(25%)], T6[Bleed(50%)/Vomit+Ill(25%)/Confusion(16.7%)/Monochrome(8.3%)].", "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false); InvisibleProvidenceEntry = MyActivatedAbility(InvisibleProvidenceID); InvisibleProvidenceEntry.DisplayName = "Invisible Providence:[" + buildUp.ToString() + "/20 | T0]"; Authorities.Add("InvisibleProvidence");
                return true;
            }
            return false;
        }

        public void SynchronizeIP()
        {
            InvisibleProvidenceEntry.DisplayName = "Invisible Providence:[" + buildUp.ToString() + "/20]";
        }

        public bool ObtainAuthority()
        {
            List<string> MissingAuthorities = new List<string>();
            XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations") as XRL.World.Parts.Mutations;
            if (Authorities.Contains("InvisibleProvidence"))
            {

            }
            else
            {
                MissingAuthorities.Add("InvisibleProvidence");
            }

            if (MissingAuthorities.Count > 0)
            {
                int a = Stat.Random(0, MissingAuthorities.Count - 1);
                //Popup.Show(MissingAuthorities[a].ToString());

                switch (MissingAuthorities[a])
                {
                    default:
                        break;
                    case "InvisibleProvidence":
                        AddAuthority(MissingAuthorities[a]);
                        //CheckpointEvent.Send(ParentObject);
                        Authorities.Add(MissingAuthorities[a]);
                        return true;
                }
            }

            return false;
        }

    }
    }




 