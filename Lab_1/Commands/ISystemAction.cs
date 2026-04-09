using System;

namespace Lab_1.Commands
{
    public interface ISystemAction
    {
        string ActionId { get; }
        DateTime Timestamp { get; }
        string LogMessage { get; } 
        string InitiatorLogin { get; } 

        void Execute();
        void Undo();
    }
}