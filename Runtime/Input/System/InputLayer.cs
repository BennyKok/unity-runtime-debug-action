namespace BennyKok.RuntimeDebug.DebugInput
{
    public interface InputLayer
    {
        void Enable();
        void Disable();
        void Update();

        bool Check();

        bool IsConfirmAction();

        bool IsBackAction();

        bool IsMenuAction();

        bool IsKeyDown(string keycode);


        bool IsUpPressing();

        bool IsUpReleased();

        bool IsUp();


        bool IsDownPressing();

        bool IsDownReleased();

        bool IsDown();
    }
}