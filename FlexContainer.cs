using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
[ExecuteInEditMode]

public class FlexContainer : MonoBehaviour
{
    [HideInInspector]
    public bool RootContainer;
    [HideInInspector]
    public bool ChildContainer;
    [HideInInspector]
    public int displayTypeIndex;
    [HideInInspector]
    public int flexDirectionIndex;

    [HideInInspector]
    public int flexWrapIndex;



    [HideInInspector]
    public int justifyContentIndex;

    [HideInInspector]
    public int alignItemsIndex;

    [HideInInspector]
    public int alignContentIndex;


    [Serializable]
    public class ChildrenData
    {
        public RectTransform childRect;
        public Vector2 childHeightMinMax;
        public Vector2 childWidthMinMax;
        public int childOrder;
        public int childFlexGrow;
        public int childFlexShrink;
        public float childFlexBasis;
        public int flexBasisType;
        public float definedBasis;
        public int violateType = 0;
        public bool isFrozen = false;
        public Vector2 targetMainSize;
        public float hypotheticalMainSize;
        public bool autoHeight;
        public bool autoWidth;
        public int LineNumber = 1;
        public bool nestedContainer = false;


    }
    [Serializable]
    [HideInInspector]
    public class LineData
    {
        public int lineNumber;
        public float crossSize;
    }
    [HideInInspector]
    public Dictionary<(int, int), LineData> LineDataDic = new Dictionary<(int, int), LineData>();
    [HideInInspector]
    public List<int> childFlexGrowList = new List<int>();


    [HideInInspector]
    public List<int> childFlexShrinkList = new List<int>();

    [HideInInspector]

    private List<float> flexBasisSize;

    [HideInInspector]
    public Dictionary<int, ChildrenData> childrenDict = new Dictionary<int, ChildrenData>();
    [HideInInspector]
    public Dictionary<int, ChildrenData> containerDict = new Dictionary<int, ChildrenData>();
    [HideInInspector]
    public string currentChildIndex; //need to make it an int


    private Vector2 containerCenter;

    private float height;
    private float width;
    RectTransform cont;
    [HideInInspector]
    public GameObject parentCanvas;
    public bool normalContainer;

    private int wrapping;
    private float mainSize;



    [HideInInspector]
    public int numberOfChildren = 0;
    public List<int> childKeys = new List<int>();
    [HideInInspector]
    public int numberOfChildContainers;


    void Update()
    {
        parentCanvas = transform.root.gameObject;
        cont = gameObject.GetComponent<RectTransform>();
        SetContainerSize();
        GetContainerInfo();
        GetChildren();
        
        LineLengthDetermination();
        //     CalculateFlexBaseValues();

        MainSizeDetermination();

        if (ChildContainer)
        {

            MainAxisAlignment();
            CrossSizeDetermination();
        }

    }

    [HideInInspector]

