using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopRotationY : MonoBehaviour
{
    [SerializeField, Range(0, 5)]
    private float speed = 1;
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime * 10f);       
    }
}
