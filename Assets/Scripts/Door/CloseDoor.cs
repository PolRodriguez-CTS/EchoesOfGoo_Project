using UnityEngine;

public class CloseDoor : MonoBehaviour
{
    [SerializeField] private GameObject door;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Door doorScript = door.GetComponent<Door>();
            doorScript.CloseAnimation();
        }
    }
}
