namespace InputSystem
{
    public interface IInputContext {
        /// <summary>
        /// Higher priority receives input first. Equal priorities: the most recently pushed wins.
        /// Suggested values: Exploration 0, Puzzle 20, Drag 40, Dialogue 50, Pause 100.
        /// </summary>
        int Priority { get; }
    
        GameAction BlockedActions { get; }
    
        /// <summary>
        /// Returns true if successfully consumed the event.
        /// </summary>
        bool Handle(ActionEvent e);
    }
}