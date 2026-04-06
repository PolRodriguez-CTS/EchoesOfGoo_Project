using UnityEngine;

public class CoreManager : MonoBehaviour
{
    void Start()
    {
        //Core Setup for the game
        //Load everything like AudioManagers
        
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }
}