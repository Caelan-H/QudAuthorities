using System;




namespace XRL.World.Effects
{
    [Serializable]
    public class StarEaten : Effect
    {
        public StarEaten()
        {
            base.DisplayName = "{{R|StarEaten}}";
        }

        public StarEaten(int Duration)
            : this()
        {
            base.Duration = DURATION_INDEFINITE;
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
