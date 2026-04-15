using MyBox;
using UnityEngine;

[CreateAssetMenu(fileName = "New ArenaConfig", menuName = "ArenaConfig")]
public class ArenaConfig : ScriptableObject
{
    public string arenaID;
    public SceneReference sceneRef;
}