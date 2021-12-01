using System.Collections.Generic;

public interface IAction
{
    public abstract void PerformAction();
    public abstract void PerformAction(List<int> inventory);
}
