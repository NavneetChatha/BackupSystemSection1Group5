using System;

namespace BackupServer
{
    /// <summary>
    /// Defines the operational states of the server.
    /// </summary>
    public enum ServerState
    {
        IDLE,
        RECEIVING_BACKUP,
        STORING_DATA,
        SENDING_RESTORE,
        MAINTENANCE
    }

    /// <summary>
    /// Manages the operational state machine of the server.
    /// State transitions are triggered by client commands only.
    /// </summary>
    public class ServerStateMachine
    {
        private ServerState currentState;
        private readonly object stateLock = new object();

        /// <summary>
        /// Initializes the state machine with a default state of idle.
        /// </summary>
        public ServerStateMachine()
        {
            currentState = ServerState.IDLE;
        }

        /// <summary>
        /// Returns the current state of the server.
        /// </summary>
        public ServerState GetCurrentState()
        {
            lock (stateLock)
            {
                return currentState;
            }
        }

        /// <summary>
        /// Handles a command received from the client and transitions state accordingly.
        /// </summary>
        /// <param name="command">The command received from the client.</param>
        public void HandleCommand(CommandType command)
        {
          lock(stateLock)
            {
                switch (command)
                {
                    case CommandType.START_BACKUP:
                        if (currentState == ServerState.IDLE)
                            TransitionTo(ServerState.RECEIVING_BACKUP);
                        else
                            Console.WriteLine($"Warning: Cannot START_BACKUP from state {currentState}. Command ignored.");
                        break;
                    case CommandType.STORE_COMPLETE:
                        if (currentState == ServerState.RECEIVING_BACKUP)
                            TransitionTo(ServerState.STORING_DATA);
                        else
                            Console.WriteLine($"Warning: Cannot STORE_COMPLETE from state {currentState}. Command ignored.");
                        break;
                    case CommandType.REQUEST_RESTORE:
                        if (currentState == ServerState.IDLE)
                            TransitionTo(ServerState.SENDING_RESTORE);
                        else
                            Console.WriteLine($"Warning: Cannot REQUEST_RESTORE from state {currentState}. Command ignored.");
                        break;
                    case CommandType.ENTER_MAINTENANCE:
                        TransitionTo(ServerState.MAINTENANCE);
                        break;
                    case CommandType.EXIT_MAINTENANCE:
                        if (currentState == ServerState.MAINTENANCE)
                            TransitionTo(ServerState.IDLE);
                        else
                            Console.WriteLine($"Warning: Cannot EXIT_MAINTENANCE from state {currentState}. Command ignored.");
                        break;
                    default:
                        Console.WriteLine("Unknown command received.");
                        break;
                }
            }
        }

        /// <summary>
        /// Transitions the server to a new state and logs the change.
        /// </summary>
        /// <param name="newState">The new state to transition to.</param>
        private void TransitionTo(ServerState newState)
        {
            Console.WriteLine($"State changed: {currentState} -> {newState}");
            currentState = newState;
        }

        /// <summary>
        /// Resets the server state back to idle.
        /// </summary>
        public void ResetToIdle()
        {
            lock (stateLock)
            {
                Console.WriteLine($"Resetting state to IDLE from {currentState}");
                currentState = ServerState.IDLE;
            }
        }
    }
}