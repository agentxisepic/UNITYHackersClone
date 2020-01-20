using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtensionNode : Node
{
    protected float _degreesPerNode;

    //LAYER STUFF
    LayerController _layerController;

    private void Awake()
    {
        _numberOfConnections = 30;//TODO make level manager control this, once it exists
        _connectionPoints = new Connection[_numberOfConnections];
        _degreesPerNode = (Mathf.PI / 4.0f) * Mathf.Rad2Deg;
    }

    // Start is called before the first frame update
    private void Start()
    {
        _GameManager = FindObjectOfType<GameManager>();
        _layerController = GetComponent<LayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))//REMOVE Once UI is done
        {
            CreateNode();
        }
    }
    

    public void CreateNode()
    {
        if (BHasOpenConnection())
        {
            if (_layerController.BAllLayersFull()) { return; }
            
            GameObject newNode = _GameManager._NodeNetworkManager.NewNode();

            bool successfullyCompleted = InitializeNewNode(newNode);
            if (!successfullyCompleted) { return; }

            RequestConnection(newNode.GetComponent<Node>());
            _layerController.AddObject(newNode.gameObject);
        }
    }

    private bool InitializeNewNode(GameObject node)//TODO make this more performant
    {
        Node nodeComponent = node.GetComponent<Node>();
        if (nodeComponent == null) { return false; }

        float nextNodeRotation = NextNodeRotation(_layerController.SelectedLayer);
        (float x, float z) newNodePos = NodeMath.FindPointFromAngle(nextNodeRotation, _GameManager.NodeDistance);
        float YPosition = GetYPosition(_layerController.SelectedLayer);

        node.transform.position = new Vector3(newNodePos.x, YPosition, newNodePos.z);


        nodeComponent.UpdateRotation(gameObject);
        while (BNodeAlreadyAtRotation((float)nodeComponent.angleFromExtensionNode, _layerController.SelectedLayer))
        {
            nextNodeRotation += _degreesPerNode;
            newNodePos = NodeMath.FindPointFromAngle(nextNodeRotation, _GameManager.NodeDistance);
            node.transform.position = new Vector3(newNodePos.x, YPosition, newNodePos.z);
            nodeComponent.UpdateRotation(gameObject);
        }
        return true;
    }

    private float GetYPosition(int layer)//TODO Think about other ways of doing this?
    {
        if (layer == 0)
        {
            return 0;
        }
        else if (layer == 1)
        {
            return 2;
        }
        else if (layer == 2)
        {
            return -2; 
        }
        else if (layer == 3)
        {
            return 4;
        }
        else if (layer == 4)
        {
            return -4;
        }
        else
        {
            return -11111111;
        }
    }

    private float NextNodeRotation(int layer)//TODO remove connection refrences to angle
    {
        GameObject[] currentLayer = _layerController.GetLayer(layer);

        float nextRotation;
        float freeRotation = 0;
        for (int i = 0; i < currentLayer.Length; i++)
        {
            nextRotation = (_degreesPerNode * i);
            if (currentLayer[i] != null && !BNodeAlreadyAtRotation(nextRotation, layer))
            {
                return nextRotation;
            }
        }
        return freeRotation;
    }

    ///THE BLACK BOX WE DO NOT TOUCH, The myths say it works, but no one has confirmed it
    private bool BNodeAlreadyAtRotation(float rotation, int layer) 
    {
        //Check each Node for existing at rotation
        GameObject[] LAYER = _layerController.GetLayer(layer);
        for (int i = 0; i < LAYER.Length; i++)
        {
            Node CurrentNode = LAYER[i]?.GetComponent<Node>();
            if (LAYER[i] == null || CurrentNode == null)//if we don't have a connection here, just continue
            {
                continue;
            }

            //Get the rotation of the Node at this connection
            float rotationA = LAYER[i].GetComponent<Node>().angleFromExtensionNode;

            //Check for a common Node, this will mean they have a swath just for them
            Node commonNode = _GameManager._NodeNetworkManager.FindCommonNode(this, CurrentNode);

            //Rotation of the Common Node
            float rotationB;
            rotation = Mathf.RoundToInt(rotation);
            rotationA = Mathf.RoundToInt(rotationA);
            
                
                //If rotation lines up with the current Node we cannot go here
                if (rotation == rotationA)
                {
                    return true;
                }




                //Check to see if the rotation is in between two connected nodes
                else if (commonNode != null)
                {
                    //Get common node rotation
                    rotationB = Mathf.RoundToInt(commonNode.angleFromExtensionNode);


                    //Check for rotations in between already connected Nodes
                    if (rotationA < 0 && rotationB < 0)
                    {
                        if (rotationA > rotationB)
                        {
                            if (rotation < rotationA && rotation > rotationB)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (rotation > rotationA && rotation < rotationB)
                            {
                                return true;
                            }
                        }
                    }
                    else if (rotationA > 0 && rotationB > 0)
                    {
                        if (rotationA > rotationB)
                        {
                            if (rotation > rotationB && rotation < rotationA)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (rotation < rotationB && rotation > rotationA)
                            {
                                return true;
                            }
                        }
                    }
                    else if (rotationA > 0 && rotationB < 0)
                    {
                        if (rotation > rotationA && rotation > rotationB)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (rotation > rotationB && rotation < rotationA)
                        {
                            return true;
                        }
                    }


                    if (rotationB > rotationA)
                    {
                        float temp = (float)rotationB;
                        rotationB = rotationA;
                        rotationA = temp;
                    }

                    if (rotation > 0)
                    {
                        if (rotationA > 90)
                        {
                            if (rotation >= rotationA && rotation >= rotationB)
                            {
                                return true;
                            }
                        }
                    }
                    else if (rotation < 0)
                    {
                        if (rotationB < -90)
                        {
                            if (rotation <= rotationA && rotation <= rotationB)
                            {
                                return true;
                            }
                        }
                    }
                }
        }
        return false;
    }

    private void UpdateNodeRotations()//TODO CREATE
    {
        return;
    }

    public override void RemoveConnection(Connection connection)
    {
        //Added loop into to purge the LayerController of Node aswell
        GameObject other = connection.GetOtherNode(this).gameObject;
        for (int i = 0; i < _layerController.GetNumberOfLayers(); i++)
        {
            GameObject[] currentLayer = _layerController.GetLayer(i);
            for (int j = 0; j < currentLayer.Length; j++)
            {
                if (currentLayer[j] == other)
                {
                    currentLayer[j] = null;
                }
            }
            
        }

        base.RemoveConnection(connection);
    }
}
