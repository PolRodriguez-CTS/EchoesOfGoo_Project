using UnityEngine;

public class FabricaManager : MonoBehaviour
{
    #region Singleton
    public static FabricaManager Instance;

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
    public void ToDungeon1()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.Fabrica, SceneDatabase.Scenes.Fabrica)
            .WithOverlay()
            .Load(SceneDatabase.Slots.Dungeon_1, SceneDatabase.Scenes.Dungeon_1)
            .Perform();
    }
}
