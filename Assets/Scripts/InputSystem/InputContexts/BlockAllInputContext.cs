namespace InputSystem.InputContexts
{
    public class BlockAllInputContext : IInputContext
    {
        public int Priority => 999;
        
        public GameAction BlockedActions => GameAction.All;
        
        public bool Handle(ActionEvent e) => true;
    }
}