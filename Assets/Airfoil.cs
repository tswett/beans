using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airfoil : MonoBehaviour {
    new Rigidbody rigidbody;
    new Collider collider;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }
    
    void Update()
    {

    }

    void FixedUpdate()
    {
        Vector3 velocity = rigidbody.velocity;
        Vector3 airfoilNormal = transform.up;

        float angleOfAttackDeg = Vector3.Angle(velocity, airfoilNormal) - 90.0f;

        float coefficientOfLift;
        float coefficientOfDrag;
        calculateCoefficients(angleOfAttackDeg, out coefficientOfLift, out coefficientOfDrag);

        float wingArea = 4 * collider.bounds.extents.x * collider.bounds.extents.z;

        float airDensity = 0.07962f; // density of air in pounds per cubic foot

        float lift = coefficientOfLift * wingArea * airDensity * velocity.sqrMagnitude / 2;
        float drag = coefficientOfDrag * wingArea * airDensity * velocity.sqrMagnitude / 2;

        Vector3 directionOfLift =
            Vector3.Cross(velocity, Vector3.Cross(airfoilNormal, velocity)).normalized;

        Vector3 directionOfDrag = (-velocity).normalized;

        rigidbody.AddForce(lift * directionOfLift + drag * directionOfDrag);
    }

    void calculateCoefficients(
        float angleOfAttackDeg, out float coefficientOfLift, out float coefficientOfDrag)
    {
        bool stalled = Mathf.Abs(angleOfAttackDeg) > 12.0f;

        if (stalled) 
            coefficientOfLift = 1.2f * Mathf.Sin(2 * Mathf.Deg2Rad * angleOfAttackDeg);
        else
            coefficientOfLift = angleOfAttackDeg / 10.0f;

        bool turbulent = Mathf.Abs(angleOfAttackDeg) > 10.0f;

        if (turbulent)
            coefficientOfDrag = 2.0f * Mathf.Sin(Mathf.Deg2Rad * angleOfAttackDeg);
        else
            coefficientOfDrag = 0.0f;
    }
}
