using UnityEngine;
using System;

[Serializable]
public class QuestObjective
{
    [TextArea]
    public string description;

    public Transform target;   // what the arrow points to

    [HideInInspector]
    public bool completed;

    // How this objective is completed
    public Func<bool> completionCondition;
}
