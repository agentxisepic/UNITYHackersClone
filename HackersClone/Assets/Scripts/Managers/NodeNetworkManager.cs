using System.Collections.Generic;
using System.Collections;
using UnityEngine;

enum ESideOfTriangle
{
    none = 0,
    right = 1,
    left = -1
}

public class NodeNetworkManager : MonoBehaviour
{
    [SerializeField] private GameObject _nodeUI;
    [SerializeField] public GameObject _mainNode;//TODO Make private after testing
    [SerializeField] private GameObject[] _nodePrefabs;
    [SerializeField] private GameObject[] _connectionPrefabs;

    private GameManager _gameManager { get; set; }

    private NodeUIController _nodeUIController;

    private ObjectPooler _nodePool;
    private ObjectPooler _connectionPool;

    private void Awake()
    {

    }

    private void Start()
    {
        _mainNode = Instantiate(_mainNode, Vector3.zero, Quaternion.identity);
        _nodeUI = Instantiate(_nodeUI, transform);

        _nodeUIController = _nodeUI.GetComponentInChildren<NodeUIController>();
        _gameManager = FindObjectOfType<GameManager>();
        
        _nodePool = CreateObjectPool("NodePool", _nodePrefabs);
        _connectionPool = CreateObjectPool("ConnectionPool", _connectionPrefabs);
    }


    private void Update()
    {
    }

