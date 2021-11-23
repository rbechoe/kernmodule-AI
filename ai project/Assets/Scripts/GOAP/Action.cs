using System.Collections.Generic;

public abstract class Action
{
    public string name { get; set; }
    public int requiredItem { get; set; }
    public bool hasRequirement { get; set; }

    public abstract void PerformAction();
    public abstract void PerformAction(List<int> inventory);
}
