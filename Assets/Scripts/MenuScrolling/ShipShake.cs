using UnityEngine;

public class ShipShake : MonoBehaviour
{
    void Update()
    {
        float floatingY = Mathf.Sin(Time.time * 2f) * 0.5f; 
        transform.localPosition = new Vector3(transform.localPosition.x, floatingY, transform.localPosition.z);
    }
}
