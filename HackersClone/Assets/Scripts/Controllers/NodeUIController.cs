using TMPro;
using UnityEngine;

public class NodeUIController : MonoBehaviour
{
    private GameObject _player;
    private TextMeshProUGUI _nodeStats;
    private NodeNetworkManager _nodeNetworkManager;

    void Start()
    {
        _player = FindObjectOfType<CameraController>()?.gameObject;
        _nodeStats = GetComponentsInChildren<TextMeshProUGUI>()[1];
        _nodeNetworkManager = transform.parent.gameObject.GetComponent<NodeNetworkManager>();
    }

    void Update()
    {
        transform.LookAt(_player.transform);
        transform.Rotate(new Vector3(0, 180, 0));
    }

    public void updateText(string name, int level)
    {
        _nodeStats.text = name + "\n" + "Level: " + level;
    }

    //Button Methods
    public void Destroy()
    {
        GameObject parent = transform.parent.gameObject;
        _nodeNetworkManager.ReturnUI(parent);
    }
}
