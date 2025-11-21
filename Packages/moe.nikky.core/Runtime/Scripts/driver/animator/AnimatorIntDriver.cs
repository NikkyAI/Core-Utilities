using UnityEngine;

namespace nikkyai.driver.animator
{
    public class AnimatorIntDriver : IntDriver
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string intParameterName;
        protected override string LogPrefix => $"AnimatorIntDriver {name}";

        public override void UpdateInt(int value)
        {
            animator.SetInteger(intParameterName, value);
        }
    }
}