    public void GetChildren()
    {
        childrenDict.Clear();
        childKeys.Clear();
        numberOfChildContainers = 0;
        float xmin;
        float xmax;
        float ymin;
        float ymax;
        for (int i = 0; i < cont.childCount; i++)
        {
            if ((RootContainer && cont.GetChild(i).gameObject.GetComponent<FlexContainer>()) || ChildContainer)
            {

                ChildrenData cd = new ChildrenData();
                if (cont.GetChild(i).gameObject.GetComponent<FlexChildren>() == null)
                {
                    cont.GetChild(i).gameObject.AddComponent<FlexChildren>();
                }
                if (cont.GetChild(i).gameObject.GetComponent<FlexContainer>() != null)
                {
                    numberOfChildContainers++;
                    cd.nestedContainer = true;
                }
                cd.childRect = cont.GetChild(i).gameObject.GetComponent<RectTransform>();
                if (cont.GetChild(i).gameObject.GetComponent<FlexChildren>().constraintTypeIndex.x == 0)
                {
                    ymin = 0;
                }
                else{
                    ymin = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().containerConstraintsHeightx;
                }
                if (cont.GetChild(i).gameObject.GetComponent<FlexChildren>().constraintTypeIndex.y == 0)
                {
                    ymax = Mathf.Infinity;
                }
                else{
                    ymax = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().containerConstraintsHeighty;
                }
                if (cont.GetChild(i).gameObject.GetComponent<FlexChildren>().constraintTypeIndex.z == 0)
                {
                    xmin = 0;
                }
                else{
                    xmin = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().containerConstraintsWidthx;
                }
                if (cont.GetChild(i).gameObject.GetComponent<FlexChildren>().constraintTypeIndex.w == 0)
                {
                    xmax = Mathf.Infinity;
                }
                else{
                    xmax = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().containerConstraintsWidthy;
                }
                cd.childHeightMinMax = new Vector2(ymin, ymax);
                cd.childWidthMinMax = new Vector2(xmin, xmax);
                cd.childFlexGrow = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().childFlexGrow;
                cd.childFlexShrink = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().childFlexShrink;
                // cd.childOrder = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().childOrder;
                cd.flexBasisType = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().childFlexTypeIndex;
                if (cd.flexBasisType == 2)
                {
                    cd.childFlexBasis = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().flexBasisSize;

                }
                //Im automating childOrder, need to distinguish between auto and manual
                cd.childOrder = i;
                cont.GetChild(i).gameObject.GetComponent<FlexChildren>().childOrder = i;
                cd.autoHeight = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().autoHeight;
                cd.autoWidth = cont.GetChild(i).gameObject.GetComponent<FlexChildren>().autoWidth;
                childrenDict.Add(cont.GetChild(i).gameObject.GetInstanceID(), cd);

                childKeys.Add(cont.GetChild(i).gameObject.GetInstanceID());

            }
        }
        childrenDict = childrenDict.OrderBy(x => x.Value.childOrder).ToDictionary(x => x.Key, x => x.Value);
        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {


        }

    }

