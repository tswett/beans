using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script which makes an object behave as an airfoil.
/// </summary>
/// <remarks>
/// Currently, this script assumes that the object is a flat rectangle whose sides are parallel
/// to its x and z axes.
/// </remarks>
public class Airfoil : MonoBehaviour {
    public bool DisplayStalls;
    public Material NormalMaterial;
    public Material StalledMaterial;
    public Camera AeroCamera;
    public Camera AeroPreviewCamera;

    int activeAirfoilLayer;
    new Rigidbody rigidbody;
    new Collider collider;
    MeshRenderer meshRenderer;
    FollowAero followAero;
    Texture2D intermediateTexture;

    void Start()
    {
        activeAirfoilLayer = LayerMask.NameToLayer("ActiveAirfoil");
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        followAero = AeroCamera.GetComponent<FollowAero>();

        if (AeroCamera != null)
        {
            RenderTexture aeroTexture = AeroCamera.targetTexture;
            intermediateTexture = new Texture2D(aeroTexture.width, aeroTexture.height);
        }
    }
    
    void FixedUpdate()
    {
        float exposure = calculateExposure();

        Vector3 velocity = rigidbody.velocity;
        Vector3 airfoilNormal = transform.up;

        float angleOfAttackDeg = Vector3.Angle(velocity, airfoilNormal) - 90.0f;

        float coefficientOfLift;
        float coefficientOfDrag;
        calculateCoefficients(angleOfAttackDeg, out coefficientOfLift, out coefficientOfDrag);

        float exposedWingArea =
            4.0f * exposure * collider.bounds.extents.x * collider.bounds.extents.z;

        float airDensity = 0.07962f; // density of air in pounds per cubic foot

        float lift = coefficientOfLift * exposedWingArea * airDensity * velocity.sqrMagnitude / 2;
        float drag = coefficientOfDrag * exposedWingArea * airDensity * velocity.sqrMagnitude / 2;

        Vector3 directionOfLift =
            Vector3.Cross(velocity, Vector3.Cross(airfoilNormal, velocity)).normalized;

        Vector3 directionOfDrag = (-velocity).normalized;

        rigidbody.AddForce(lift * directionOfLift + drag * directionOfDrag);
    }

    float calculateExposure()
    {
        if (AeroCamera == null)
            return 1.0f;

        // set this airfoil to a distinctive material
        Material[] originalMaterial = meshRenderer.materials;
        meshRenderer.materials = new[] { followAero.ActiveAirfoilMaterial };

        // calculate the total pixels by rendering just this airfoil
        int originalLayer = gameObject.layer;
        gameObject.layer = activeAirfoilLayer;
        int originalCullingMask = AeroCamera.cullingMask;
        AeroCamera.cullingMask = activeAirfoilLayer;
        float totalPixels = countActiveAirfoilPixels();

        // calculate the visible pixels by rendering the entire aircraft
        gameObject.layer = originalLayer;
        AeroCamera.cullingMask = originalCullingMask;
        float visiblePixels = countActiveAirfoilPixels();

        if (AeroPreviewCamera != null)
            AeroPreviewCamera.Render();

        meshRenderer.materials = originalMaterial;

        float exposure = visiblePixels / totalPixels;

        if (float.IsNaN(exposure))
            return 1.0f;
        else
            return exposure;
    }

    float countActiveAirfoilPixels()
    {
        AeroCamera.Render();

        RenderTexture aeroTexture = AeroCamera.targetTexture;
        Color activeAirfoilColor = followAero.ActiveAirfoilMaterial.color;

        // This strange incantation is how you get the pixels out of a RenderTexture.
        // Adapted from https://docs.unity3d.com/ScriptReference/RenderTexture-active.html
        RenderTexture originalActiveTexture = RenderTexture.active;
        RenderTexture.active = aeroTexture;
        Rect textureRect = new Rect(0, 0, intermediateTexture.width, intermediateTexture.height);
        intermediateTexture.ReadPixels(textureRect, 0, 0);
        RenderTexture.active = originalActiveTexture;

        int pixels = 0;

        foreach (Color pixel in intermediateTexture.GetPixels())
        {
            // Debug.Log(activeAirfoilColor);
            // Debug.Log("This pixel's color is: --------------------------------" + pixel.ToString());

            if (pixel == activeAirfoilColor)
                pixels++;
        }

        return (float)pixels;
    }

    void calculateCoefficients(
        float angleOfAttackDeg, out float coefficientOfLift, out float coefficientOfDrag)
    {
        bool stalled = Mathf.Abs(angleOfAttackDeg) > 12.0f;

        if (stalled) 
            coefficientOfLift = 1.2f * Mathf.Sin(2 * Mathf.Deg2Rad * angleOfAttackDeg);
        else
            coefficientOfLift = angleOfAttackDeg / 10.0f;

        if (DisplayStalls && meshRenderer != null)
        {
            if (stalled)
                meshRenderer.material = StalledMaterial;
            else
                meshRenderer.material = NormalMaterial;
        }

        bool turbulent = Mathf.Abs(angleOfAttackDeg) > 10.0f;

        if (turbulent)
            coefficientOfDrag = Mathf.Abs(2.0f * Mathf.Sin(Mathf.Deg2Rad * angleOfAttackDeg));
        else
            coefficientOfDrag = 0.0f;
    }
}
