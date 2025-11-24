using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace nikkyai.driver.material
{
    public class FloatMaterialPropertyDriver : FloatDriver
    {
        [SerializeField] private Material[] materials;
        [SerializeField] private string[] propertyNames = { };
        private int[] propertyIds = { };
    
        protected override string LogPrefix => nameof(FloatMaterialPropertyDriver);
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

        public override void UpdateFloat(float value)
        {
            for (var i = 0; i < materials.Length; i++)
            {
                materials[i].SetFloat(propertyIds[i], value);
            
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                materials[i].MarkDirty();
#endif
            }
        }
    
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyFloatValue(float value)
        {
            UpdateFloat(value);
        }
#endif
    }
}