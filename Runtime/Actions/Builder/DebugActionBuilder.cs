namespace BennyKok.RuntimeDebug.Actions
{
    /// <summary>
    /// Builder class for built-in <see cref="BaseDebugAction"/> derived class
    /// </summary>
    public static class DebugActionBuilder
    {
        /// <summary>
        /// Create a new <see cref="DebugActionButton"/> instance
        /// </summary>
        /// <returns>A new <see cref="DebugActionButton"/> instance</returns>
        public static DebugActionButton Button() => new DebugActionButton();

        /// <summary>
        /// Create a new <see cref="DebugActionToggle"/> instance
        /// </summary>
        /// <returns>A aew <see cref="DebugActionToggle"/> instance</returns>
        public static DebugActionToggle Toggle() => new DebugActionToggle();

        /// <summary>
        /// Create a new <see cref="DebugActionEnum"/> instance
        /// </summary>
        /// <returns>A aew <see cref="DebugActionEnum"/> instance</returns>
        public static DebugActionEnum Enum() => new DebugActionEnum();

        /// <summary>
        /// Create a new <see cref="DebugActionInput"/> instance
        /// </summary>
        /// <returns>A new <see cref="DebugActionInput"/> instance</returns>
        public static DebugActionInput Input() => new DebugActionInput();

        /// <summary>
        /// Create a new <see cref="DebugActionFlag"/> instance
        /// </summary>
        /// <returns>A new <see cref="DebugActionFlag"/> instance</returns>
        public static DebugActionFlag Flag() => new DebugActionFlag();
    }
}