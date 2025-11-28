using System;
using nikkyai.ArrayExtensions;
using nikkyai.common;
using nikkyai.driver;
using nikkyai.Kinetic_Controls;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Texel;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using VRC;
using VRC.Core;

namespace nikkyai.toggle
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedToggle : BaseSyncedBehaviour
    {
        [Tooltip(
             "The button will initialize into this value, toggle this for elements that should be enabled by default"),
         SerializeField]
        private bool defaultValue = false;

        // [Header("UI")] // header
        // [SerializeField]
        // private string label;
        // [SerializeField]
        // private string label2;

        
        [Header("Drivers")] // header
        [SerializeField] private Transform valueIndicator;

        [SerializeField] private Transform isAuthorizedIndicator;

        #region ACL

        [Header("Access Control")] // header
        [SerializeField]
        private bool enforceACL = true;

        protected override bool EnforceACL
        {
            get => enforceACL;
            set => enforceACL = value;
        }

        [Tooltip("ACL used to check who can use the toggle")] [SerializeField]
        private AccessControl accessControl;

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

        protected override string LogPrefix => $"{nameof(SyncedToggle)} {name}";

        protected override DebugLog DebugLog
        {
            get => debugLog;
            set => debugLog = value;
        }

        #endregion

        [Header("State")] // header
        
        [UdonSynced] private bool _isOn = false;

        [SerializeField, UdonSynced]
        private bool synced = true;

        public override bool Synced
        {
            get => synced;
            set
            {
                if (!isAuthorized) return;
                
                TakeOwnership();
                synced = value;
                
                RequestSerialization();
            }
        }
        
        public const int EVENT_UPDATE = 0;
        public const int EVENT_COUNT = 1;

        protected override int EventCount => EVENT_COUNT;

        // private BoolDriver[] _boolDrivers = { };
        
        private BoolDriver[] _valueBoolDrivers = { };
        private BoolDriver[] _isAuthorizedBoolDrivers = { };
        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            // if (button == null)
            // {
            //     button = gameObject;
            // }
            //
            // if (buttonCollider == null)
            // {
            //     buttonCollider = button.GetComponent<Collider>();
            // }

            DisableInteractive = true;

            // if (button != null) button.SetActive(!offIfNotUsable);
            // if (buttonCollider != null) buttonCollider.enabled = !offIfNotUsable;

            // if (!string.IsNullOrEmpty(label))
            // {
            //     var labelText = (label.Trim() + "\n" + label2.Trim()).Trim('\n', ' ');
            //     InteractionText = (label.Trim() + " " + label2.Trim()).Trim('\n', ' ', '-');
            //     if (tmpLabel)
            //     {
            //         tmpLabel.text = labelText;
            //     }
            // }

            _isOn = defaultValue;
            
            _valueBoolDrivers = valueIndicator.GetComponents<BoolDriver>()
                .AddRange(
                    valueIndicator.GetComponentsInChildren<BoolDriver>()
                );
            if (isAuthorizedIndicator)
            {
                _isAuthorizedBoolDrivers = isAuthorizedIndicator.GetComponents<BoolDriver>()
                    .AddRange(
                        isAuthorizedIndicator.GetComponentsInChildren<BoolDriver>()
                    );
            }
            
            OnDeserialization();
        }

        protected override void AccessChanged()
        {
            DisableInteractive = !isAuthorized;
            
            for (var i = 0; i < _isAuthorizedBoolDrivers.Length; i++)
            {
                _isAuthorizedBoolDrivers[i].UpdateBool(isAuthorized);
            }
            _UpdateState();
        }

        public void SetState(bool newValue)
        {
            if (enforceACL && !isAuthorized) return;
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _isOn = newValue;

            if (synced)
            {
                RequestSerialization();
            }
            OnDeserialization();
        }

        public void Reset()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _isOn = defaultValue;
            if (synced)
            {
                RequestSerialization();
            }
            OnDeserialization();
        }

        public override void Interact()
        {
            _Interact();
        }

        public void _Interact()
        {
            if (enforceACL && !isAuthorized) return;

            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _isOn = !_isOn;
            _UpdateState();
            if (synced)
            {
                RequestSerialization();
            }
        }

        private void _UpdateState()
        {
            if (!isAuthorized) return;
            Log($"_UpdateState {_isOn}");
            
            for (var i = 0; i < _valueBoolDrivers.Length; i++)
            {
                _valueBoolDrivers[i].UpdateBool(_isOn);
            }
        }

        public override void OnDeserialization()
        {
            _UpdateState();
        }

        [NonSerialized] private string prevLabel;
        [NonSerialized] private string prevLabel2;
        [NonSerialized] private TextMeshPro prevTMPLabel;

        // [Header("Editor Only")] // header
        // [SerializeField] private TMP_FontAsset fontAsset;
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorUtility.SetDirty(this);

            // TODO: check on localTransforms too
            // if (label != prevLabel || label2 != prevLabel2 || tmpLabel != prevTMPLabel)
            // {
            //     // To prevent trying to apply the theme to often, as without it every single change in the scene causes it to be applied
            //     prevLabel = label;
            //     prevLabel2 = label2;
            //     prevTMPLabel = tmpLabel;
            //
            //     ApplyValues();
            // }
        }

        [ContextMenu("Apply Values")]
        public void ApplyValues()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorUtility.SetDirty(this);

            // if (!string.IsNullOrEmpty(label))
            // {
            //     InteractionText = label;
            //     this.MarkDirty();
            //     // this.MarkDirty();
            //     if (tmpLabel != null)
            //     {
            //         var text = (label.Trim() + "\n" + label2.Trim()).Trim('\n', ' ');
            //         tmpLabel.text = text;
            //         tmpLabel.MarkDirty();
            //     }
            // }
        }

        [ContextMenu("Assign Defaults")]
        public void AssignDefaults()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorUtility.SetDirty(this);

            // if (button == null)
            // {
            //     button = gameObject;
            // }
            //
            // if (buttonCollider == null)
            // {
            //     buttonCollider = button.GetComponent<Collider>();
            // }

            this.MarkDirty();
        }
#endif
    }
}