using UnityEngine;
using Mirror;
public class NetworkSingleton<T> : NetworkBehaviour where T : Component {
    private static T instance = null;

    public static T Instance {
        get {
            if (instance == null)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }

}