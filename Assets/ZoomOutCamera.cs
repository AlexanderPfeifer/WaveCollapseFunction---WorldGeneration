using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ZoomOutCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera zoomOutCam;
    [SerializeField] private Vector2 zoomOutBuffer;

    [SerializeField] private float zoomTime;

    public IEnumerator ZoomOut(Vector2Int gridSize, Vector2Int gridMiddle)
    {
        // Calculate the center of the world
        Vector2Int groupCenter = gridMiddle / 2;

        // Target position for the camera
        Vector3 targetPosition = new Vector3(groupCenter.x, groupCenter.y, zoomOutCam.transform.position.z);

        // Calculate the target orthographic size
        float targetOrthographicSize = Mathf.Max(
            (gridSize.x + zoomOutBuffer.x) / zoomOutCam.Lens.Aspect / 2f,
            (gridSize.y + zoomOutBuffer.y) / 2f
        );
        
        // Store initial values
        Vector3 initialPosition = zoomOutCam.transform.position;
        float initialOrthographicSize = zoomOutCam.Lens.OrthographicSize;

        float elapsedTime = 0f;

        // Smoothly interpolate position and orthographic size
        while (elapsedTime < zoomTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomTime;

            // Interpolate position and orthographic size
            zoomOutCam.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            zoomOutCam.Lens.OrthographicSize = Mathf.Lerp(initialOrthographicSize, targetOrthographicSize, t);

            yield return null;
        }

        // Ensure final values are exactly set
        zoomOutCam.transform.position = targetPosition;
        zoomOutCam.Lens.OrthographicSize = targetOrthographicSize;    
    }

    public void ChangeZoomCamPriority()
    {
        zoomOutCam.Priority -= 2;
    }
}
