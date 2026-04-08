using UnityEngine;

public class Mazmorra_3Manager : MonoBehaviour
{
    #region Singleton
    public static Mazmorra_3Manager Instance;

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
            .Unload(SceneDatabase.Slots.Dungeon_3, SceneDatabase.Scenes.Dungeon_3)
            .WithOverlay()
            .Load(SceneDatabase.Slots.Dungeon_4, SceneDatabase.Scenes.Dungeon_4)
            .Perform();
    }
}
