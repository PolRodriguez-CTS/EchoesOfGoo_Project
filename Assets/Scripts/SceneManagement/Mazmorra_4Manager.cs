using UnityEngine;

public class Mazmorra_4Manager : MonoBehaviour
{
    #region Singleton
    public static Mazmorra_4Manager Instance;

    void Awake()
    {
        
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion
    public void ReturnMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.Dungeon_4, SceneDatabase.Scenes.Dungeon_4)
            .WithOverlay()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Perform();
    }

    public void GoToCredits()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.Dungeon_4, SceneDatabase.Scenes.Dungeon_4)
            .WithOverlay()
            .Load(SceneDatabase.Slots.Credits, SceneDatabase.Scenes.Credits)
            .Perform();
    }
}
