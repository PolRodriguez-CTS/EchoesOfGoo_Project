using UnityEngine;

public class Mazmorra_1Manager : MonoBehaviour
{
    #region Singleton
    public static Mazmorra_1Manager Instance;

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
            .Unload(SceneDatabase.Slots.Mazmorra_1, SceneDatabase.Scenes.Mazmorra_1)
            .WithOverlay()
            .Load(SceneDatabase.Slots.Mazmorra_2, SceneDatabase.Scenes.Mazmorra_2)
            .Perform();
    }
}