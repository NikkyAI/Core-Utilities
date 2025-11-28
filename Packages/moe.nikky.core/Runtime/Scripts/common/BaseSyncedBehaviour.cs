using System.Runtime.CompilerServices;
using nikkyai.common;
using nikkyai.driver;
using UnityEngine;
using VRC.SDKBase;

namespace nikkyai.Kinetic_Controls
{
    public abstract class BaseSyncedBehaviour: ACLBase
    {
        public abstract bool Synced { get; set; }
        
        public virtual void TakeOwnership()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
        }
    }
}