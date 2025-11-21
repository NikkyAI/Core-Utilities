using System;
using nikkyai.driver;
using nikkyai.toggle.common;
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
    public class SyncedToggle : ACLBase
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

        [SerializeField] private TextMeshPro tmpLabel;

        [Header("Access Control")] // header
        [SerializeField]
        private bool enforceACL = true;

        protected override bool EnforceACL
        {
            get => enforceACL;
            set => enforceACL = value;
        }

        [Tooltip("ACL used to check who can use the toggle"),
         SerializeField]
        private AccessControl accessControl;

        protected override AccessControl AccessControl
        {
            get => accessControl;
            set => accessControl = value;
        }

        [Header("Debug")] // header
        [SerializeField]
        private DebugLog debugLog;

        protected override DebugLog DebugLog
        {
            get => debugLog;
            set => debugLog = value;
        }

        protected override string LogPrefix => nameof(SyncedToggle);

        [Header("Internals")] // header
        // [Tooltip(
        //     "This GameObject gets turned off if `Off If Not Usable` is TRUE.\n\n!!MAKE SURE THERE ARE NO SCRIPTS ON THIS OBJECT!!\nscripts do not run if they get turned off.")]
        // [SerializeField]
        // private GameObject button;

        // [Tooltip(
        //     "This Collider gets turned off if `Off If Not Usable` is TRUE.\nIf you using UI buttons, leave this empty.")]
        // [SerializeField]
        // private Collider buttonCollider;
        
        [FormerlySerializedAs("state0")] [SerializeField] private Transform stateDisabledUnauthorized;
        [FormerlySerializedAs("state1")] [SerializeField] private Transform stateEnabledUnauthorized;
        [FormerlySerializedAs("state2")] [SerializeField] private Transform stateDisabledAuthorized;
        [FormerlySerializedAs("state3")] [SerializeField] private Transform stateEnabledAuthorized;

        [UdonSynced] private bool _isOn = false;

        public const int EVENT_UPDATE = 0;
        public const int EVENT_COUNT = 1;

        protected override int EventCount => EVENT_COUNT;

        private BoolDriver[] _boolDrivers = { };
        
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

            _boolDrivers = GetComponents<BoolDriver>();

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
            OnDeserialization();
        }

        protected override void AccessChanged()
        {
            DisableInteractive = !isAuthorized;
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

            RequestSerialization();
            OnDeserialization();
        }

        public void Reset()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            _isOn = defaultValue;
            RequestSerialization();
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
            RequestSerialization();
        }

        private void _UpdateState()
        {
            Log($"_UpdateState {_isOn}");
            // if (stateDisabledUnauthorized) stateDisabledUnauthorized.localScale = _isOn ? Vector3.zero : Vector3.one;
            // if (stateEnabledUnauthorized) stateEnabledUnauthorized.localScale = _isOn ? Vector3.one : Vector3.zero;
            // if (stateDisabledAuthorized) stateDisabledAuthorized.localScale = Vector3.zero;
            // if (stateEnabledAuthorized) stateEnabledAuthorized.localScale = Vector3.zero;
            
            for (var i = 0; i < _boolDrivers.Length; i++)
            {
                _boolDrivers[i].UpdateBool(_isOn);
            }

            _UpdateHandlers(EVENT_UPDATE, _isOn);

            if (isAuthorized)
            {
                SetState(_isOn ? 3 : 2);
            }
            else
            {
                SetState(_isOn ? 1 : 0);
            }
        }

        private void SetState(int state)
        {
            if (stateDisabledUnauthorized) stateDisabledUnauthorized.localScale = state == 0 ? Vector3.one : Vector3.zero;
            if (stateEnabledUnauthorized) stateEnabledUnauthorized.localScale = state == 1 ? Vector3.one : Vector3.zero;
            if (stateDisabledAuthorized) stateDisabledAuthorized.localScale = state == 2 ? Vector3.one : Vector3.zero;
            if (stateEnabledAuthorized) stateEnabledAuthorized.localScale = state == 3 ? Vector3.one : Vector3.zero;
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

            if (tmpLabel == null)
            {
                tmpLabel = transform.Find("Label").GetComponent<TextMeshPro>();
            }

            if (stateDisabledUnauthorized == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.EndsWith("S0"))
                    {
                        stateDisabledUnauthorized = child;
                        stateDisabledUnauthorized.localScale = Vector3.one;
                        stateDisabledUnauthorized.MarkDirty();
                        break;
                    }
                }
            }

            if (stateEnabledUnauthorized == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.EndsWith("S1"))
                    {
                        stateEnabledUnauthorized = child;
                        stateEnabledUnauthorized.MarkDirty();
                        break;
                    }
                }
            }

            if (stateDisabledAuthorized == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.EndsWith("S2"))
                    {
                        stateDisabledAuthorized = child;
                        stateDisabledAuthorized.MarkDirty();
                        break;
                    }
                }
            }

            if (stateEnabledAuthorized == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.EndsWith("S3"))
                    {
                        stateEnabledAuthorized = child;
                        stateEnabledAuthorized.MarkDirty();
                        break;
                    }
                }
            }

            if (stateDisabledUnauthorized)
            {
                stateDisabledUnauthorized.localScale = Vector3.one;
                stateDisabledUnauthorized.MarkDirty();
            }

            if (stateEnabledUnauthorized)
            {
                stateEnabledUnauthorized.localScale = Vector3.zero;
                stateEnabledUnauthorized.MarkDirty();
            }

            if (stateDisabledAuthorized)
            {
                stateDisabledAuthorized.localScale = Vector3.zero;
                stateDisabledAuthorized.MarkDirty();
            }

            if (stateEnabledAuthorized)
            {
                stateEnabledAuthorized.localScale = Vector3.zero;
                stateEnabledAuthorized.MarkDirty();
            }

            this.MarkDirty();
        }
#endif
    }
}