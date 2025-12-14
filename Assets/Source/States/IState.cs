namespace NoScope.States
{
    public interface IState
    {
        public void Enter();
        public IState? Execute();
        public void Exit();
    }
}
