using UnityEngine;

public class CameraController : MonoBehaviour
{
    private int _last;

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            RotateAroundOrigin();
        }
        Zoom();
    }

    private void RotateAroundOrigin()
    {
        

        if (Input.GetMouseButton(1))
        { 
            float yRotation = transform.eulerAngles.y;
            float horizontalInput = Input.GetAxis("Mouse Y");

            //Perfect Axis Alignment
            if (yRotation == 0 || yRotation == 180)
            {
                VerticalRotation(Vector3.right, Vector3.zero, horizontalInput);
            }
            else if (yRotation == 90 || yRotation == 270)
            {
                VerticalRotation(Vector3.zero, Vector3.forward, horizontalInput);
            }

            //As long as the Player holds the mouse button, do this. As the camera rotates smoothly stops it from thinking it's in different quadrant
            if (_last == 0)
            {
                VerticalRotation(Vector3.left, Vector3.back, horizontalInput);
                return;
            }
            else if (_last == 1)
            {
                VerticalRotation(Vector3.left, Vector3.forward, horizontalInput);
                return;
            }

            
            bool[] quadrants = { yRotation < 360 && yRotation > 270, yRotation > 0 && yRotation < 90, yRotation < 180 && yRotation > 90, yRotation < 280 && yRotation > 180 };

            //Determine which quadrant to rotate as on first button press
            if (quadrants[0] || quadrants[2])
            {
                _last = 0;
                return;
            }
            else if (quadrants[1] || quadrants[3])
            {
                _last = 1;
                return;
            }
            
        }
        else
        {
            float verticalInput = Input.GetAxis("Mouse X");
            _last = 2;
            transform.RotateAround(Vector3.zero, Vector3.up, 5f * verticalInput);
        }



    }

    private void Zoom()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        transform.Translate(Vector3.forward * scrollWheel, Space.Self);

    }

    private void VerticalRotation(Vector3 xRotation, Vector3 zRotation, float horizontalInput)
    {
        transform.RotateAround(Vector3.zero, xRotation, 5f * horizontalInput);
        transform.RotateAround(Vector3.zero, zRotation, 5f * horizontalInput);
        transform.localRotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0));
    }


}
