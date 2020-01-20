using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float NodeDistance { get; private set; }
    public NodeNetworkManager _NodeNetworkManager { get; private set; } //TODO Reference GameManager Everywhere and Get NodeNetworkManager from here

    private void Awake()
    {
        NodeDistance = 10;
    }

    void Start()
    {
        _NodeNetworkManager = FindObjectOfType<NodeNetworkManager>();
    }
}
