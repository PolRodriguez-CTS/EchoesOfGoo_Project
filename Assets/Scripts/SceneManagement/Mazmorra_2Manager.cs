using UnityEngine;

public class Mazmorra_2Manager : MonoBehaviour
{
    #region Singleton
    public static Mazmorra_2Manager Instance;

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
    public void NextLevel()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.Dungeon_2, SceneDatabase.Scenes.Dungeon_2)
            .WithOverlay()
            .Load(SceneDatabase.Slots.Dungeon_3, SceneDatabase.Scenes.Dungeon_3)
            .Perform();
    }
}
