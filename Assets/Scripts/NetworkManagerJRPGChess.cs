using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class NetworkManagerJRPGChess : NetworkManager
{
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        if (SceneManager.GetActiveScene().name != "GameScene")
            ServerChangeScene("GameScene");
    }
}
