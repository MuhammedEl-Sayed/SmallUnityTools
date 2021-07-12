using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[ExecuteInEditMode]
public class FlexChildren : MonoBehaviour
{
    [HideInInspector]

    public Vector2 childHeightMinMax;
    [HideInInspector]
    public bool autoHeight;
    [HideInInspector]
    public bool autoWidth;
    [HideInInspector]

    public Vector2 childWidthMinMax;
    [HideInInspector]

    public int childOrder;
    [HideInInspector]

    public int childFlexGrow;


    [HideInInspector]

    public int childFlexShrink;

    [HideInInspector]
    public int childFlexTypeIndex;
    public float flexBasisSize;
    [HideInInspector]

    public GameObject parentCanvas;
    [HideInInspector]

    public GameObject parentContainer;
    [HideInInspector]
    public Vector4 constraintTypeIndex;
    [HideInInspector]
    public float containerConstraintsHeightx;
    [HideInInspector]
    public float containerConstraintsHeighty;

    [HideInInspector]
    public float containerConstraintsWidthx;
    [HideInInspector]
    public float containerConstraintsWidthy;

    void Awake()
    {
        parentCanvas = transform.root.gameObject;
        parentContainer = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
