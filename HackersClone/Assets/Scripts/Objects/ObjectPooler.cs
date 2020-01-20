using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour //TODO determine which GameObject type you want from the _prefabs you want
{
    private Queue<GameObject> _objectPool;

    private int _cloneNumber;
    
    private GameObject[] _prefabs;

    private void Awake()
    {
        _cloneNumber = 0;
    }

    private void Start()
    {
        _objectPool = new Queue<GameObject>();
    }

    public GameObject GetObject()
    {
        if (_objectPool.Count == 0)
        {
            GameObject newGameObject = AddObject();
            newGameObject.SetActive(true);
            return newGameObject;
        }

        GameObject objectToUse = _objectPool.Dequeue();
        objectToUse.SetActive(true);
        return objectToUse;
    }

    private GameObject AddObject()
    {
        GameObject newObject = Instantiate(_prefabs[0]);
        newObject.transform.SetParent(transform);
        newObject.name = name + ": " + _cloneNumber;
        _cloneNumber++;
        return newObject;
    }

    public void ReturnObject(GameObject returnedObject)
    {
        ResetObjectTransform(returnedObject);
        returnedObject.SetActive(false);
        _objectPool.Enqueue(returnedObject);
    }

    private void ResetObjectTransform(GameObject objectToReset)
    {
        objectToReset.transform.position = new Vector3(0, 0, 0);
        objectToReset.transform.localScale = _prefabs[0].transform.localScale;
        objectToReset.transform.rotation = Quaternion.identity;
    }

    public void SetPrefabList(GameObject[] prefabs)
    {
        _prefabs = prefabs;
    }

}
