using nikkyai.toggle;
using UnityEngine;

namespace nikkyai.driver
{
    public class TriggerRandom : TriggerDriver
    {
        protected override string LogPrefix => nameof(TriggerRandom);

    
        [SerializeField] private ExclusiveToggle[] targetToggles;
        [SerializeField] private int minIndex = 0;
        [SerializeField] private int maxIndex = 16;

        public override void Trigger()
        {
            foreach (var exclusiveToggle in targetToggles)
            {
                // assign random index to exclusiveToggle
                int newIndex = exclusiveToggle.SyncedIndex;
                while (newIndex == exclusiveToggle.SyncedIndex)
                {
                    newIndex = Random.Range(minIndex, maxIndex);
                }
                exclusiveToggle.SyncedIndex = newIndex;
            }
        }
    }
}
