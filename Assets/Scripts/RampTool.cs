using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RampTool : MonoBehaviour
{
    //TODO this is a shite temporary solution for testing purposes
    //replace with legit editor tool or delete at the earliest convenience
    [SerializeField] List<Transform> parts;

    [SerializeField] float count = 5;
    [SerializeField] float width = 2;
    [SerializeField] float dist = .5f;
    [SerializeField] float angle = 10f;
    [Space]
    [SerializeField] bool exponential = true;

    private bool changed = true;



    private void Update()
    {
        if(!changed) return;
        changed = false;

        while(parts.Count > 0)
        {
            if(parts[0] != null) Destroy(parts[0].gameObject);
            parts.RemoveAt(0);
        }

        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Plane);
        part.transform.SetParent(transform);
        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = new Vector3(0.1f*width, 1f, 0.1f*dist);
        parts.Add(part.transform);
        for (int i = 1; i <= count; i++)
        {
            part = GameObject.CreatePrimitive(PrimitiveType.Plane);
            part.transform.SetParent(parts[i-1]);
            part.transform.localPosition = new Vector3(0, 0, dist);
            if(exponential)
            {
                int changes = 0;
                for (int j = 1; j <= count; j++) changes += j;
                float angleChange = -angle / changes;
                part.transform.localRotation = Quaternion.Euler(i*angleChange, 0, 0);
            }
            else
            {
                float angleChange = -angle / count;
                part.transform.localRotation = Quaternion.Euler(angleChange, 0, 0);
            }
            part.transform.parent = transform;
            part.transform.localScale = new Vector3(0.1f*width, 1f, 0.1f*dist);
            parts.Add(part.transform);
        }
    }


    private void OnValidate()
    {
        changed = true;
    }
}
