using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Systems;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components
{
    public abstract class RuntimeDebugBehaviour : MonoBehaviour
    {
        private BaseDebugAction[] actions;

        protected virtual void Awake()
        {
            actions = RuntimeDebugSystem.RegisterActionsAuto(this);
        }

        protected virtual void OnDestroy()
        {
            RuntimeDebugSystem.UnregisterActions(actions);
        }
    }
}