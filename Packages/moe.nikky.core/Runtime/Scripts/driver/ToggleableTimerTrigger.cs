using UnityEngine;
using VRC.SDKBase;

namespace nikkyai.driver
{
    public class ToggleableTimerTrigger : BoolDriver
    {
        protected override string LogPrefix => nameof(ToggleableTimerTrigger);

        [SerializeField] [Min(5f)] private Vector2 delay = new Vector2(20.0f, 30.0f);

        [SerializeField] private bool onlyInstanceMaster = false;
        [SerializeField] private TriggerDriver[] triggerDrivers;

        private float _minDelay, _maxDelay;

        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            if (delay.x < delay.y)
            {
                _minDelay = delay.x;
                _maxDelay = delay.y;
            }
            else
            {
                _minDelay = delay.y;
                _maxDelay = delay.x;
            }
        }

        private bool _toggleState = false;

        private bool _timerRunning = false;

        public override void UpdateBool(bool value)
        {
            Log($"timer set {_toggleState} -> {value}");

            if (!_toggleState && value)
            {
                _toggleState = true;

                // start timer
                if (!_timerRunning)
                {
                    _timerRunning = true;
                    TriggerTimer();
                }
                else
                {
                    LogWarning("Timer already running");
                }
            }

            if (!value && _toggleState)
            {
                _toggleState = false;
            }
        }

        public void TriggerTimer()
        {
            Log("timer triggered");
            _timerRunning = false;

            if (onlyInstanceMaster && !Networking.IsMaster)
            {
                return;
            }

            if (_toggleState)
            {
                foreach (var triggerDriver in triggerDrivers)
                {
                    triggerDriver.Trigger();
                }

                // call timer on a delay
            }

            if (!_timerRunning)
            {
                _timerRunning = true;
                float nextDelay = Random.Range(_minDelay, _maxDelay);
                SendCustomEventDelayedSeconds(nameof(TriggerTimer), nextDelay);
            }
            else
            {
                LogWarning("too many timers already running or smth else broke");
            }
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            Debug.Log($"New master: {newMaster.displayName}");
        }
    }
}