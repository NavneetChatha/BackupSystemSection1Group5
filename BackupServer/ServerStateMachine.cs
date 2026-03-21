using System;

namespace BackupServer
{
    public enum ServerState
    {
        IDLE,
        RECEIVING_BACKUP,
        STORING_DATA,
        SENDING_RESTORE,
        MAINTENANCE
    }

    public class ServerStateMachine
    {
        private ServerState currentState;

        public ServerStateMachine()
        {
            currentState = ServerState.IDLE;
        }

        public ServerState GetCurrentState()
        {
            return currentState;
        }

        public void HandleCommand(CommandType command)
        {
            switch (command)
            {
                case CommandType.START_BACKUP:
                    if (currentState == ServerState.IDLE)
                        TransitionTo(ServerState.RECEIVING_BACKUP);
                    break;

                case CommandType.STORE_COMPLETE:
                    if (currentState == ServerState.RECEIVING_BACKUP)
                        TransitionTo(ServerState.STORING_DATA);
                    break;

                case CommandType.REQUEST_RESTORE:
                    if (currentState == ServerState.IDLE)
                        TransitionTo(ServerState.SENDING_RESTORE);
                    break;

                case CommandType.ENTER_MAINTENANCE:
                    TransitionTo(ServerState.MAINTENANCE);
                    break;

                case CommandType.EXIT_MAINTENANCE:
                    if (currentState == ServerState.MAINTENANCE)
                        TransitionTo(ServerState.IDLE);
                    break;

                default:
                    Console.WriteLine("Unknown command received.");
                    break;
            }
        }

        private void TransitionTo(ServerState newState)
        {
            Console.WriteLine($"State changed: {currentState} -> {newState}");
            currentState = newState;
        }

        public void ResetToIdle()
        {
            Console.WriteLine($"Resetting state to IDLE from {currentState}");
            currentState = ServerState.IDLE;
        }
    }
}