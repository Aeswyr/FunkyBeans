using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : Singleton<MenuHandler>
{

    [SerializeField] private TMP_InputField input;
    public void OnHostPressed() {
        LobbyHandler.Instance.shouldHost = true;
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnJoinPressed() {
        LobbyHandler.Instance.shouldHost = false;
        LobbyHandler.Instance.addresss = input.text;
        SceneManager.LoadScene("LobbyScene");
    }
}
