using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelDataSO : ScriptableObject
{
    public string levelName;
    public List<WaveDataSO> waves = new();
    public AudioClip music;
    public Material skybox;
}