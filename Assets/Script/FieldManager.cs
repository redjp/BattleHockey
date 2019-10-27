using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    [SerializeField]
    private GameObject leftBottom;
    [SerializeField]
    private GameObject rightBottom;
    private Vector3 vec;

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        transform.localScale = GameManager.fieldScale;

        leftBottom.transform.localScale = GameManager.bottomScale;
        rightBottom.transform.localScale = GameManager.bottomScale;

        vec = leftBottom.transform.localPosition;
        vec.z = -1 - (2 - GameManager.bottomScale.z) / 2;
        leftBottom.transform.localPosition = vec;
        vec = rightBottom.transform.localPosition;
        vec.z = 1 + (2 - GameManager.bottomScale.z) / 2;
        rightBottom.transform.localPosition = vec;
    }
}
