using UnityEngine;

public class ToMainMenu : MonoBehaviour
{
    public void FinishDemo()
    {
        Mazmorra_4Manager.Instance.GoToCredits();
    } 
}