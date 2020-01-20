using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerController : MonoBehaviour
{
    private const int NUMBEROFLAYERS = 5;
    private const int OBJECTSPERLEVEL = 6;
    private GameObject[][] _Layers;
    public int SelectedLayer { get; private set; }

    private void Awake()
    {
        InitLayers();
        SelectedLayer = 0;
    }

    private void InitLayers()
    {
        _Layers = new GameObject[NUMBEROFLAYERS][];
        for (int i = 0; i < NUMBEROFLAYERS; i++)
        {
            _Layers[i] = new GameObject[OBJECTSPERLEVEL];
            for (int j = 0; j < OBJECTSPERLEVEL; j++)
            {
                _Layers[i][j] = null;
            }
        }
    }

    private void AddToLayer(GameObject Object, int layer)
    {
        GameObject[] Layer = _Layers[layer];
        for (int i = 0; i < OBJECTSPERLEVEL; i++)
        {
            if (Layer[i] == null)
            {
                Layer[i] = Object;
                return;
            }
        }
    }

    private void RemoveFromLayer(GameObject Object, int layer)
    {
        GameObject[] Layer = _Layers[layer];
        for (int i = 0; i < OBJECTSPERLEVEL; i++)
        {
            if (Layer[i] == Object)
            {
                Layer[i] = null;
                return;
            }
        }
    }

    private int GetObjectLayer(GameObject Object)
    {
        for (int i = 0; i < NUMBEROFLAYERS; i++)
        {
            GameObject[] Layer = _Layers[i];
            for (int j = 0; j < OBJECTSPERLEVEL; j++)
            {
                if (Layer[j] == Object)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private void ChangeLayer(GameObject Object, int newLayer)
    {
        int oldLayer = GetObjectLayer(Object);
        RemoveFromLayer(Object, oldLayer);
        AddToLayer(Object, newLayer);
    }

    private bool BLayerFull(int layer)
    {
        GameObject[] Layer = _Layers[layer];
        for (int i = 0; i < OBJECTSPERLEVEL; i++)
        {
            if (Layer[i] == null)
            {
                return false;
            }
        }
        return true;
    }

    private int GetEmptyLayer()
    {
        for (int i = 0; i < NUMBEROFLAYERS; i++)
        {
            if (!BLayerFull(i))
            {
                return i;
            }
        }
        return -1;
    }


    //PUBLIC ACCESSORS
    public bool BAllLayersFull()
    {
        if (BLayerFull(SelectedLayer))
        {
            SelectedLayer = GetEmptyLayer();
        }

        if (SelectedLayer == -1)
        {
            return true;
        }
        return false;
    }

    public void AddObject(GameObject Object)
    {
        AddToLayer(Object, SelectedLayer);
    }

    public GameObject[] GetLayer(int layer)
    {
        return _Layers[layer];
    }
    
    public int GetNumberOfLayers()
    {
        return NUMBEROFLAYERS;
    }

}