    public void SetContainerSize()
    {

        /*         cont.sizeDelta = containerPreferredSize / parentCanvas.GetComponent<Canvas>().scaleFactor;
                cont.localPosition = new Vector3(0, 0, 0); */
        if (RootContainer)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            SetChildContainerSize();
        }




    }
    public void SetChildContainerSize()
    {

        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {
            if (k.Value.nestedContainer)
            {
                if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
                {
                    k.Value.childRect.sizeDelta = new Vector2(Screen.width / numberOfChildContainers, Screen.height);
                }
                else k.Value.childRect.sizeDelta = new Vector2(Screen.width, Screen.height / numberOfChildContainers);
                k.Value.childRect.localPosition = new Vector3(0, 0, 0);
            }

        }

    }
    public void GetContainerInfo()
    {

        containerCenter = cont.rect.center;
        height = cont.rect.height;
        width = cont.rect.width;
    }

    public void LineLengthDetermination()
    {
        float remainingPrecent = 100;
        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {
            if (k.Value.childFlexBasis != 0)
            {
                if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
                {

                    k.Value.definedBasis = cont.sizeDelta.x * (k.Value.childFlexBasis / 100);
                    remainingPrecent -= k.Value.childFlexBasis;

                        k.Value.hypotheticalMainSize = Mathf.Clamp(k.Value.definedBasis, k.Value.childHeightMinMax.x, k.Value.childHeightMinMax.y);
                        Debug.Log("Hypo: " + k.Value.hypotheticalMainSize);

 

                }
                if (flexDirectionIndex == 2 || flexDirectionIndex == 3)
                {
                    k.Value.definedBasis = cont.sizeDelta.y * (k.Value.childFlexBasis / 100);
                    remainingPrecent -= k.Value.childFlexBasis;
                    k.Value.hypotheticalMainSize =  Mathf.Clamp(k.Value.definedBasis, k.Value.childHeightMinMax.x, k.Value.childHeightMinMax.y);
                    
                }

            }
            else
            {
                if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
                {
                    k.Value.definedBasis = (cont.sizeDelta.x * ((remainingPrecent/100)/cont.childCount));
                    Debug.Log("Testing" + ((remainingPrecent/100)/cont.childCount));
                    k.Value.hypotheticalMainSize =  Mathf.Clamp(k.Value.definedBasis, k.Value.childHeightMinMax.x, k.Value.childHeightMinMax.y);

                    Debug.Log("Hypo: " + k.Value.hypotheticalMainSize);
                }
                if (flexDirectionIndex == 2 || flexDirectionIndex == 3)
                {
                    k.Value.definedBasis = (cont.sizeDelta.y *((remainingPrecent/100)/cont.childCount));
                    k.Value.hypotheticalMainSize =  Mathf.Clamp(k.Value.definedBasis, k.Value.childHeightMinMax.x, k.Value.childHeightMinMax.y);
                    Debug.Log("Hypo: " + cont.sizeDelta.y / cont.childCount);

                }

            }

        }
    }
    private bool firstItem;

    public void MainSizeDetermination()
    {
        var edge = RectTransform.Edge.Left;
        var inset = cont.rect.width;
        bool row = true;
        float positionDisplacementSign = 1f;
        Vector3 rtPrevCornerStart = new Vector3();
        Vector3 rtPrevCornerEnd = new Vector3();
        Vector3 rtCornerEnd = new Vector3();
        Vector3 firstCornerStart = new Vector3();
        Vector3 firstCornerEnd = new Vector3();
        Vector3 rtDimension = new Vector3();

        float totalDistance = 0f;
        float mainAxisSize = 0f;

        firstItem = true;
        RectTransform rtPrev = null;
        RectTransform rtFirst = null;
        RectTransform rtSelf = null;
        Vector3[] firstCorners = new Vector3[4];
        Vector3[] rtCorners = new Vector3[4];
        Vector3[] rtPrevCorners = new Vector3[4];
        Vector3 tempCorner = new Vector3();
        Debug.Log("justify content" + justifyContentIndex);
        if (justifyContentIndex == 1)
        {
            childrenDict = childrenDict.OrderByDescending(x => x.Value.childOrder).ToDictionary(x => x.Key, x => x.Value);
        }
        else
        {
            childrenDict = childrenDict.OrderBy(x => x.Value.childOrder).ToDictionary(x => x.Key, x => x.Value);

        }
        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {
            RectTransform rt = k.Value.childRect;
            switch (flexDirectionIndex)
            {
                case 0:
                    if (justifyContentIndex == 1)
                    {
                        edge = RectTransform.Edge.Right;

                    }
                    else
                    {
                        edge = RectTransform.Edge.Left;
                    }

                    inset = rt.rect.width;
                    positionDisplacementSign = 1f;
                    row = true;
                    break;
                case 1:
                    if (justifyContentIndex == 1)
                    {
                        edge = RectTransform.Edge.Left;

                    }
                    else
                    {
                        edge = RectTransform.Edge.Right;
                    }

                    inset = rt.rect.width;
                    positionDisplacementSign = -1f;
                    row = true;
                    break;
                case 2:
                    if (justifyContentIndex == 1)
                    {
                        edge = RectTransform.Edge.Bottom;

                    }
                    else
                    {
                        edge = RectTransform.Edge.Top;
                    }

                    inset = rt.rect.height;
                    positionDisplacementSign = -1f;
                    row = false;
                    break;
                case 3:
                    if (justifyContentIndex == 1)
                    {
                        edge = RectTransform.Edge.Top;

                    }
                    else
                    {
                        edge = RectTransform.Edge.Bottom;
                    }
                    inset = rt.rect.height;
                    positionDisplacementSign = 1f;
                    row = false;
                    break;
            }
            if (row)
            {
                k.Value.childRect.sizeDelta = new Vector2(k.Value.hypotheticalMainSize, k.Value.childRect.sizeDelta.y);
            }
            else
            {
                k.Value.childRect.sizeDelta = new Vector2(k.Value.childRect.sizeDelta.x, k.Value.hypotheticalMainSize);
            }
            if (firstItem)
            {
                rt.localPosition = new Vector3(0, 0, 0);
                rt.SetInsetAndSizeFromParentEdge(edge, 0, inset);
                Debug.Log(rt.gameObject.name + "name");
                rtPrev = rt;
                rtFirst = rt;
                firstItem = false;
                rt.GetWorldCorners(firstCorners);
                rtPrev.GetWorldCorners(rtPrevCorners);
                continue;
            }
            else
            {
                rt.GetWorldCorners(rtCorners);
                if (row)
                {
                    rtPrevCornerStart = new Vector3(rtPrevCorners[0].x, 0, 0);
                    rtPrevCornerEnd = new Vector3(rtPrevCorners[3].x, 0, 0);
                    tempCorner = new Vector3(rtCorners[0].x, 0, 0);
                    rtCornerEnd = new Vector3(rtCorners[3].x, 0, 0);
                    if ((flexDirectionIndex == 1 && justifyContentIndex != 1) || (flexDirectionIndex == 0 && justifyContentIndex == 1))
                    {
                        rt.localPosition = new Vector2(rtPrev.localPosition.x - rtPrev.rect.width, rtPrev.localPosition.y);
                    }
                    else if ((flexDirectionIndex == 0 && justifyContentIndex != 1) || (flexDirectionIndex == 1 && justifyContentIndex == 1))
                    {
                        rt.localPosition = new Vector2(rtPrev.localPosition.x + rtPrev.rect.width, rtPrev.localPosition.y);
                    }
                    firstCornerStart = new Vector3(0, firstCorners[0].y, 0);
                    firstCornerEnd = new Vector3(0, firstCorners[1].y, 0);
                    rtDimension = new Vector3(0, rtCorners[0].y, 0);
                }
                else
                {
                    rtPrevCornerStart = new Vector3(0, rtPrevCorners[0].y, 0);
                    rtPrevCornerEnd = new Vector3(0, rtPrevCorners[1].y, 0);
                    tempCorner = new Vector3(0, rtCorners[0].y, 0);
                    rtCornerEnd = new Vector3(0, rtCorners[1].y, 0);
                    if ((flexDirectionIndex == 2 && justifyContentIndex != 1) || (flexDirectionIndex == 3 && justifyContentIndex == 1))
                    {
                        rt.localPosition = new Vector2(rtPrev.localPosition.x, rtPrev.localPosition.y - rtPrev.rect.height);

                    }
                    else if ((flexDirectionIndex == 3 && justifyContentIndex != 1) || (flexDirectionIndex == 2 && justifyContentIndex == 1))
                    {
                        rt.localPosition = new Vector2(rtPrev.localPosition.x, rtPrev.localPosition.y + rtPrev.rect.height);
                    }
                    firstCornerStart = new Vector3(firstCorners[0].x, 0, 0);
                    firstCornerEnd = new Vector3(firstCorners[3].x, 0, 0);
                    rtDimension = new Vector3(rtCorners[0].x, 0, 0);
                }



                if (!ChildrenFit(rt) && flexWrapIndex == 0)
                {
                    //      Debug.Log("Is it entering here?");
                    rt.SetInsetAndSizeFromParentEdge(edge, 0, inset);
                    if (row)
                        rt.localPosition = new Vector2(rt.localPosition.x, rt.localPosition.y - rtFirst.rect.height);
                    else
                        rt.localPosition = new Vector2(rt.localPosition.x - rtFirst.rect.width, rt.localPosition.y);


                }
                else if (!ChildrenFit(rt) && flexWrapIndex == 2)
                {
                    rt.SetInsetAndSizeFromParentEdge(edge, 0, inset);
                    if (row)
                        rt.localPosition = new Vector2(rt.localPosition.x, rt.localPosition.y + rtFirst.rect.height);
                    else
                        rt.localPosition = new Vector2(rt.localPosition.x + rtFirst.rect.width, rt.localPosition.y);

                }




                float distancetoFirst = Vector3.Distance(firstCornerStart, firstCornerEnd);
                float heightofFirst = Vector3.Distance(firstCornerEnd, rtDimension);
                if (distancetoFirst != 0)
                {
                    k.Value.LineNumber = Mathf.RoundToInt(Math.Abs(distancetoFirst / heightofFirst));
                }
                else
                {
                    k.Value.LineNumber = 1;
                }


                //    Debug.Log("LineNumber " + k.Value.LineNumber);
                rtPrev = rt;
                rtPrev.GetWorldCorners(rtPrevCorners);

            }

        }



        ResolveFlexibleLengths();



    }



    public float InnerMainSize(Dictionary<int, ChildrenData> cd)
    {
        //Resize to hypothetical main size and calculate flexLineSize
        float flexLineSize = 0;
        foreach (KeyValuePair<int, ChildrenData> k in cd)
        {
            if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
            {
                flexLineSize += k.Value.hypotheticalMainSize;

            }
            else if (flexDirectionIndex == 2 || flexDirectionIndex == 3)
            {
                flexLineSize += k.Value.hypotheticalMainSize;
            }
        }

        return flexLineSize;


    }
    public float InnerMainSize(Dictionary<int, ChildrenData> cd, bool checkForFrozen)
    {
        //Resize to hypothetical main size and calculate flexLineSize
        float flexLineSize = 0;
        if (checkForFrozen)
        {
            foreach (KeyValuePair<int, ChildrenData> k in cd)
            {
                if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
                {
                    if (k.Value.isFrozen)
                    {
                        flexLineSize += k.Value.targetMainSize.x;
                    }
                    else
                    {
                        flexLineSize += k.Value.definedBasis;
                    }


                }
                else if (flexDirectionIndex == 2 || flexDirectionIndex == 3)
                {
                    if (k.Value.isFrozen)
                    {
                        flexLineSize += k.Value.targetMainSize.y;
                    }
                    else
                    {
                        flexLineSize += k.Value.definedBasis;
                    }


                }
            }

        }

        return flexLineSize;


    }
    public void printChildrenDict()
    {
        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {
            Debug.Log("Key: " + k.Key + " Rect: " + k.Value.childRect);
        }
    }
    public void ResolveFlexibleLengths()
    {
        bool FlexGrow = false;
        int frozenItems = 0;
        float freeSpace = 0;
        float contMainSize = 0;
        float targetSizex = 0;
        float targetSizey = 0;
        float maxSizeComparison = 0;
        bool mainAxisWidth = false;
        switch (flexDirectionIndex)
        {
            case 0:
            case 1:
                mainAxisWidth = true;
                contMainSize = cont.sizeDelta.x;
                break;
            case 2:
            case 3:
                mainAxisWidth = false;
                contMainSize = cont.sizeDelta.y;
                break;
        }

        if (InnerMainSize(childrenDict) < contMainSize)
        {
            FlexGrow = true;
        }

        for (int i = 0; i < childrenDict.Count; i++)
        {
            if (mainAxisWidth)
            {
                targetSizex = childrenDict[childKeys[i]].definedBasis;
                targetSizey = childrenDict[childKeys[i]].childRect.sizeDelta.y;
            }
            else
            {
                targetSizex = childrenDict[childKeys[i]].childRect.sizeDelta.x;
                targetSizey = childrenDict[childKeys[i]].definedBasis;
            }
            childrenDict[childKeys[i]].targetMainSize = new Vector2(targetSizex, targetSizey);
        }
        for (int i = 0; i < childrenDict.Count; i++)
        {
            if (mainAxisWidth)
            {
                maxSizeComparison = childrenDict[childKeys[i]].hypotheticalMainSize;
                targetSizex = maxSizeComparison;
                targetSizey = childrenDict[childKeys[i]].childRect.sizeDelta.y;
            }
            else
            {
                maxSizeComparison = childrenDict[childKeys[i]].hypotheticalMainSize;
                targetSizex = childrenDict[childKeys[i]].childRect.sizeDelta.x;
                targetSizey = maxSizeComparison;

            }
            if ((FlexGrow && childrenDict[childKeys[i]].definedBasis > maxSizeComparison) || (!FlexGrow && childrenDict[childKeys[i]].definedBasis < maxSizeComparison) || (childrenDict[childKeys[i]].childFlexGrow == 0 && childrenDict[childKeys[i]].childFlexShrink == 0))
            {

                childrenDict[childKeys[i]].targetMainSize = new Vector2(targetSizex, targetSizey);
                childrenDict[childKeys[i]].isFrozen = true;

            }
            else
            {
                frozenItems++;

            }


        }

        while (frozenItems < numberOfChildren)
        {

            freeSpace = CalculateInitialFreeSpace(true);

            int violations = 0;
            float remainingFreeSpace = 0;
            int sumOfFlexValues = 0;
            float adjustmentSum = 0;
            Vector2 clampedtargetMainSize = new Vector2();
            float violationCheck = 0;
            float clampCheck = 0;
            foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
            {
                if (k.Value.isFrozen == true)
                {
                    continue;
                }
                if (FlexGrow)
                {
                    sumOfFlexValues += k.Value.childFlexGrow;
                }
                else
                {
                    sumOfFlexValues += k.Value.childFlexShrink;
                }

            }
            if (sumOfFlexValues < 1)
            {
                remainingFreeSpace = freeSpace * sumOfFlexValues;
                if (remainingFreeSpace > freeSpace)
                {
                    remainingFreeSpace = freeSpace;
                }

            }
            foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
            {
                if (k.Value.isFrozen)
                {

                    continue;
                }
                switch (flexDirectionIndex)
                {
                    case 0:
                    case 1:
                        targetSizex = k.Value.definedBasis + ((k.Value.childFlexGrow / sumOfFlexValues) * remainingFreeSpace);
                        targetSizey = k.Value.childRect.sizeDelta.y;
                        clampedtargetMainSize = new Vector2(Mathf.Clamp(targetSizex, k.Value.childWidthMinMax.x, k.Value.childWidthMinMax.y), targetSizey);
                        violationCheck = clampedtargetMainSize.x;
                        break;
                    case 2:
                    case 3:
                        targetSizex = k.Value.childRect.sizeDelta.x;
                        targetSizey = k.Value.definedBasis + ((k.Value.childFlexGrow / sumOfFlexValues) * remainingFreeSpace);
                        clampedtargetMainSize = new Vector2(targetSizex, Mathf.Clamp(targetSizey, k.Value.childHeightMinMax.x, k.Value.childHeightMinMax.y));
                        violationCheck = clampedtargetMainSize.y;
                        break;
                }
                if (remainingFreeSpace != 0)
                {

                    if (FlexGrow)
                    {

                        k.Value.targetMainSize = new Vector2(targetSizex, targetSizey);
                        Vector2 beforeClamp = k.Value.targetMainSize;
                        if (mainAxisWidth)
                        {
                            clampCheck = beforeClamp.x;
                        }
                        else clampCheck = beforeClamp.y;
                   

                        if (beforeClamp != clampedtargetMainSize)
                        {
                            violations++;
                            if (clampCheck < violationCheck)
                            {

                                k.Value.violateType = 1; //1 == min violation

                            }
                            else
                            {
                                k.Value.violateType = 2; //1 == min violation
                            }
                            adjustmentSum += violationCheck - clampCheck;
                        }
                    }
                    else
                    {
                        float scaledShrink = 0;
                        if (mainAxisWidth)
                        {
                            scaledShrink = k.Value.childFlexShrink * k.Value.childRect.sizeDelta.x;
                            float scaledToRest = scaledShrink / sumOfFlexValues;
                            k.Value.targetMainSize = new Vector2(k.Value.definedBasis - (Mathf.Abs(remainingFreeSpace / scaledToRest) * remainingFreeSpace), k.Value.childRect.sizeDelta.y);
                            clampedtargetMainSize = new Vector2(Mathf.Clamp(k.Value.definedBasis - (Mathf.Abs(remainingFreeSpace / scaledToRest) * remainingFreeSpace), k.Value.childWidthMinMax.x, k.Value.childWidthMinMax.y), k.Value.childRect.sizeDelta.y);
                            targetSizex = k.Value.targetMainSize.x;
                            targetSizey = k.Value.childRect.sizeDelta.y;
                            violationCheck = clampedtargetMainSize.x;
                        }

                        else
                        {
                            scaledShrink = k.Value.childFlexShrink * k.Value.childRect.sizeDelta.y;
                            float scaledToRest = scaledShrink / sumOfFlexValues;
                            k.Value.targetMainSize = new Vector2(k.Value.childRect.sizeDelta.x, ((k.Value.definedBasis - (Mathf.Abs(remainingFreeSpace / scaledToRest)) * remainingFreeSpace)));
                            clampedtargetMainSize = new Vector2(k.Value.childRect.sizeDelta.x, Mathf.Clamp(k.Value.definedBasis - (Mathf.Abs(remainingFreeSpace / scaledToRest) * remainingFreeSpace), k.Value.childHeightMinMax.x, k.Value.childHeightMinMax.y));
                            targetSizey = k.Value.targetMainSize.y;
                            targetSizex = k.Value.childRect.sizeDelta.x;
                            violationCheck = clampedtargetMainSize.y;
                        }


                         k.Value.targetMainSize = new Vector2(targetSizex, targetSizey);
                        Vector2 beforeClamp = k.Value.targetMainSize;
                        if (mainAxisWidth)
                        {
                            clampCheck = beforeClamp.x;
                        }
                        else clampCheck = beforeClamp.y;
                        if (beforeClamp !=clampedtargetMainSize)
                        {
                            violations++;
                            if (clampCheck < violationCheck)
                            {

                                k.Value.violateType = 1; //1 == min violation
                            }
                            else
                            {
                                k.Value.violateType = 2; //1 == min violation
                            }
                            adjustmentSum += violationCheck - clampCheck;
                        }
                    }
                }





            }
            if (adjustmentSum == 0)
            {
                break;
            }
            else
            {
                if (adjustmentSum > 0)
                {
                    foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
                    {
                        if (k.Value.violateType == 1)
                        {
                            k.Value.isFrozen = true;
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
                    {
                        if (k.Value.violateType == 2)
                        {
                            k.Value.isFrozen = true;
                        }
                    }
                }
            }
        }
        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {
            k.Value.childRect.sizeDelta = k.Value.targetMainSize;
        }
    }
    public void CrossSizeDetermination()
    {
        LineDataDic.Clear();
        int lines = GetNumberOfLines();
        bool row = true;
        if (flexDirectionIndex == 0 || flexDirectionIndex == 1) row = true;
        else row = false;
        if (lines == 1)
        {
            LineData ln = new LineData();
            if (row) ln.crossSize = cont.rect.height;
            else ln.crossSize = cont.rect.width;
            ln.lineNumber = 1;
            LineDataDic.Add((1, 1), ln);
        }
        else
        {
            for (int i = 1; i <= lines; i++)
            {
                LineData ln = new LineData();
                ln.lineNumber = i;
                ln.crossSize = 0;
                foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
                {
                    if (i == k.Value.LineNumber)
                    {
                        if (row)
                        {
                            if (ln.crossSize < k.Value.definedBasis)
                            {
                                ln.crossSize = k.Value.definedBasis;
                            }
                        }
                        else
                        {
                            if (ln.crossSize < k.Value.definedBasis)
                            {
                                ln.crossSize = k.Value.definedBasis;
                            }
                        }
                        LineDataDic.Add((i, k.Value.childOrder), ln);
                    }


                }
            }
        }

        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {

            if (row)
            {
                k.Value.childRect.sizeDelta = new Vector2(k.Value.childRect.sizeDelta.x, Mathf.Clamp(k.Value.definedBasis, k.Value.childHeightMinMax.x, k.Value.childHeightMinMax.y));
                Debug.Log("Delta: " + k.Value.childRect.sizeDelta);

            }
            else
            {
                k.Value.childRect.sizeDelta = new Vector2( Mathf.Clamp(k.Value.definedBasis, k.Value.childWidthMinMax.x, k.Value.childWidthMinMax.y), k.Value.childRect.sizeDelta.y);
                Debug.Log("Delta: " + k.Value.childRect.sizeDelta);
            }
        }

    }

    public void MainAxisAlignment()

    {
        int lines = GetNumberOfLines();
        Vector3[] childCorners = new Vector3[4];
        float freeSpace = CalculateWorldFreeSpace();
        if (freeSpace > 0)
        {
            for (int i = 0; i <= lines; i++)
            {

                float marginForEach = freeSpace / childrenDict.Count;
                float j = 0;
                if (justifyContentIndex == 2)
                {
                    j = 0.5f;
                }
                int counter = 0;
                foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
                {


                    if (k.Value.LineNumber == i + 1)
                    {
                        if ((flexDirectionIndex == 0 && justifyContentIndex != 1) || (flexDirectionIndex == 1 && justifyContentIndex == 1))
                        {
                            k.Value.childRect.localPosition = new Vector2(k.Value.childRect.localPosition.x + marginForEach * j, k.Value.childRect.localPosition.y);


                        }
                        else if ((flexDirectionIndex == 1 && justifyContentIndex != 1) || (flexDirectionIndex == 0 && justifyContentIndex == 1))
                        {
                            k.Value.childRect.localPosition = new Vector2(k.Value.childRect.localPosition.x - marginForEach * j, k.Value.childRect.localPosition.y);

                        }
                        else if ((flexDirectionIndex == 2 && justifyContentIndex != 1) || (flexDirectionIndex == 3 && justifyContentIndex == 1))
                        {
                            k.Value.childRect.localPosition = new Vector2(k.Value.childRect.localPosition.x, k.Value.childRect.localPosition.y - marginForEach * j);

                        }
                        else if ((flexDirectionIndex == 3 && justifyContentIndex != 1) || (flexDirectionIndex == 2 && justifyContentIndex == 1))
                        {
                            k.Value.childRect.localPosition = new Vector2(k.Value.childRect.localPosition.x, k.Value.childRect.localPosition.y + marginForEach * j);

                        }
                        counter++;
                        j++;
                    }
                    else
                    {
                        continue;
                    }

                }
            }

        }
    }
    public int GetNumberOfLines()
    {
        int lines = 0;
        foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
        {
            if (k.Value.LineNumber > lines)
            {
                lines = k.Value.LineNumber;
            }
        }
        return lines;
    }
    public float CalculateWorldFreeSpace()
    {

        Vector3[] childCorners = new Vector3[4];


        float freeSpace = 0;
        if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
        {

            freeSpace = cont.rect.width;
        }
        else if (flexDirectionIndex == 2 || flexDirectionIndex == 3)
        {

            freeSpace = cont.rect.height;
        }



        int NumberOfLines = GetNumberOfLines();


        List<float> freeSpacePerLine = new List<float>();
        for (int i = 0; i < NumberOfLines; i++)
        {
            freeSpacePerLine.Add(freeSpace);
        }


        for (int i = 1; i <= NumberOfLines; i++)
        {
            foreach (KeyValuePair<int, ChildrenData> k in childrenDict)
            {
                if (k.Value.LineNumber == i)
                {

                    if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
                    {


                        freeSpacePerLine[i - 1] = freeSpacePerLine[i - 1] - k.Value.childRect.rect.width;
                    }
                    else if (flexDirectionIndex == 2 || flexDirectionIndex == 3)
                    {


                        freeSpacePerLine[i - 1] = freeSpacePerLine[i - 1] - k.Value.childRect.rect.height;
                    }

                }
                else
                {
                    continue;
                }


            }
        }
        foreach (float f in freeSpacePerLine)
        {

            if (f < freeSpace)
            {
                if (f < 0)
                {
                    freeSpace = 0;
                }
                else
                    freeSpace = f;
            }
        }
        //  Debug.Log("f: " + freeSpace);
        return freeSpace;

    }
    public bool ChildrenFit(RectTransform rt)
    {
        bool contained = true;

        Vector3[] childCorners = new Vector3[4];
        rt.GetLocalCorners(childCorners);
        Vector3 mainSize = new Vector3();
        if (flexDirectionIndex == 0) mainSize = new Vector3((rt.rect.width - 15) / 2, 0, 0);
        else if (flexDirectionIndex == 1) mainSize = new Vector3((-rt.rect.width + 15) / 2, 0, 0);
        else if (flexDirectionIndex == 2) mainSize = new Vector3((-rt.rect.height + 15) / 2, 0, 0);
        else if (flexDirectionIndex == 3) mainSize = new Vector3((rt.rect.height - 1) / 2, 0, 0);
        if (RectTransformUtility.RectangleContainsScreenPoint(cont, rt.position, null))
        {
            Debug.Log("ALL CONTAINED");
        }
        else
        {

            contained = false;
        }
        return contained;

    }

    public float CalculateInitialFreeSpace(bool frozen)
    {
        return ReturnNormalContainerMainSize() - InnerMainSize(childrenDict, frozen);
    }

    public float ReturnNormalContainerMainSize()
    {
        float ncMainSize = 0;
        if (flexDirectionIndex == 0 || flexDirectionIndex == 1)
        {
            ncMainSize = cont.sizeDelta.x;
        }
        else
        {
            ncMainSize = cont.sizeDelta.y;
        }
        return ncMainSize;
    }


}
