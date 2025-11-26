namespace GameCore.Process
{
    public interface IProcess
    {
        void Init();

        void Enter();

        void Leave();

        void Update();
    }
}