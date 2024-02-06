using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourManager : MonoBehaviour
{
    public delegate void updateClient();
    public static updateClient updateClients;
    public static updateClient fixedUpdateClients;
    public static updateClient lateUpdateClients;

    private void FixedUpdate()
    {
        fixedUpdateClients?.Invoke();
    }
    private void LateUpdate()
    {
        lateUpdateClients?.Invoke();
    }
    private void Update()
    {
        updateClients?.Invoke();
    }

    public static void Subscribe(updateClient update, updateClient lateupdate, updateClient fixedupdate)
    {
        updateClients += update;
        lateUpdateClients += lateupdate;
        fixedUpdateClients += fixedupdate;
    }
}
