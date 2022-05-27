using Mirror;

public class CombatID : NetworkBehaviour {
    [SyncVar] private long m_cid;
    public long CID => m_cid;
    void Start() {
        if (isServer)
            m_cid = (long)(UnityEngine.Random.value * long.MaxValue);
    }
}