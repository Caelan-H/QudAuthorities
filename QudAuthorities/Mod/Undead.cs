using System;
using XRL.UI;

namespace XRL.World.Effects
{
    [Serializable]
    public class Undead : Effect
    {
        public GameObject inflictor = null;
        

        public Undead()
        {
            base.DisplayName = "{{R|Undead}}";
        }

        public Undead(GameObject inflictor)
            : this()
        {
            base.DisplayName = "{{R|Undead}}";
            this.inflictor= inflictor;
           
        }

        public override int GetEffectType()
        {
            return 33554944;
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
