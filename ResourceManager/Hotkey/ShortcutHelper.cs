namespace ResourceManager.Hotkey
{
    public class ShortcutHelper
    {
        /// <summary>
        ///  Returns the string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
        /// </summary>
        /// <param name="hotkeys">The collection of configured shortcut keys.</param>
        /// <param name="commandCode">The required shortcut identifier.</param>
        /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>.</returns>
        public static string GetShortcutToolTip(IEnumerable<HotkeyCommand>? hotkeys, int commandCode)
        {
            return GetShortcutKey(hotkeys, commandCode).ToShortcutKeyToolTipString();
        }

        /// <summary>
        ///  Returns the string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
        /// </summary>
        /// <param name="hotkeys">The collection of configured shortcut keys.</param>
        /// <param name="commandCode">The required shortcut identifier.</param>
        /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>.</returns>
        public static string GetShortcutDisplay(IEnumerable<HotkeyCommand>? hotkeys, int commandCode)
        {
            return GetShortcutKey(hotkeys, commandCode).ToShortcutKeyToolTipString();
        }

        /// <summary>
        ///  Returns the string representation of <paramref name="commandCode"/> if it exists in <paramref name="hotkeys"/> collection.
        /// </summary>
        /// <param name="hotkeys">The collection of configured shortcut keys.</param>
        /// <param name="commandCode">The required shortcut identifier.</param>
        /// <returns>The string representation of the shortcut, if exists; otherwise, the string representation of <see cref="Keys.None"/>.</returns>
        public static Keys GetShortcutKey(IEnumerable<HotkeyCommand>? hotkeys, int commandCode)
        {
            return hotkeys?.FirstOrDefault(h => h.CommandCode == commandCode)?.KeyData ?? Keys.None;
        }
    }
}
