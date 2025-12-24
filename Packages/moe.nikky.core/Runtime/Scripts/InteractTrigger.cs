using nikkyai.ArrayExtensions;
using nikkyai.common;
using nikkyai.driver;
using Texel;
using UdonSharp;
using UnityEngine;

namespace nikkyai
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InteractTrigger : ACLBase
    {
        [Header("Drivers")] // header
        [SerializeField] private Transform triggerHolder;
        [SerializeField] private Transform isAuthorizedIndicator;

        protected override string LogPrefix => $"{nameof(InteractTrigger)} {name}";
    
        private TriggerDriver[] _triggerDrivers = { };
        private BoolDriver[] _isAuthorizedBoolDrivers = { };

        void Start()
        {
            _EnsureInit();   
        }

        protected override void _Init()
        {
            if (triggerHolder == null)
            {
                triggerHolder = this.transform;
            }

            _triggerDrivers = triggerHolder.GetComponentsInChildren<TriggerDriver>();
            if (isAuthorizedIndicator)
            {
                _isAuthorizedBoolDrivers = isAuthorizedIndicator.GetComponentsInChildren<BoolDriver>();
            }
        }

        protected override void AccessChanged()
        {
            DisableInteractive = !isAuthorized;
        
            for (var i = 0; i < _isAuthorizedBoolDrivers.Length; i++)
            {
                _isAuthorizedBoolDrivers[i].UpdateBool(isAuthorized);
            }
        }

        public override void Interact()
        {
            if (!isAuthorized) return;
            for (var i = 0; i < _triggerDrivers.Length; i++)
            {
                _triggerDrivers[i].Trigger();
            }
        }
    }
}
