using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

public static class TrafficHQEditorGizmo
{
    // 화살표를 그립니다. 대각선 45도 방향 양쪽 2개.
    private static void DrawArrow(Vector3 point, Vector3 forward, float size)
    {
        forward = forward.normalized * size;
        Vector3 left = Quaternion.Euler(0, 45f, 0f) * forward;
        Vector3 right = Quaternion.Euler(0, -45f, 0f) * forward;

        Gizmos.DrawLine(point, point + left);
        Gizmos.DrawLine(point, point + right);
    }
    // 화살표 그림 타입에 따라 화살표 갯수를 얻어오겠습니다.
    private static int GetArrowCount(Vector3 pointA, Vector3 pointB, TrafficHeadquater headquater)
    {
        switch(headquater.arrowDrawType)
        {
            case TrafficHeadquater.ArrowDraw.FixedCount:
                return headquater.arrowCount;
            case TrafficHeadquater.ArrowDraw.ByLength:
                int count = (int)(Vector3.Distance(pointA, pointB) / headquater.arrowDistance);
                return Mathf.Max(1, count);
            case TrafficHeadquater.ArrowDraw.Off:
                return 0;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    // 선택되었을 때나 선택되지 않았을 때도 활성화 상태라면 기즈모를 그립니다.
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
    private static void DrawGizmo(TrafficHeadquater headquater, GizmoType gizmoType)
    {
        // 기즈모를 안그려야한다면 리턴.
        if (headquater.hideGizmos)
        {
            return;
        }

        foreach(TrafficSegment segment in headquater.segments)
        {
            // 세그먼트 이름 출력 (ex. Segment-0). 빨간색.
            GUIStyle style = new GUIStyle
            {
                normal =
                {
                    textColor = new Color(1, 0, 0)
                },
                fontSize = 15
            };
            Handles.Label(segment.transform.position, segment.name, style);
            // 웨이포인트 그리기.
            for(int j = 0; j < segment.wayPoints.Count; j++)
            {
                // 현재 웨이포인트 위치 찾고,
                Vector3 pos = segment.wayPoints[j].GetVisualPos();
                // 구 그리고, 방향을 표시하려면 색상을 변경.
                Gizmos.color = new Color(0, 0, ((j+1) / (float)segment.wayPoints.Count), 1f);
                Gizmos.DrawSphere(pos, headquater.waypointSize);
                // 다음 웨이포인트 위치 찾고,
                Vector3 pNext = Vector3.zero;
                // 다음 웨이포인트가 존재하면 가져오고.
                if (j < segment.wayPoints.Count -1 && segment.wayPoints[j + 1] != null)
                {
                    pNext = segment.wayPoints[j + 1].GetVisualPos();
                }
                // 다음 웨이포인트가 있다면.
                if (pNext != Vector3.zero)
                {
                    // 현재 웨이포인트를 추가중인 세그먼트라면, 주황색.
                    if (segment == headquater.curSegment)
                    {
                        Gizmos.color = new Color(1f, 0.3f, 0.1f);
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 0f, 0f);
                    }
                    // 선택한 구간은 녹색으로 그릴께요, 구별하기 쉽게.
                    if (Selection.activeObject == segment.gameObject)
                    {
                        Gizmos.color = Color.green;
                    }
                    // 두 웨이포인트의 연결선 그리기.
                    Gizmos.DrawLine(pos, pNext);
                    //arrowDrawType을 기반으로 화살표 그릴 갯수를 얻어와서.
                    int arrowDrawCount = GetArrowCount(pos, pNext, headquater);
                    // 화살표를 그려주자
                    for (int i = 1; i < arrowDrawCount + 1; i++)
                    {
                        Vector3 point = Vector3.Lerp(pos, pNext, (float)i / (arrowDrawCount + 1));
                        DrawArrow(point, pos - pNext, headquater.arrowSizeWaypoint);
                    }
                }
            }
            // 세그먼트를 연결하는 선 그리기.
            foreach(TrafficSegment nextSegment in segment.nextSegments)
            {
                if (nextSegment != null)
                {
                    Vector3 p1 = segment.wayPoints.Last().GetVisualPos();
                    Vector3 p2 = nextSegment.wayPoints.First().GetVisualPos();
                    // 노란색 선으로 그려줍니다.
                    Gizmos.color = new Color(1f, 1f, 0f);
                    Gizmos.DrawLine(p1, p2);
                    if (headquater.arrowDrawType != TrafficHeadquater.ArrowDraw.Off)
                    {
                        DrawArrow((p1 + p2) / 2f, p1 - p2, headquater.arrowSizeIntersection);
                    }
                }
            }
        }

    }
}
