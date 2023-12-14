using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Manager/Game Manager")]
public class GameManager : ScriptableObject
{
    public void BossKilled(){
        onBossKilled?.Invoke();
    }

    public event Action onBossKilled;
}
