using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSurface : MonoBehaviour
{
    public string AxisName;
    public float Coefficient;
    HingeJoint hingeJoint;

    void Start()
    {
        hingeJoint = GetComponent<HingeJoint>();
    }

    void Update()
    {
        float controlInput = Input.GetAxis(AxisName);

        if (AxisName == "Rudder")
            Debug.Log(string.Format("Axis {0}, input {1}", AxisName, controlInput));

        JointSpring hingeSpring = hingeJoint.spring;
        hingeSpring.targetPosition = Coefficient * controlInput;
        hingeJoint.spring = hingeSpring;
    }
}
