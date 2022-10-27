using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MenuHandler : Singleton<MenuHandler>
{

    [SerializeField] private TMP_InputField inputIP;
    [SerializeField] private TMP_InputField inputName;
    //[SerializeField] private NetworkManagerJRPGChess network;
    [SerializeField] private LobbyManager lobby;
    public void OnHostPressed() {
        //network.StartHost();
        lobby.Host();
    }

    public void OnJoinPressed() 
    {
        /*
        if(inputIP.text.Length == 0)
        {
            network.networkAddress = "localhost";
        }
        else
        {
            network.networkAddress = inputIP.text;
        }

        //SceneManager.LoadScene("GameScene");

        network.StartClient();
        */
    }
}
