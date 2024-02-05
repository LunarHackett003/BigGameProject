using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourManager : MonoBehaviour
{
    public delegate void u_UpdateClient();
    public delegate void u_FixedUpdateClient();
    public delegate void u_LateUpdateClient();

    public static u_UpdateClient updateClients;
    public static u_FixedUpdateClient fixedUpdateClients;
    public static u_LateUpdateClient lateUpdateClients;

    private void FixedUpdate()
    {
        fixedUpdateClients.Invoke();
    }
    private void LateUpdate()
    {
        lateUpdateClients.Invoke();
    }
    private void Update()
    {
        updateClients.Invoke();
    }
}
