using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    public (Node a, Node b) ConnectingNodes { get; private set; } 
    public float angleFromExtensionNode { get; private set; }

    public void Initialize(Node node1, Node node2)
    {
        //Set Nodes that are being connected
        ConnectingNodes = (node1, node2);
        UpdateTransform();
    }

    private void Update()
    {
        UpdateTransform();
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(angleFromExtensionNode);
        }
    }

    public Node GetOtherNode(Node caller)
    {
        if (caller == ConnectingNodes.a)
        {
            return ConnectingNodes.b;
        }
        else
        {
            return ConnectingNodes.a;
        }
    }

    public void UpdateTransform()
    {
        ResetTransform();
        //Set scale
        float length = NodeMath.DistanceFormula(ConnectingNodes.a.transform.position.x, ConnectingNodes.a.transform.position.z, ConnectingNodes.b.transform.position.x, ConnectingNodes.b.transform.position.z);
        transform.localScale += new Vector3(0, length, 0);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2.0f, transform.localScale.z);

        //Set position
        (float x, float y, float z) spawnPos = NodeMath.MidpointFormula(ConnectingNodes.a.transform.position.x, ConnectingNodes.b.transform.position.x, ConnectingNodes.a.transform.position.y, ConnectingNodes.b.transform.position.y, ConnectingNodes.a.transform.position.z, ConnectingNodes.b.transform.position.z);
        transform.position = new Vector3(spawnPos.x, spawnPos.y, spawnPos.z);

        //Set rotation
        transform.LookAt(ConnectingNodes.b.gameObject.transform, Vector3.up);
        transform.Rotate(0, 90, 90);

        
    }

    private void ResetTransform()
    {
        transform.position = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        transform.rotation = Quaternion.identity;
    }

    public void ResetConnection()
    {
        ConnectingNodes = (null, null);
    }

}
