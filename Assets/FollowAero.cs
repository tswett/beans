using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script which is intended to be attached to a camera. This script makes the camera "follow"
/// ahead of a target object, maintaining a constant distance from the target object, looking
/// directly at the object in the direction of the relative wind.
/// </summary>
public class FollowAero : MonoBehaviour {
    public Rigidbody TargetObject;
    public float Distance;

    void Start()
    {
    }

    void FixedUpdate()
    {
        if (TargetObject.velocity.magnitude == 0.0f)
            return;

        Vector3 targetDirection = TargetObject.velocity.normalized;
        Vector3 cameraPosition = TargetObject.position + Distance * targetDirection;

        transform.position = cameraPosition;
        transform.LookAt(TargetObject.position);
    }
}
