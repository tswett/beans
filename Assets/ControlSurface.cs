using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script which makes a hinge joint controllable, as a control surface.
/// </summary>
/// <remarks>
/// This simply sets the hinge's target position to the coefficient times the control input.
/// </remarks>
public class ControlSurface : MonoBehaviour
{
    public string AxisName;
    public float Coefficient;
    new HingeJoint hingeJoint;

    void Start()
    {
        hingeJoint = GetComponent<HingeJoint>();
    }

    void Update()
    {
        float controlInput = Input.GetAxis(AxisName);

        if (AxisName == "Aileron")
            Debug.Log(string.Format("Axis {0}, input {1}", AxisName, controlInput));

        JointSpring hingeSpring = hingeJoint.spring;
        hingeSpring.targetPosition = Coefficient * controlInput;
        hingeJoint.spring = hingeSpring;
    }
}
