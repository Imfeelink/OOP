using System.Collections.Generic;
using System.Linq;
using Lab_1.Commands;

namespace Lab_1.Services
{
    public class ActionManager
    {
        private readonly JsonDataContext _context;

        //список логов
        public List<ISystemAction> History { get; private set; } = new List<ISystemAction>();

        public ActionManager(JsonDataContext context)
        {
            _context = context;
        }

        public void ExecuteAction(ISystemAction action)
        {
            action.Execute();
            History.Add(action);
            _context.SaveChanges();
        }

        public void UndoAction(ISystemAction action)
        {
            if (History.Contains(action))
            {
                action.Undo();
                History.Remove(action); 
                _context.SaveChanges();
            }
        }
    }
}