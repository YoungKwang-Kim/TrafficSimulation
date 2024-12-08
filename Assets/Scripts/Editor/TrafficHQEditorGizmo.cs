using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

public static class TrafficHQEditorGizmo
{
    // ȭ��ǥ�� �׸��ϴ�. �밢�� 45�� ���� ���� 2��.
    private static void DrawArrow(Vector3 point, Vector3 forward, float size)
    {
        forward = forward.normalized * size;
        Vector3 left = Quaternion.Euler(0, 45f, 0f) * forward;
        Vector3 right = Quaternion.Euler(0, -45f, 0f) * forward;

        Gizmos.DrawLine(point, point + left);
        Gizmos.DrawLine(point, point + right);
    }
    // ȭ��ǥ �׸� Ÿ�Կ� ���� ȭ��ǥ ������ �����ڽ��ϴ�.
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
    // ���õǾ��� ���� ���õ��� �ʾ��� ���� Ȱ��ȭ ���¶�� ����� �׸��ϴ�.
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
    private static void DrawGizmo(TrafficHeadquater headquater, GizmoType gizmoType)
    {
        // ����� �ȱ׷����Ѵٸ� ����.
        if (headquater.hideGizmos)
        {
            return;
        }

        foreach(TrafficSegment segment in headquater.segments)
        {
            // ���׸�Ʈ �̸� ��� (ex. Segment-0). ������.
            GUIStyle style = new GUIStyle
            {
                normal =
                {
                    textColor = new Color(1, 0, 0)
                },
                fontSize = 15
            };
            Handles.Label(segment.transform.position, segment.name, style);
            // ��������Ʈ �׸���.
            for(int j = 0; j < segment.wayPoints.Count; j++)
            {
                // ���� ��������Ʈ ��ġ ã��,
                Vector3 pos = segment.wayPoints[j].GetVisualPos();
                // �� �׸���, ������ ǥ���Ϸ��� ������ ����.
                Gizmos.color = new Color(0, 0, ((j+1) / (float)segment.wayPoints.Count), 1f);
                Gizmos.DrawSphere(pos, headquater.waypointSize);
                // ���� ��������Ʈ ��ġ ã��,
                Vector3 pNext = Vector3.zero;
                // ���� ��������Ʈ�� �����ϸ� ��������.
                if (j < segment.wayPoints.Count -1 && segment.wayPoints[j + 1] != null)
                {
                    pNext = segment.wayPoints[j + 1].GetVisualPos();
                }
                // ���� ��������Ʈ�� �ִٸ�.
                if (pNext != Vector3.zero)
                {
                    // ���� ��������Ʈ�� �߰����� ���׸�Ʈ���, ��Ȳ��.
                    if (segment == headquater.curSegment)
                    {
                        Gizmos.color = new Color(1f, 0.3f, 0.1f);
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 0f, 0f);
                    }
                    // ������ ������ ������� �׸�����, �����ϱ� ����.
                    if (Selection.activeObject == segment.gameObject)
                    {
                        Gizmos.color = Color.green;
                    }
                    // �� ��������Ʈ�� ���ἱ �׸���.
                    Gizmos.DrawLine(pos, pNext);
                    //arrowDrawType�� ������� ȭ��ǥ �׸� ������ ���ͼ�.
                    int arrowDrawCount = GetArrowCount(pos, pNext, headquater);
                    // ȭ��ǥ�� �׷�����
                    for (int i = 1; i < arrowDrawCount + 1; i++)
                    {
                        Vector3 point = Vector3.Lerp(pos, pNext, (float)i / (arrowDrawCount + 1));
                        DrawArrow(point, pos - pNext, headquater.arrowSizeWaypoint);
                    }
                }
            }
            // ���׸�Ʈ�� �����ϴ� �� �׸���.
            foreach(TrafficSegment nextSegment in segment.nextSegments)
            {
                if (nextSegment != null)
                {
                    Vector3 p1 = segment.wayPoints.Last().GetVisualPos();
                    Vector3 p2 = nextSegment.wayPoints.First().GetVisualPos();
                    // ����� ������ �׷��ݴϴ�.
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
