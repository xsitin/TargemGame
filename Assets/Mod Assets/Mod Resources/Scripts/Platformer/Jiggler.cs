using UnityEngine;

public class Jiggler : MonoBehaviour
{
    [Range(0, 1)] public float power = .1f;

    [Header("Position Jiggler")] public bool jigPosition = true;

    public Vector3 positionJigAmount;

    [Range(0, 120)] public float positionFrequency = 10;

    [Header("Rotation Jiggler")] public bool jigRotation = true;

    public Vector3 rotationJigAmount;

    [Range(0, 120)] public float rotationFrequency = 10;

    [Header("Scale Jiggler")] public bool jigScale = true;

    public Vector3 scaleJigAmount = new(.1f, -.1f, .1f);

    [Range(0, 120)] public float scaleFrequency = 10;

    private Vector3 basePosition;
    private Quaternion baseRotation;
    private Vector3 baseScale;
    private float positionTime;
    private float rotationTime;
    private float scaleTime;

    private void Start()
    {
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;
        baseScale = transform.localScale;
    }

    // Update is called once per frame
    private void Update()
    {
        var dt = Time.deltaTime;
        positionTime += dt * positionFrequency;
        rotationTime += dt * rotationFrequency;
        scaleTime += dt * scaleFrequency;

        if (jigPosition)
            transform.localPosition = basePosition + positionJigAmount * Mathf.Sin(positionTime) * power;

        if (jigRotation)
            transform.localRotation =
                baseRotation * Quaternion.Euler(rotationJigAmount * Mathf.Sin(positionTime) * power);

        if (jigScale)
            transform.localScale = baseScale + scaleJigAmount * Mathf.Sin(scaleTime) * power;
    }
}