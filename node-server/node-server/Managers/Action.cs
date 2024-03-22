using System.Reflection.Metadata.Ecma335;

namespace NodeServer.Managers
{
    public class Action
    {
        public string ActionName { get; set; }

        public object[] Args { get; set; }

        public Action(string actionName, string args)
        {
            ActionName = actionName;
            string[] values = args.Trim('[', ']').Split(',');
            this.Args = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                int intValue;
                if (int.TryParse(values[i], out intValue))
                {
                    this.Args[i] = intValue;
                }
                else
                {
                    this.Args[i] = values[i];
                }
            }
        }
    }
}
