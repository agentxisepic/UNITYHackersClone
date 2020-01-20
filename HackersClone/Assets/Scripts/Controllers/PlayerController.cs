using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;

    private GameManager _gameManager;
    private Node _selected;

    private float _dragDetectTime;
    private float _lastClickTime;

    private bool _bIsDragging;


    private void Awake()
    {
        _bIsDragging = false;
        _dragDetectTime = 1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Update is called once per frame
    void Update()
    {
        SetCursorLockState();
        Interact();
        CheckForDrag();
    }

    private void CheckForDrag()
    {
        if (Input.GetMouseButton(0) && Cursor.lockState == CursorLockMode.None && !_bIsDragging && Time.time > _lastClickTime + _dragDetectTime)
        {
            StartDrag();
        }
        else if (Input.GetMouseButtonUp(0) && Cursor.lockState == CursorLockMode.None && _bIsDragging)
        {
            EndDrag();
        }
    }

    private void SetCursorLockState()
    {
        bool bLeftControlPressed = Input.GetKey(KeyCode.LeftControl);

        if (bLeftControlPressed)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Interact()
    {
        bool bCanInteract = Cursor.lockState == CursorLockMode.None;

        if (Input.GetMouseButtonDown(0) && bCanInteract)
        {
            _selected = Click().collider?.GetComponent<Node>();
            _lastClickTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0) && bCanInteract && !_bIsDragging)
        {
            if (_selected != null) //TODO check for different Objects, right now this is only for Nodes, Change Selected to a GameObject
            {
                _selected?.RequestUI();
            }
            else if (_selected == null)
            {
                _gameManager._NodeNetworkManager.ReturnUI(null);
            }
        }
    }

    private RaycastHit Click()
    { 
        Vector3 vectorToCastTowards = _playerCamera.transform.TransformDirection(Vector3.forward);

        RaycastHit hitInfo;
        Physics.Raycast(_playerCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);
        return hitInfo;
    }

    private void StartDrag()
    {
        _bIsDragging = true;
    }

    private void EndDrag()
    {
        _bIsDragging = false;
        Node secondNode = Click().collider?.GetComponent<Node>();
        if (secondNode == null || secondNode == _selected) { return; }
        _selected?.RequestConnection(secondNode);
        _selected = null;
    }


}
