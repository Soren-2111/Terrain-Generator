using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{

    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;
    Vector3 pointA;
    Vector3 pointB;
    Vector3 pointC;
    Vector3 pointD;

    public int resolution = 10; // グリッドの解像度
    public float noiseScale = 0.5f; // パーリンノイズのスケール
    public float heightMultiplier = 1f; // 高さの倍率

    public int numberOfPoints = 4;//ランダムで柱として選ぶ点の数
    Vector3[] vertices;

    //[ContextMenu("生成")]
    void Start()
    {
        
    }
    [ContextMenu("生成")]
    private void Update()
    {

        pointA = A.position;
        pointB = B.position;
        pointC = C.position;
        pointD = D.position;

        GenerateTerrain(pointA, pointB, pointC, pointD);
    }

    void GenerateTerrain(Vector3 _posA,Vector3 _posB,Vector3 _posC,Vector3 _posD)
    {
        
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        
        // グリッドのサイズ
        int width = resolution + 1;
        int height = resolution + 1;

        //Vector3[] vertices = new Vector3[width * height];
        vertices = new Vector3[width * height];
        int[] triangles = new int[resolution * resolution * 6];
        //Vector2[] uvs = new Vector2[vertices.Length];

        /*
        float slopexy= (pointB.y - pointA.y) / (pointB.x - pointA.x);
        float slopeZY = (pointB.y - pointA.y) / (pointB.z - pointA.z);
        */

        // 座標の補間
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float u = (float)j / resolution;
                float v = (float)i / resolution;

                //sinのやつ
                float uSin = Mathf.Sin(u*2.0f*Mathf.PI)*0.5f;
                float vSin = Mathf.Sin(v * 2.0f * Mathf.PI)*0.5f;
                float randomNum = UnityEngine.Random.Range(heightMultiplier*-1, heightMultiplier);


                Vector3 interpolatedPoint1 = Vector3.Lerp(_posB, _posA, u);
                Vector3 interpolatedPoint2 = Vector3.Lerp(_posD, _posC, u);
                Vector3 pointOnPlane = Vector3.Lerp(interpolatedPoint1, interpolatedPoint2, v);
                //print(pointOnPlane.x + "x");
                //print(pointOnPlane.z + "z");
                //float noise = Mathf.PerlinNoise(pointOnPlane.x * noiseScale, pointOnPlane.z * noiseScale) * vSin * uSin*randomNum;
                float noise = Mathf.PerlinNoise(pointOnPlane.x * noiseScale, pointOnPlane.z * noiseScale) ;
                float heightValue = noise * heightMultiplier;
                

                //print(CalculateInterpolatedY(pointA, pointB, pointC, pointD, pointOnPlane.x, pointOnPlane.z));
                //vertices[i * width + j] = new Vector3(pointOnPlane.x, heightValue+ CalculateInterpolatedY(pointA, pointB, pointC, pointD, pointOnPlane.x, pointOnPlane.z), pointOnPlane.z);


                float y=0;
                if (IsPointInTriangle(new Vector2(pointOnPlane.x, pointOnPlane.z), pointA, pointB, pointC))
                {
                    y = GetYFromXZ(pointOnPlane.x, pointOnPlane.z, pointA, pointB, pointC);
                }
                else if (IsPointInTriangle(new Vector2(pointOnPlane.x, pointOnPlane.z), pointB, pointC, pointD))
                {
                    y = GetYFromXZ(pointOnPlane.x, pointOnPlane.z, pointB, pointC, pointD);
                }
                /*
                else if (IsPointInTriangle(new Vector2(pointOnPlane.x, pointOnPlane.z), pointA, pointC, pointD))
                {
                    y = GetYFromXZ(pointOnPlane.x, pointOnPlane.z, pointA, pointC, pointD);
                }
                else if (IsPointInTriangle(new Vector2(pointOnPlane.x, pointOnPlane.z), pointA, pointB, pointD))
                {
                    y = GetYFromXZ(pointOnPlane.x, pointOnPlane.z, pointA, pointB, pointD);
                }
                */
                else
                {
                    Vector2 originalPoint = new Vector2(pointOnPlane.x, pointOnPlane.z);
                    Vector2 closestPoint = FindClosestPointInTriangle(originalPoint, pointA, pointB, pointC);
                    y = GetYFromXZ(closestPoint.x, closestPoint.y, pointB, pointC, pointD);
                    print("どこの面にも属していない"+"iのあたい"+i+"jのあたい"+j);
                    print("x座標"+pointOnPlane.x);
                    print("z座標" +pointOnPlane.z);
                }


                vertices[i * width + j] = new Vector3(pointOnPlane.x, y+ heightValue, pointOnPlane.z);

                //uvs[i * width + j] = new Vector2(u, v);
            }
        }

        // 三角形の作成
        int triangleIndex = 0;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int bottomLeft = i * width + j;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + width;
                int topRight = topLeft + 1;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomRight;
            }
        }
        
        mesh.vertices = vertices;
        //GenerateRandomPoints(vertices);
        mesh.triangles = triangles;
        //mesh.uv = uvs;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;


        
    }

    //1
    #region
    float CalculateInterpolatedY(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float x, float z)
    {
        Vector3[] closestTriangle = DetermineClosestTriangle(a, b, c, d, x, z);

        if (closestTriangle == null)
        {
            Debug.LogError("Failed to find a valid triangle.");
            return float.NaN;
        }

        return InterpolateUsingBarycentricCoordinates(closestTriangle[0], closestTriangle[1], closestTriangle[2], x, z);
    }

    Vector3[] DetermineClosestTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float x, float z)
    {
        Vector3[][] triangles = new Vector3[] []{
            new Vector3[] { a, b, c },
            new Vector3[] { a, c, d },
            new Vector3[] { a, b, d },
            new Vector3[] { b, c, d }
        };

        Vector3 point = new Vector3(x, 0, z);
        Vector3[] closestTriangle = null;
        float minDistance = float.MaxValue;

        foreach (var triangle in triangles)
        {
            Vector3 projection = ProjectPointOntoTriangle(point, triangle[0], triangle[1], triangle[2]);
            float distance = (point - projection).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                closestTriangle = triangle;
            }
        }

        return closestTriangle;
    }

    Vector3 ProjectPointOntoTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = point - a;

        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return a + u * v0 + v * v1;
    }

    

    float InterpolateUsingBarycentricCoordinates(Vector3 a, Vector3 b, Vector3 c, float x, float z)
    {
        Vector3 p = new Vector3(x, 0, z);
        float areaABC = Vector3.Cross(b - a, c - a).magnitude;
        float areaPBC = Vector3.Cross(b - p, c - p).magnitude;
        float areaPCA = Vector3.Cross(c - p, a - p).magnitude;

        float alpha = areaPBC / areaABC;
        float beta = areaPCA / areaABC;
        float gamma = 1.0f - alpha - beta;

        return alpha * a.y + beta * b.y + gamma * c.y;
    }
    #endregion

    //2
    #region
    bool IsPointInTriangle(Vector2 p, Vector3 A, Vector3 B, Vector3 C)
    {
        Vector2 a = new Vector2(A.x, A.z);
        Vector2 b = new Vector2(B.x, B.z);
        Vector2 c = new Vector2(C.x, C.z);

        // Calculate vectors
        Vector2 v0 = c - a;
        Vector2 v1 = b - a;
        Vector2 v2 = p - a;

        // Compute dot products
        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        // Compute barycentric coordinates
        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v <= 1.5);
    }

    Vector2 FindClosestPointInTriangle(Vector2 p, Vector3 A, Vector3 B, Vector3 C)
    {
        Vector2 a = new Vector2(A.x, A.z);
        Vector2 b = new Vector2(B.x, B.z);
        Vector2 c = new Vector2(C.x, C.z);

        Vector2 closestPoint = ClosestPointOnSegment(p, a, b);
        float minDist = Vector2.Distance(p, closestPoint);

        Vector2 testPoint = ClosestPointOnSegment(p, b, c);
        float dist = Vector2.Distance(p, testPoint);
        if (dist < minDist)
        {
            minDist = dist;
            closestPoint = testPoint;
        }

        testPoint = ClosestPointOnSegment(p, c, a);
        dist = Vector2.Distance(p, testPoint);
        if (dist < minDist)
        {
            closestPoint = testPoint;
        }

        return closestPoint;
    }

    Vector2 ClosestPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    float GetYFromXZ(float x, float z, Vector3 A, Vector3 B, Vector3 C)
    {
        // Calculate the plane normal
        Vector3 normal = Vector3.Cross(B - A, C - A);

        // Extract the coefficients of the plane equation ax + by + cz + d = 0
        float a = normal.x;
        float b = normal.y;
        float c = normal.z;
        float d = -(a * A.x + b * A.y + c * A.z);

        // Use the plane equation to solve for y
        float y = -(a * x + c * z + d) / b;

        return y;
    }

    void CheckNearTriangle(float _x,float _y)
    {
        Vector2 p2 = new Vector2(_x, _y);
        Vector2 A2 = new Vector2(pointA.x, pointA.z);
        Vector2 B2 = new Vector2(pointA.x, pointA.z);
        Vector2 C2 = new Vector2(pointA.x, pointA.z);
        Vector2 D2 = new Vector2(pointA.x, pointA.z);

        Vector2[] points = new Vector2[] { A2, B2, C2, D2 };
        // 各点とpの距離を計算
        var distances = points.Select(point => (point, Vector2.Distance(p2, point))).ToList();

        // 距離が近い順にソート
        distances.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        // 最も距離が近い2点を取得
        var closestPoints = distances.Take(2).Select(d => d.point).ToArray();

    }

    #endregion

    //配列の結合
    #region 

    Vector3[] KetugouVartices(Vector3[] _vertices1,Vector3[] _vertices2)
    {
        Vector3[] vertices = new Vector3[_vertices1.Length + _vertices2.Length];
        Array.Copy(_vertices1, vertices, _vertices1.Length);
        Array.Copy(_vertices2, 0, vertices, _vertices1.Length, _vertices2.Length);
        return vertices;
    }

    int[] KetugouTriangles(int[] _vertices1, int[] _vertices2)
    {
        int[] vertices = new int[_vertices1.Length + _vertices2.Length];
        Array.Copy(_vertices1, vertices, _vertices1.Length);
        Array.Copy(_vertices2, 0, vertices, _vertices1.Length, _vertices2.Length);
        return vertices;
    }

    void CreateMesh(Vector3[] _vertices,int[] _triangles)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = _vertices;
        mesh.triangles = _triangles;
        //mesh.uv = uvs;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    #endregion

    //ランダムな場所を選び線をひく
    #region
    // ?????_?????_??????
    void GenerateRandomPoints(Vector3[] _vertices)
    {
        Vector3[] newvertices = new Vector3[numberOfPoints];
        for (int i = 0; i < numberOfPoints; i++)
        {
            int randomNum = UnityEngine.Random.Range(0, _vertices.Length);
            print(randomNum);
            newvertices[i] = _vertices[randomNum];
        }
        ArrangePointsInCircle(newvertices);

    }

    // ?_???~??????????????
    void ArrangePointsInCircle(Vector3[] _newvertices)
    {
        // ?????????_???d?S???v?Z
        Vector3 centroid = Vector3.zero;
        int i = 0;
        foreach (var point in _newvertices)
        {
            centroid += point;
            print(_newvertices[i]);
            i++;
        }
        centroid /= numberOfPoints;

        // ?_???d?S???????p?x???v?Z
        System.Array.Sort(_newvertices, (a, b) =>
        {
            float angleA = Mathf.Atan2(a.y - centroid.y, a.x - centroid.x);
            float angleB = Mathf.Atan2(b.y - centroid.y, b.x - centroid.x);
            return angleA.CompareTo(angleB);
        });
        DrawLines(_newvertices);
        
        Debug.Log(_newvertices);
        i = 0;
        foreach (var point in _newvertices)
        {
            
            print(_newvertices[i]);
            i++;
        }
    }
    void DrawLines(Vector3[] _a)
    {
        for (int i = 0; i < _a.Length; i++)
        {
            Vector3 start = _a[i];
            Vector3 end = _a[(i + 1) % _a.Length];
            Debug.DrawLine(start, end, Color.red, 100f);
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (vertices != null)
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(vertices[i], 0.05f);
            }
        }
    }

}
