using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyHandler : Singleton<LobbyHandler>
{
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }
    public bool shouldHost;
    public string addresss;
}
