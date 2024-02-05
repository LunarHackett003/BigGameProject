using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalCameraController : MonoBehaviour, IManagedBehaviour
{
    [SerializeField] Vector2 lookAngle;
    [SerializeField] float distance;

    bool initialised;
    Transform orbitPoint;
    Transform aimTarget;
    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] Transform cam;

    [SerializeField] Transform followTransform;
    [SerializeField] Vector3 followOffset;
    [SerializeField] Vector3 aimTargetPosition;
    [SerializeField, Range(0, 1)] float shoulderLerp = 1;

    public void Awake()
    {
        if (cam != null)
        {
            initialised = true;
            orbitPoint = new GameObject().transform;
            aimTarget = new GameObject().transform;
            aimTarget.parent = orbitPoint;
            cam.SetParent(orbitPoint);
            cam.localPosition = Vector3.back * distance;
            vcam.LookAt = aimTarget;
            
        }
        else
        {
            Debug.LogWarning("This camera controller has not been initialised!\nPlease check all necessary fields are assigned!", gameObject);
        }

    }
    private void OnEnable()
    {
        BehaviourManager.updateClients += ManagedUpdate;
        BehaviourManager.lateUpdateClients += ManagedLateUpdate;
        BehaviourManager.fixedUpdateClients += ManagedFixedUpdate;
    }
    private void OnDisable()
    {
        BehaviourManager.updateClients -= ManagedUpdate;
        BehaviourManager.lateUpdateClients -= ManagedLateUpdate;
        BehaviourManager.fixedUpdateClients -= ManagedFixedUpdate;
    }

    public void ManagedFixedUpdate()
    {
    }

    public void ManagedLateUpdate()
    {
        if (initialised)
        {
            orbitPoint.SetPositionAndRotation(followTransform.position, Quaternion.Euler(lookAngle));
            cam.localPosition = Vector3.back * distance + followOffset;
            aimTarget.localPosition = aimTargetPosition;
        }
    }

    public void ManagedUpdate()
    {
    }
    
    
}
