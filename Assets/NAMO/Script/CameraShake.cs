using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Transform camTransform;
    private float shakeTimer = 0f;
    private float shakeIntensity = 0f;
    private Vector3 originalPos;

    private void Awake()
    {
        Instance = this;
        camTransform = transform;
    }

    private void OnEnable()
    {
        originalPos = camTransform.localPosition;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            camTransform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeIntensity;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            camTransform.localPosition = originalPos;
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        shakeIntensity = intensity;
        shakeTimer = time;
    }
}