    //Node Connection control functions
    public bool InitiateConnection(Node caller, Node connector)//TODO Break into multiple functions, it does not simply InitiateAConnection, Also there are similarly named functions, good design? Idk
    {
        if (caller.BHasOpenConnection() && connector.BHasOpenConnection() && !caller.BAlreadyConnected(connector))
        {
            Connection newConnection;
            Node commonNode = FindCommonNode(caller, connector);

            ESideOfTriangle callerSideOfTriangle = BNodeIsPartOfTriangle(caller);
            ESideOfTriangle connectorSideOfTriangle = BNodeIsPartOfTriangle(connector);
            //0 not part of a triangle, 1 right of triangle, -1 left of triangle

            ///3D Equilateral Stuffs
            int numberOfCallerConnections = caller.GetNumberOfConnections();
            if (numberOfCallerConnections == 2)
            {

            }
            ////

            List<Node> HandledNodes = new List<Node>();
            HandledNodes.Add(caller);
            HandledNodes.Add(connector);
            HandledNodes.Add(commonNode);

            InitializeConnection(caller, connector, out newConnection);
            if (commonNode != null)
            {
                if (connectorSideOfTriangle == ESideOfTriangle.none || connectorSideOfTriangle == ESideOfTriangle.right)
                {
                    StartCoroutine(MoveNode(caller, connector, commonNode, ESideOfTriangle.right, HandledNodes));
                }
                else if (connectorSideOfTriangle == ESideOfTriangle.left)
                {
                    StartCoroutine(MoveNode(caller, connector, commonNode, ESideOfTriangle.left, HandledNodes));
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator MoveNode(Node toMove, Node connecting, Node common, ESideOfTriangle sideToPlace, List<Node> handledNodes)
    {
        //Get Location to Move Node to
        Vector3 newLocation = Vector3.zero;
        if (sideToPlace == ESideOfTriangle.left)
        {
            newLocation = NewNodeLocation(connecting, common, 0, true);
        }
        else if (sideToPlace == ESideOfTriangle.right)
        {
            newLocation = NewNodeLocation(connecting, common, 0, false);
        }

        newLocation.y = toMove.transform.position.y;

        //Move Node to that location over time
        Vector3 directionToMove = (newLocation - toMove.transform.position) / 60.0f;
        directionToMove.y = 0.0f;
        bool bCompleted = false;
        while (toMove.transform.position != newLocation)
        {
            if (!toMove.gameObject.activeSelf) { break; } 
            directionToMove = (newLocation - toMove.transform.position) / 60.0f;
            //Decide wether distance is small enough to visually stutter
            Vector3 distBetween = newLocation - toMove.transform.position;
            float smallJump = distBetween.x + distBetween.y + distBetween.z;
            smallJump = smallJump < 0 ? smallJump * -1 : smallJump;
            if (smallJump < 0.01f)
            {
                toMove.transform.position = newLocation;
                bCompleted = true;
            }
            else
            {
                toMove.transform.Translate(directionToMove);
            }
            yield return new WaitForSeconds(1.0f / 60.0f);
        }

        //Move any other nodes it was connected to, to maintain equilateral triangles
        if (bCompleted)
        {
            Connection[] toMoveConnections = toMove._connectionPoints;//TODO fix variable names so that privates are the only ones with underscores. PROPERTIES
            for (int i = 0; i < toMoveConnections.Length; i++)
            {
                if (toMoveConnections[i] == null) { continue; }
                Node otherNode = toMoveConnections[i].GetOtherNode(toMove);
                if (!BAlreadyInList(handledNodes, otherNode))
                {
                    handledNodes.Add(otherNode);
                    yield return StartCoroutine(MoveNode(otherNode, toMove, common, sideToPlace, handledNodes));
                }
            }
        }
        yield return null;
    }

    private bool BAlreadyInList(List<Node> nodes, Node testFor)
    {
        foreach (Node current in nodes)
        {
            if (current == testFor) { return true; }
        }
        return false;
    }


    private Vector3 NewNodeLocation(Node a, Node b, int SideOfTriangle, bool bSideOfTriangle)//TODO implement sideoftriangle enum
    {
        (float x, float z) newNodePoint = NodeMath.FindThirdPointOfTriangle (a.transform.position.x, a.transform.position.z,
                                                                             b.transform.position.x, b.transform.position.z, 
                                                                             bSideOfTriangle);

        Vector3 newNodePosition = new Vector3(newNodePoint.x, 0, newNodePoint.z);
        return newNodePosition;
    }

   

    private void InitializeConnection (Node caller, Node connector, out Connection newConnection)
    {
        newConnection = NewConnection(caller, connector);
        caller.CreateConnection(newConnection);
        connector.CreateConnection(newConnection);
    }

    public Node FindCommonNode(Node toMove, Node toConnectWith) //Finds the common node between two connections
    {
        for (int i = 0; i < toMove._connectionPoints.Length; i++)
        {
            Connection sameConnection = toMove._connectionPoints[i];
            Node otherNode = sameConnection?.GetOtherNode(toMove);
            for (int j = 0; j < otherNode?._connectionPoints.Length; j++)
            {
                if (otherNode._connectionPoints[j]?.GetOtherNode(otherNode) == toConnectWith)
                {
                    return otherNode._connectionPoints[j]?.GetOtherNode(toConnectWith);
                }
            }
            
        }
        return null;
    }

    private ESideOfTriangle BNodeIsPartOfTriangle(Node toCheck)
    {
        for (int i = 0; i < toCheck._connectionPoints.Length; i++)
        {
            Node otherNode = toCheck._connectionPoints[i]?.GetOtherNode(toCheck);
            Node commonNode = FindCommonNode(toCheck, otherNode);
            if (commonNode != null && otherNode.gameObject.GetComponent<ExtensionNode>() == null)
            {
                float toCheckRotation = toCheck.angleFromExtensionNode;
                float otherNodeRotation = otherNode.angleFromExtensionNode;
                if (toCheckRotation < -90 && otherNodeRotation > 90)
                {
                    otherNodeRotation *= -1;
                }
                if (toCheckRotation > otherNodeRotation)
                {
                    return ESideOfTriangle.right;
                }
                else if (toCheck.angleFromExtensionNode < otherNode.angleFromExtensionNode)
                {
                    return ESideOfTriangle.left;
                }
            }
        }
        return ESideOfTriangle.none;
    }

    
    private Connection NewConnection(Node a, Node b)
    {
        Connection newConnection = _connectionPool.GetObject().GetComponent<Connection>();
        newConnection.Initialize(a, b);
        return newConnection;
    }

    private void DestroyConnection(Connection connection)
    {
        connection.ResetConnection();
        _connectionPool.ReturnObject(connection.gameObject);
    }

    private void ReturnConnectionObjects()
    {
        Connection[] connections = FindObjectsOfType<Connection>();

        for (int i = 0; i < connections.Length; i++)
        {
            _connectionPool.ReturnObject(connections[i].gameObject);
        }
    }


    //Node UI functions TODO put in a UI controller
    public void SendUI(Transform node, string name, int level)//TODO fix spawn position of UI
    {
        if (_nodeUI.transform.parent == node) { return; }

        Vector3 offset = new Vector3(1.25f, 0.137f, -0.572f);
        Vector3 UIPosition = new Vector3(node.position.x + offset.x, node.position.y + offset.y, node.position.z + offset.z);
        _nodeUI.transform.position = UIPosition;
        _nodeUI.transform.SetParent(node);
        _nodeUIController.updateText(name, level);
        _nodeUI.SetActive(true);
    }

    public void ReturnUI(GameObject parent)
    {
        _nodeUI.transform.SetParent(this.transform);
        _nodeUI.SetActive(false);
        if (parent == null) { return; }
        DestroyNode(parent);
    }


    //Node ObjectPooling Functions, do a little more than communicate with the pool and are accessed outside the class TODO ObjectPoolController?
    public GameObject NewNode()
    {
        GameObject newNode = _nodePool.GetObject();
        return newNode;
    }//TODO make these funcitons completely private

    public void DestroyNode(GameObject node)
    {
        Node nodeComponent = node.GetComponent<Node>();
        DestroyNodesConnections(nodeComponent);
        nodeComponent.ResetVariables();
        _nodePool.ReturnObject(node);
    }

    private void DestroyNodesConnections(Node node)
    {
        Connection[] nodeConnections = node._connectionPoints;
        for (int i = 0; i < nodeConnections.Length; i++)
        {
            if (nodeConnections[i] != null)
            {
                Connection current = nodeConnections[i];
                current.ConnectingNodes.a?.RemoveConnection(nodeConnections[i]);
                current.ConnectingNodes.b?.RemoveConnection(nodeConnections[i]);
                DestroyConnection(current);
            }
        }
    }

    private ObjectPooler CreateObjectPool(string nameOfPool, GameObject[] prefabs)
    {
        ObjectPooler newObjectPool = new GameObject(nameOfPool).AddComponent<ObjectPooler>();
        newObjectPool.transform.SetParent(this.transform);
        newObjectPool.SetPrefabList(prefabs);
        return newObjectPool;
    }
}
