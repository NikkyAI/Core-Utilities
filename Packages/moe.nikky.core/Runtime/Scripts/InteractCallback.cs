using nikkyai.common;
using Texel;
using UdonSharp;
using UnityEngine;

namespace nikkyai
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InteractCallback : ACLBase
    { 
        [SerializeField, HideInInspector] private int index;

        public int Index
        {
            get => index;
            set => index = value;
        }

        #region ACL
        [Header("Access Control")] // header
        public bool enforceACL;

        protected override bool EnforceACL
        {
            get => enforceACL;
            set => enforceACL = value;
        }

        [Tooltip("ACL used to check who can use the toggle")] [SerializeField]
        public AccessControl accessControl;


        protected override AccessControl AccessControl
        {
            get => accessControl;
            set => accessControl = value;
        }

        #endregion

        #region Debug
        
        [Header("Debug")] // header
        [SerializeField]
        private DebugLog debugLog;

        protected override DebugLog DebugLog
        {
            get => debugLog;
            set => debugLog = value;
        }
        protected override string LogPrefix => nameof(InteractCallback);

        #endregion

        public const int EVENT_INTERACT = 0;

        public const int EVENT_RELEASE = 1;
        const int EVENT_COUNT = 2;

        protected override int EventCount
        {
            get => EVENT_COUNT;
        }

        void Start()
        {
            _EnsureInit();
        }

        protected override void AccessChanged()
        {
            DisableInteractive = !isAuthorized;
        }

        private bool _isInteracting = false;
        public override void Interact()
        {
            _isInteracting = true;
            _UpdateHandlers(EVENT_INTERACT, index);
        }

        public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (!_isInteracting) return;
            if (!isAuthorized) return;
            if (!value)
            {
                _isInteracting = false;
                _UpdateHandlers(EVENT_RELEASE, index);
            }
        }
    }
}