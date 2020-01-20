using System.Collections;
using UnityEngine;

public class Node : MonoBehaviour, IImplementUI//TODO determine if this is the best way to do UI
{
    protected GameManager _GameManager;

    protected int _numberOfConnections;

    public Connection[] _connectionPoints { get; protected set; }

    public float angleFromExtensionNode { get; protected set; }


    //TEST
    [SerializeField] private float _angleFromExtensionNode;

    private void Awake()
    {
        Name = "Basic Node";
        Level = 1;
        _numberOfConnections = 3;
        _connectionPoints = new Connection[_numberOfConnections];
    }

    private void Start()
    {
        _GameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        UpdateRotation(_GameManager._NodeNetworkManager._mainNode);
        _angleFromExtensionNode = angleFromExtensionNode;
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(this.name + "Rotation: " + angleFromExtensionNode);
        }
    }

    public void RequestConnection(Node nodeToConnectWith)
    {
        _GameManager._NodeNetworkManager.InitiateConnection(this, nodeToConnectWith);
    }

    public void CreateConnection(Connection connection)
    {
        if (BAlreadyConnected(connection.GetOtherNode(this))) { return; }

        for (int i = 0; i < _numberOfConnections; i++)
        {
            if (_connectionPoints[i] == null)
            {
                _connectionPoints[i] = connection;
                return;
            }
        }
    }

    public virtual void RemoveConnection(Connection connection) //Remove connection object, should get rid of connected Node references
    {
        for (int i = 0; i < _numberOfConnections; i++)
        {
            if (_connectionPoints[i] == connection)
            {
                _connectionPoints[i] = null;
                return;
            }
        }
    }

    public bool BHasOpenConnection()
    {
        for (int i = 0; i < _numberOfConnections; i++)
        {
            if (_connectionPoints[i] == null)
            {
                return true;
            }
        }

        return false;

    }

    public bool BAlreadyConnected(Node node)
    {
        for (int i = 0; i < _numberOfConnections; i++)
        {
            if (_connectionPoints[i]?.GetOtherNode(this) == node)
            {
                return true;
            }
        }
        return false;
    }

    //NEW METHOD FOR 3D EQUILATERAL STUFF
    public int GetNumberOfConnections()
    {
        int numberOfConnections = 0;
        for (int i = 0; i < _connectionPoints.Length; i++)
        {
            if (_connectionPoints[i] != null)
            {
                numberOfConnections++;
            }
        }
        return numberOfConnections;
    }


    public void PrintConnections()
    {
        string Connections = string.Empty;
        for (int i = 0; i < _connectionPoints.Length; i++)
        {
            Connections += "Slot " + i + ": " + _connectionPoints[i] + ", ";
        }
        Debug.Log(Connections);
    }//TODO remove when done, NO LONGER will show Nodes, instead the connectionObjects

    public void ResetVariables() //TODO make a constructor to do this(?)
    {
        Awake();
    }


    public void UpdateRotation(GameObject extension)//TODO better name
    {
        if (extension.GetComponent<ExtensionNode>() == null) { return; }
        angleFromExtensionNode = NodeMath.AngleBetweenNodes(extension.transform.position, gameObject.transform.position);
        if (angleFromExtensionNode > 180)
        {
            angleFromExtensionNode -= 360;
        }
    }

    /** IImplementUI Members **/

    public string Name { get; set; }
    public int Level { get; set; }

    public void RequestUI()
    {
        _GameManager._NodeNetworkManager.SendUI(transform, Name, Level); //TODO tell GameManager what you want to do and have it tell the NetworkManager
    }

    public void UpdateUI()
    {
        GetComponent<NodeUIController>()?.updateText(Name, Level);
    }

    public void Upgrade()
    {
        return;
    }//TODO implement
}
