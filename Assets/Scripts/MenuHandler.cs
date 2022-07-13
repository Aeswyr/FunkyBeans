using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MenuHandler : Singleton<MenuHandler>
{

    [SerializeField] private TMP_InputField inputIP;
    [SerializeField] private TMP_InputField inputName;
    [SerializeField] private NetworkManagerJRPGChess network;
    public void OnHostPressed() {
        //SceneManager.LoadScene("GameScene");
        network.StartHost();
    }

    public void OnJoinPressed() 
    {
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
    }
}
