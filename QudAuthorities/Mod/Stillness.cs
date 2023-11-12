
using System;

using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects
{
    public class Stillness : Effect
    {
        public int x;

        public int y;

        public string zone;

        public Stillness()
        {
        }

        public Stillness(int Duration)
            : this()
        {
            base.Duration = Duration;
        }

        public override bool SameAs(Effect e)
        {
            return false;
        }

        public override string GetDetails()
        {
            
            return "The time of the affected has been stopped";
        }

        public override string GetDescription()
        {
            return "{{B|Stillness}} ";
        }

        public override bool Apply(GameObject Object)
        {         
            ApplyStats();          
            return true;
        }

        public override void Remove(GameObject Object)
        {
          
            UnapplyStats();
        }

        private void ApplyStats()
        {
            base.StatShifter.SetStatShift("Speed", -(10000));
            //base.StatShifter.SetStatShift("Energy", -base.Object.Energy.BaseValue);
        }

        private void UnapplyStats()
        {
            base.StatShifter.RemoveStatShifts(base.Object);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            
            if (ID == PlayerTurnPassedEvent.ID)
            {
                
                if (base.Duration > 0)
                {
                    if (base.Duration != 9999)
                    {
                        base.Duration--;
                    }
                    if (base.Duration <= 0)
                    {
                        if (base.Object.IsPlayer())
                        {
                            Popup.Show("Suddenly you blink, and realize time has passed instantly without you knowing. Time was stopped for " + Duration.ToString() + " turns");
                        }
                        base.Object.RemoveEffect(this);
                    }

                }
                return false;
            }
            
            

                return true;
        }

        public override bool HandleEvent(BeginTakeActionEvent E)
        {
            
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(ZoneDeactivatedEvent E)
        {
            base.Object.RemoveEffect(this);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneThawedEvent E)
        {
            base.Object.RemoveEffect(this);
            return base.HandleEvent(E);
        }



        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent(this, "AttackerDealingDamage");
            Object.RegisterEffectEvent(this, "BeforeApplyDamage");
            Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
            Object.RegisterEffectEvent(this, "BeforeTemperatureChange");
            Object.RegisterEffectEvent(this, "Overdose");
            base.Register(Object);
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
            Object.UnregisterEffectEvent(this, "AttackerDealingDamage");
            Object.UnregisterEffectEvent(this, "BeforeApplyDamage");
            Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
            Object.UnregisterEffectEvent(this, "BeforeTemperatureChange");
            Object.UnregisterEffectEvent(this, "Overdose");
            base.Unregister(Object);
        }

        public override bool FireEvent(Event E)
        {

            if (E.ID == "BeforeDeepCopyWithoutEffects")
            {
                UnapplyStats();
            }
            else if (E.ID == "AfterDeepCopyWithoutEffects")
            {
                ApplyStats();
            }
            return base.FireEvent(E);
        }





   
    }
}