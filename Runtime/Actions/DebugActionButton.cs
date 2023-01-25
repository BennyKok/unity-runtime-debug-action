using System;
using UnityEngine;
using UnityEngine.Events;

namespace BennyKok.RuntimeDebug
{
    /// <summary>
    /// Button action will be represented as a simple button in the debug menu
    /// </summary>
    /// <example>
    /// <code>
    /// RuntimeDebugSystem.RegisterActions(
    ///     DebugActionBuilder.Button()
    ///         .WithName("Quit")
    ///         .WithAction(() => Application.Quit())
    /// );
    /// </code>
    /// </example>
    [Serializable]
    public class DebugActionButton : FluentAction<DebugActionButton>
    {

    }

}