using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interface for managed behaviour stuff. Helps with performance due to eliminating loads of magic methods.
/// <br></br>Used an interface this time so that I can also implement this on objects that inherit things other than monobehaviour, such as networked objects.
/// </summary>
public interface IManagedBehaviour
{

    public abstract void ManagedUpdate();
    public abstract void ManagedFixedUpdate();
    public abstract void ManagedLateUpdate();
}
