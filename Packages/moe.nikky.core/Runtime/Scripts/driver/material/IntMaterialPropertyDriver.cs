using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace nikkyai.driver.material
{
    public class IntMaterialPropertyDriver : IntDriver
    {
        [SerializeField] private Material[] materials;
        [SerializeField] private string[] propertyNames = { };
        private int[] propertyIds = { };
        protected override string LogPrefix => nameof(IntMaterialPropertyDriver);
        private void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();
            propertyIds = new int[propertyNames.Length];
            for (var i = 0; i < propertyNames.Length; i++)
            {
                propertyIds[i] = VRCShader.PropertyToID(propertyNames[i]);
            }
        }

        public override void UpdateInt(int value)
        {
            for (var i = 0; i < materials.Length; i++)
            {
                materials[i].SetInt(propertyIds[value], value);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                materials[i].MarkDirty();
#endif
            }
        }
    
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyIntValue(int value)
        {
            UpdateInt(value);
        }
#endif
    }
}
