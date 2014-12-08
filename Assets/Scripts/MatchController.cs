using UnityEngine;
using System.Collections.Generic;

public class MatchController : MonoBehaviour
{
    class SwapInfo
    {
        public Vector2 v1, v2;
        public SwapInfo(Vector2 v1, Vector2 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }
    }
    [SerializeField]
    List<GameObject> items = new List<GameObject>();

    [SerializeField]
    int sizeHoriz= 7, sizeVert = 7;

    [SerializeField]
    Vector3 vecHoriz = Vector3.left, vecVert = Vector3.forward;

    [SerializeField]
    float concurTime = 0.5f;

    List<List<MatchElement>> elements = null;

    float nextConcurTime = 0;

    Vector2 selected = new Vector2(-1, -1);

    SwapInfo swapToUndo = null;
    
    void Start()
    {
        CreateField();
    }

    private void CreateField()
    {
        elements = new List<List<MatchElement>>(sizeHoriz);
        for (int h = 0; h < sizeHoriz; ++h)
            elements.Add(new List<MatchElement>(sizeVert));

        for (int h = 0; h < sizeHoriz; ++h)
            for (int v = 0; v < sizeVert; ++v)
                elements[h].Add(CreateElement(Pos(h, v)));
        
    }

    private Vector3 Pos(int h, int v)
    {
        return transform.position + vecHoriz * h + vecVert * v;
    }

    private MatchElement CreateElement(Vector3 pos)
    {
        int r = Random.Range(0, items.Count);
        var go = Instantiate(items[r]) as GameObject;
        go.transform.parent = transform;
        var element = go.GetOrAddComponent<MatchElement>();
        element.kind = r;
        element.SetPosInstant(pos);
        element.CreateCollider(vecHoriz + vecVert);
        return element;
    }

    void Update()
    {
        if (nextConcurTime < Time.time)
        {
            float time = FindConcur(concurTime);
            if (time > 0)
            {
                nextConcurTime = Time.time + time;
                selected = new Vector2(-1, -1);
                swapToUndo = null;
            }
            else
            {
                if (swapToUndo != null)
                {
                    Swap(swapToUndo.v1, swapToUndo.v2);
                    swapToUndo = null;
                }
                else
                {
                    CheckInput();
                }
            }
        }
        
    }

