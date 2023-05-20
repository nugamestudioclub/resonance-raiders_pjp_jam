using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class DisplayPath : MonoBehaviour
{
    [SerializeField]
    private GameObject pathArrow;
    [SerializeField]
    private float snapSize = 1;

    private PathCreator _pathCreator;

    [SerializeField]
    private float yOffset = 1;
    [SerializeField]
    private float arrowScale = 1;
    [SerializeField]
    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        _pathCreator = gameObject.GetComponent<PathCreator>();

        if (_pathCreator != null)
        {
            MakePath();
            _pathCreator.pathUpdated += OnPathChanged;
        }
    }

    void ClearPath()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void MakePath()
    {
        List<Vector3> points = Utils.GetPathPoints(_pathCreator, snapSize);
        line.positionCount = points.Count*2;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 point = points[i];
            Vector3 nextPoint = points[i + 1];
            line.SetPosition(i*2, points[i]+Vector3.up*yOffset);
            line.SetPosition(1+(i*2), points[i]+Vector3.up*yOffset);
            /*GameObject gameObject = Instantiate(pathArrow, transform);

            gameObject.transform.position = point;
            gameObject.transform.position += Vector3.up * yOffset;
            gameObject.transform.localScale *= arrowScale;
            gameObject.transform.LookAt(nextPoint);*/
        }
    }

    void OnPathChanged()
    {
        ClearPath();
        MakePath();
    }
}
