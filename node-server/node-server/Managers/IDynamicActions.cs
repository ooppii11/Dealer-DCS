namespace NodeServer.Managers
{
    public abstract class IDynamicActions
    {
        public abstract Task<bool> NameToAction(Action ac);
    }
}
