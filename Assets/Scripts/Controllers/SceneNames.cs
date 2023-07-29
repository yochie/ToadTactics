using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Any changes to scene names only need to be updated here
//Enum changes can be automated via IDE renaming
public class SceneNames
{
    private static SceneNames singleton = null;
    public static SceneNames Singleton
    {
        get 
        {
            if (SceneNames.singleton == null)
                SceneNames.singleton = new();
            return SceneNames.singleton;
        }
    }

    private Dictionary<SceneName, string> sceneNames;

    public SceneNames()
    {
        this.sceneNames = new();
        this.sceneNames.Add(SceneName.Draft, "Draft");
        this.sceneNames.Add(SceneName.EquipmentDraft, "EquipmentDraft");
        this.sceneNames.Add(SceneName.Lobby, "Lobby");
        this.sceneNames.Add(SceneName.MainGame, "MainGame");
        this.sceneNames.Add(SceneName.Menu, "Menu");
    }

    public string GetSceneName(SceneName scene)
    {
        return this.sceneNames[scene];
    }
}
