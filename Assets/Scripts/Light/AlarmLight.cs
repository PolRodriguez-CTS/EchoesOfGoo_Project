using UnityEngine;

[RequireComponent(typeof(Light))]
public class AlarmLight : MonoBehaviour
{
    [Header("Range")]
    public float tempMin = 12f;
    public float tempMax = 12310f;

    [Header("Alarm")]
    public float speed = 2.0f;

    private Light directionalLight;

    void Start()
    {
        directionalLight = GetComponent<Light>();
        directionalLight.useColorTemperature = true;
    }

    void Update()
    {
        float lerpColor = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;

        directionalLight.colorTemperature = Mathf.Lerp(tempMin, tempMax, lerpColor);
    }
}
