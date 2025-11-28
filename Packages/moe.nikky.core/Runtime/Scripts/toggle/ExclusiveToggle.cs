using System;
using System.Linq;
using System.Runtime.CompilerServices;
using nikkyai.ArrayExtensions;
using nikkyai.common;
using nikkyai.driver;
using nikkyai.Kinetic_Controls;
using Texel;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;

namespace nikkyai.toggle
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ExclusiveToggle : BaseSyncedBehaviour
    {
        [SerializeField, Min(0)] private int defaultIndex;
        [SerializeField] private bool clickOnActiveDisables = false;
        [SerializeField, Min(0)] private int disabledIndex = 0;
        [SerializeField] private int[] remapValues = { };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RemapIndex(int index)
        {
            if (remapValues.Length - 1 >= _syncedIndex)
            {
                return remapValues[_syncedIndex];
            }
            else
            {
                return index;
            }
        }

        [Header("Drivers")] // header
        [FormerlySerializedAs("drivers")]
        [SerializeField] private Transform selectionIndicator;
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

        [Tooltip("ACL used to check who can use the toggle"),
         SerializeField]
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

        protected override DebugLog DebugLog
        {
            get => debugLog;
            set => debugLog = value;
        }

        protected override string LogPrefix => $"{nameof(ExclusiveToggle)} {name}";

        #endregion

        private InteractCallback[] _interactCallbacks = { };
        private IntDriver[] _intDrivers;
        private BoolDriver[][] _boolDrivers;
        private BoolDriver[] _isAuthorizedBoolDrivers = { };


        [Header("State")] // header
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

        [UdonSynced]
        private int _syncedIndex;

        public int SyncedIndex
        {
            private set
            {
                var oldIndex = _syncedIndex;
                _syncedIndex = value;

                if (oldIndex != _syncedIndex)
                {
                    Log($"index changed {oldIndex} => {_syncedIndex}");
                    var remappedValue = RemapIndex(_syncedIndex);
                    // if (remapValues.Length - 1 >= _syncedIndex)
                    // {
                    //     remappedValue = remapValues[_syncedIndex];
                    // }

                    for (var i = 0; i < _intDrivers.Length; i++)
                    {
                        _intDrivers[i].UpdateInt(remappedValue);
                    }

                    var oldDrivers = _boolDrivers[oldIndex];
                    if (oldDrivers != null)
                    {
                        for (var i = 0; i < oldDrivers.Length; i++)
                        {
                            oldDrivers[i].UpdateBool(false);
                        }
                    }

                    var newDrivers = _boolDrivers[_syncedIndex];
                    if (newDrivers != null)
                    {
                        for (var i = 0; i < newDrivers.Length; i++)
                        {
                            newDrivers[i].UpdateBool(true);
                        }
                    }
                }
            }
            get => _syncedIndex;
        }

        private void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            SetupComponents();
            for (var i = 0; i < _interactCallbacks.Length; i++)
            {
                _interactCallbacks[i]._Register(
                    eventIndex: InteractCallback.EVENT_INTERACT,
                    handler: this,
                    eventName: nameof(_OnInteract),
                    args: nameof(_interactIndex)
                );
            }
        }

        private void SetupComponents()
        {
            _syncedIndex = defaultIndex;
            _intDrivers = selectionIndicator.GetComponents<IntDriver>()
                .AddRange(
                selectionIndicator.GetComponentsInChildren<IntDriver>()
                );
            _interactCallbacks = GetComponentsInChildren<InteractCallback>();
            _boolDrivers = new BoolDriver[_interactCallbacks.Length][];
            
            for (var i = 0; i < _interactCallbacks.Length; i++)
            {
                _interactCallbacks[i].Index = i;
                _boolDrivers[i] = _interactCallbacks[i].GetComponents<BoolDriver>()
                    .AddRange(
                    _interactCallbacks[i].GetComponentsInChildren<BoolDriver>()
                    );
            }
            if (isAuthorizedIndicator)
            {
                _isAuthorizedBoolDrivers = isAuthorizedIndicator.GetComponents<BoolDriver>()
                    .AddRange(
                        isAuthorizedIndicator.GetComponentsInChildren<BoolDriver>()
                    );
            }
        }

        protected override void AccessChanged()
        {
            for (var i = 0; i < _interactCallbacks.Length; i++)
            {
                _interactCallbacks[i].DisableInteractive = !isAuthorized;
            }
        }

        [NonSerialized] private int _interactIndex;
        public void _OnInteract()
        {
            if (!isAuthorized) return;

            TakeOwnership();
            Log($"interact {_interactIndex}");
            if(clickOnActiveDisables && SyncedIndex == _interactIndex)
            {
                SyncedIndex = disabledIndex;
            }
            else
            {
                SyncedIndex = _interactIndex;
            }
            if (synced)
            {
                RequestSerialization();
            }
            OnDeserialization();
        }

        public override void OnDeserialization()
        {
        }

        // ReSharper disable InconsistentNaming
        [NonSerialized] private int prevDefault = -1;
        [NonSerialized] private int[] prevRemap = { };
        [NonSerialized] private AccessControl prevAccessControl;
        [NonSerialized] private bool prevEnforceACL;
        [NonSerialized] private DebugLog prevDebugLog;
        [NonSerialized] private bool childrenInitialized = false;
        // ReSharper restore InconsistentNaming
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorUtility.SetDirty(this);

            if (!childrenInitialized
                || prevAccessControl != accessControl
                || prevEnforceACL != enforceACL
                || prevDebugLog != debugLog
               )
            {
                ApplyACLsAndLog();
                prevAccessControl = accessControl;
                prevDebugLog = debugLog;
                childrenInitialized = true;
            }

            if (prevDefault != defaultIndex
                || prevRemap.SequenceEqual(remapValues)
               )
            {
                ApplyValues();
                prevDefault = defaultIndex;
                prevRemap = remapValues;
            }
        }


        [ContextMenu("Apply Values")]
        public void ApplyValues()
        {
            SetupComponents();
            foreach (var intDriver in _intDrivers)
            {
                // var remappedValue = defaultIndex;
                // if (remapValues.Length - 1 >= defaultIndex)
                // {
                //     remappedValue = remapValues[defaultIndex];
                // }

                intDriver.ApplyIntValue(RemapIndex(defaultIndex));
                // intDriver.gameObject.MarkDirty();
            }

            for (var i = 0; i < _boolDrivers.Length; i++)
            {
                for (var j = 0; j < _boolDrivers[i].Length; j++)
                {
                    _boolDrivers[i][j].ApplyBoolValue(defaultIndex == i);
                    // _boolDrivers[i][j].gameObject.MarkDirty();
                }
            }
        }

        [ContextMenu("Apply ACLs and Log")]
        private void ApplyACLsAndLog()
        {
            var children = gameObject.GetComponentsInChildren<InteractCallback>(true);
            for (var index = 0; index < children.Length; index++)
            {
                var interactCallback = children[index];
                interactCallback.Index = index;
                interactCallback.EditorACL = accessControl;
                interactCallback.EditorDebugLog = debugLog;
                interactCallback.EditorEnforceACL = enforceACL;
                interactCallback.MarkDirty();
            }
        }
#endif
    }
}