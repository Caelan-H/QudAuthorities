using System;
using XRL.UI;

namespace XRL.World.Effects
{
    [Serializable]
    public class SoulCaptured : Effect
    {
        public GameObject inflictor = null;
        

        public SoulCaptured()
        {
            base.DisplayName = "{{R|SoulCaptured}}";
        }

        public SoulCaptured(GameObject inflictor)
            : this()
        {
            base.DisplayName = "{{R|SoulCaptured}}";
            this.inflictor= inflictor;
           
        }

        public override int GetEffectType()
        {
            return 33554944;
        }

        public override bool HandleEvent(BeforeDeathRemovalEvent E)
        {
            Popup.Show("BEFOREDEATHREMOVALEVENT");
            if(inflictor.HasPart("Lust"))
            {
                base.Object.RemoveEffect(this);
                base.Object.HandleEvent(E);
                XRL.World.Parts.Mutation.Lust lust = inflictor.GetPart("Lust") as XRL.World.Parts.Mutation.Lust;
                lust.capturedSoul = base.Object;
                Popup.Show("Soul should be captured");
            }
            
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(BeforeDieEvent E)
        {
            Popup.Show("BEFOREDIEREMOVALEVENT");
            

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(BeforeDestroyObjectEvent E)
        {
            Popup.Show("Before destroy object event");


            return base.HandleEvent(E);
        }

        

        public override bool Apply(GameObject Object)
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public override void Unregister(GameObject Object)
        {
            base.Unregister(Object);
        }

        public override bool FireEvent(Event E)
        {
            return true;
        }
    }
    
}
