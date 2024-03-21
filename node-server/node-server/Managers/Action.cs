using System.Reflection.Metadata.Ecma335;

namespace NodeServer.Managers
{
    public class Action
    {
        public string ActionName { get; set; }

        public object[] Args { get; set; }

        Action(string actionName, string args)
        {
            ActionName = actionName;
            string[] values = args.Trim('[', ']').Split(',');
            this.Args = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                this.Args[i] = values[i];
            }
        }
    }
}
