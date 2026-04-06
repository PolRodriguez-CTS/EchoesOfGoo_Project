using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Load(SceneDatabase.Slots.Mazmorra_1, SceneDatabase.Scenes.Mazmorra_1)
            .Perform();
    }
}