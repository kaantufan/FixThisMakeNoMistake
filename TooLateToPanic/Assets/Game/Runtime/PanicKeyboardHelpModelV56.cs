namespace TooLateToPanic
{
    public sealed partial class PanicModel
    {
        // Independent pause owner. Closing help never clears IsPaused or another UI owner.
        public bool KeyboardHelpOpenV56 { get; private set; }
        public bool SetKeyboardHelpOpenV56(bool open)
        {
            if (open && (IsPaused || WaveLedgerOpenV56 ||
                (Phase != PanicPhase.Preparing && Phase != PanicPhase.Tutorial && Phase != PanicPhase.Inspection))) return false;
            KeyboardHelpOpenV56 = open;
            return true;
        }
        void ResetKeyboardHelpV56() { KeyboardHelpOpenV56 = false; }
    }
}