    void CheckInput()
    {
        if (Input.GetMouseButton(0))
        {
            var cam = Camera.main;
            var ray = cam.ScreenPointToRay(Input.mousePosition);

            Vector2 selNew = new Vector2(-1,-1);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                for (int h = 0; h < sizeHoriz; ++h)
                    for (int v = 0; v < sizeVert; ++v)
                    {
                        if (hitInfo.collider.gameObject == elements[h][v].gameObject)
                        {
                            selNew = new Vector2(h, v);
                            Debug.Log("found " + selNew);
                        }
                    }
            }
            if (Inside(selected))
            {
                if (Inside(selNew))
                {
                    if (SwapAllowed(selected, selNew))
                    {
                        Swap(selected, selNew);
                        selected = new Vector2(-1, -1);
                    }
                    else
                    {
                        Debug.Log("Swap not allowed " + selected + " " + selNew);
                        selected = selNew;
                    }
                }
            }
            else
            {
                if(Inside(selNew))
                    selected = selNew;
            }
        }
    }

    private bool SwapAllowed(Vector2 a, Vector2 b)
    {
        if (a.x == b.x && a.y == b.y) return false;
        if (a.x != b.x && a.y != b.y) return false;
        if (Mathf.Abs(a.x - b.x) > 1.5f) return false;
        if (Mathf.Abs(a.y - b.y) > 1.5f) return false;
        return true;
    }

    private void Swap(Vector2 s1, Vector2 s2)
    {
        int h1 = Mathf.FloorToInt(s1.x);
        int h2 = Mathf.FloorToInt(s2.x);
        int v1 = Mathf.FloorToInt(s1.y);
        int v2 = Mathf.FloorToInt(s2.y);

        var el = elements[h1][v1];
        elements[h1][v1] = elements[h2][v2].MoveTo(Pos(h1, v1), concurTime);
        elements[h2][v2] = el.MoveTo(Pos(h2, v2), concurTime);

        Debug.Log("Swap " + s1 + " " + s2);
        nextConcurTime = Time.time + concurTime;
        swapToUndo = new SwapInfo(s1, s2);
    }

    private bool Inside(Vector2 pos)
    {
        return pos.x >= 0 && pos.x < sizeHoriz && pos.y >= 0 && pos.y < sizeVert;
    }

    private float FindConcur(float concurTime)
    {
        int bonus3 = 0;
        int bonus4 = 0;
        int bonus5 = 0;

        
        MarkExploded(concurTime, ref bonus3, ref bonus4, ref bonus5);

        float time = RemoveExplodedAndAddNew(concurTime);

        return time;// bonus3 > 0 || bonus4 > 0 || bonus5 > 0;
    }
    /// <summary>
    /// removes used elements and adds new
    /// </summary>
    /// <param name="concurTime">move time for one cell</param>
    /// <returns>wait time (depends on the path)</returns>
    private float RemoveExplodedAndAddNew(float concurTime)
    {
        int maxCount = 0;
        for (int h = 0; h < sizeHoriz; ++h)
        {
            int vertCount = 0;
            for (int v = 0; v < sizeVert; ++v)
                while (elements[h][v].isExploded)
                {
                    int vert = v;
                    while (vert + 1 < sizeVert)
                    {
                        elements[h][vert] = elements[h][vert + 1].MoveTo(Pos(h, vert), concurTime);
                        ++vert;
                    }
                    elements[h][sizeVert - 1] = CreateElement(Pos(h, sizeVert + vertCount)).MoveTo(Pos(h, sizeVert - 1), concurTime);
                    maxCount = Mathf.Max(++vertCount, maxCount);
                }
        }

        if(maxCount > 0)
            return concurTime;

        return 0;
    }

    private void MarkExploded(float concurTime, ref int bonus3, ref int bonus4, ref int bonus5)
    {
        float len = 2;
        for (int h = 0; h < sizeHoriz - len; ++h)
            for (int v = 0; v < sizeVert; ++v)
            {
                //check horiz
                if (elements[h][v].kind == elements[h + 1][v].kind && elements[h][v].kind == elements[h + 2][v].kind)
                {
                    elements[h][v].Explode(concurTime);
                    elements[h + 1][v].Explode(concurTime);
                    elements[h + 2][v].Explode(concurTime);
                    if ((h + 3 < sizeHoriz) && elements[h + 3][v].kind == elements[h][v].kind)
                    {
                        elements[h + 3][v].Explode(concurTime);
                        if ((h + 4 < sizeHoriz) && elements[h + 4][v].kind == elements[h][v].kind)
                        {
                            bonus5++;
                            elements[h + 4][v].Explode(concurTime);
                        }
                        else
                        {
                            bonus4++;
                        }
                    }
                    else
                    {
                        bonus3++;
                    }
                }
            }
        
        for (int h = 0; h < sizeHoriz; ++h)
            for (int v = 0; v < sizeVert - len; ++v)
            {
                //check vert
                if (elements[h][v].kind == elements[h][v + 1].kind && elements[h][v].kind == elements[h][v + 2].kind)
                {
                    elements[h][v].Explode(concurTime);
                    elements[h][v + 1].Explode(concurTime);
                    elements[h][v + 2].Explode(concurTime);
                    if ((v + 3 < sizeVert) && elements[h][v + 3].kind == elements[h][v].kind)
                    {
                        elements[h][v + 3].Explode(concurTime);
                        if ((v + 4 < sizeVert) && elements[h][v + 4].kind == elements[h][v].kind)
                        {
                            bonus5++;
                            elements[h][v + 4].Explode(concurTime);
                        }
                        else
                        {
                            bonus4++;
                        }
                    }
                    else
                    {
                        bonus3++;
                    }
                }
            }

    }
}
