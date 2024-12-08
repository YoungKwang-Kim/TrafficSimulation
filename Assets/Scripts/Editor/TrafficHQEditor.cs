using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using NUnit;
using System.Linq;

[CustomEditor(typeof(TrafficHeadquater))]
public class TrafficHQEditor : Editor
{
    private TrafficHeadquater headquater;
    // ��������Ʈ ��ġ�� �� �ʿ��� �ӽ� ����ҵ�.
    private Vector3 startPosition;
    private Vector3 lastPoint;
    private TrafficWaypoint lastWaypoint;
    // traffic �ùķ������� ����� �Ǵ� ��ũ��Ʈ�� ����.
    [MenuItem("Component/TrafficTool/Create Traffic Object System")]
    private static void CreateTrafficSystem()
    {
        EditorHelper.SetUndoGroup("Create Traffic System");

        GameObject headquaterObject = EditorHelper.CreateGameObject("Traffic HeadQuarter");
        EditorHelper.AddComponent<TrafficHeadquater>(headquaterObject);

        GameObject segmentsObject = EditorHelper.CreateGameObject("Segments", headquaterObject.transform);
        GameObject intersectionsObject = EditorHelper.CreateGameObject("Intersections", headquaterObject.transform);


        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
    }

    private void OnEnable()
    {
        headquater = target as TrafficHeadquater;
    }

    // ��������Ʈ �߰�.
    private void AddWayPoint(Vector3 position)
    {
        // ��������Ʈ ���ӿ�����Ʈ�� ���� ����.
        GameObject go = EditorHelper.CreateGameObject("Waypoint-" + headquater.curSegment.wayPoints.Count, headquater.curSegment.transform);
        // ��ġ�� ���� Ŭ���� ������ �մϴ�.
        go.transform.position = position;
        TrafficWaypoint waypoint = EditorHelper.AddComponent<TrafficWaypoint>(go);
        waypoint.Refresh(headquater.curSegment.wayPoints.Count, headquater.curSegment);
        Undo.RecordObject(headquater.curSegment, "");
        // HQ�� ������ ��������Ʈ�� ���� �۾����� ���׸�Ʈ�� �߰��մϴ�.
        headquater.curSegment.wayPoints.Add(waypoint);
    }
    // ���׸�Ʈ �߰�.
    private void AddSegment(Vector3 position)
    {
        int segID = headquater.segments.Count;
        // Segments��� ���� �� ���ӿ�����Ʈ�� ���ϵ�� ���׸�Ʈ ���� ������Ʈ�� �����մϴ�.
        GameObject segGameObject = EditorHelper.CreateGameObject("Segment-" + segID, headquater.transform.GetChild(0).transform);
        // ���� ���� Ŭ���� ��ġ�� ���׸�Ʈ�� �̵���ŵ�ϴ�.
        segGameObject.transform.position = position;
        // HQ�� ���� �۾����� ���׸�Ʈ�� ���� ���� ���׸�Ʈ ��ũ��Ʈ�� �������ݴϴ�.
        // ���Ŀ� �߰��Ǵ� ��������Ʈ�� ���� �۾����� ���׸�Ʈ�� �߰��ǰ� �˴ϴ�.
        headquater.curSegment = EditorHelper.AddComponent<TrafficSegment>(segGameObject);
        headquater.curSegment.ID = segID;
        headquater.curSegment.wayPoints = new List<TrafficWaypoint>();
        headquater.curSegment.nextSegments = new List<TrafficSegment>();

        Undo.RecordObject(headquater, "");
        headquater.segments.Add(headquater.curSegment);
    }

    // ���ͼ��� �߰�.
    private void AddIntersection(Vector3 position)
    {
        int intID = headquater.intersections.Count;
        // ���ο� ������ ������ ���� Intersections ���� ������Ʈ ���ϵ�� �ٿ��ݴϴ�.
        GameObject intersection = EditorHelper.CreateGameObject("Intersection-" + intID, headquater.transform.GetChild(1).transform);
        intersection.transform.position = position;
        
        BoxCollider boxCollider = EditorHelper.AddComponent<BoxCollider>(intersection);
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(18, 2, 18);
        TrafficIntersection trafficIntersection = EditorHelper.AddComponent<TrafficIntersection>(intersection);
        trafficIntersection.ID = intID;

        Undo.RecordObject(headquater, "");
        headquater.intersections.Add(trafficIntersection);
    }

    // ������ ���� ��ġ�غ����� �ϰڽ��ϴ�.
    // Shift
    // Ctrl
    // Alt
    private void OnSceneGUI()
    {
        // ���콺 Ŭ�� ������ �־����� ���ɴϴ�.
        Event @event = Event.current;
        if (@event == null)
        {
            return;
        }
        // ���콺 ������ ��ġ�� ���̸� ������ݴϴ�.
        Ray ray = HandleUtility.GUIPointToWorldRay(@event.mousePosition);
        RaycastHit hit;
        // ���콺 ��ġ�� �浹ü ������ �Ǿ���, ���콺 ���� Ŭ������ ���� �߻��Ͽ���.
        // 0 ���� Ŭ�� 1 ������ Ŭ�� 2�� �ٹ�ư
        if (Physics.Raycast(ray, out hit) && 
            @event.type == EventType.MouseDown && 
            @event.button == 0)
        {
            // ���콺 ���� Ŭ�� + Shift -> ��������Ʈ �߰�.
            if (@event.shift)
            {
                // ���� ���� ��������Ʈ�� ������ �� �����ϴ�.
                if (headquater.curSegment == null)
                {
                    Debug.LogWarning("���׸�Ʈ ���� ������ּ���.");
                }
                EditorHelper.BeginUndoGroup("Add WayPoint", headquater);
                AddWayPoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            // ���콺 ���� Ŭ�� + Ctrl -> ���׸�Ʈ �߰�.
            else if (@event.control)
            {
                EditorHelper.BeginUndoGroup("Add Segment", headquater);
                AddSegment(hit.point);
                AddWayPoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            // ���콺 ���� Ŭ�� + Alt -> ���ͼ��� �߰�.
            else if (@event.alt)
            {
                EditorHelper.BeginUndoGroup("Add Intersection", headquater);
                AddIntersection(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }
        // ��������Ʈ �ý����� �����Ű�信�� ������ ���� ��ü�� ����.
        Selection.activeGameObject = headquater.gameObject;
        // ������ ��������Ʈ�� ó���մϴ�.
        if (lastWaypoint != null)
        {
            // ���̰� �浹�� �� �ֵ��� Plane�� ����մϴ�.
            Plane plane = new Plane(Vector3.up, lastWaypoint.GetVisualPos());
            plane.Raycast(ray, out float dst);
            Vector3 hitPoint = ray.GetPoint(dst);
            // ���콺 ��ư�� ó�� ������ ��, LastPoint �缳��.
            if (@event.type == EventType.MouseDown && @event.button == 0)
            {
                lastPoint = hitPoint;
                startPosition = lastWaypoint.transform.position;
            }
            // ������ ��������Ʈ�� �̵�
            if (@event.type == EventType.MouseDrag && @event.button == 0)
            {
                Vector3 realPos = new Vector3(
                    hitPoint.x - lastPoint.x, 0, hitPoint.z - lastPoint.z);
                lastWaypoint.transform.position += realPos;
                lastPoint = hitPoint;
            }
            // ������ ��������Ʈ�� ����.
            if(@event.type == EventType.MouseUp && @event.button == 0)
            {
                Vector3 curPos = lastWaypoint.transform.position;
                lastWaypoint.transform.position = startPosition;
                Undo.RegisterFullObjectHierarchyUndo(lastWaypoint, "Move WayPoint");
                lastWaypoint.transform.position = curPos;
            }
            // �� �ϳ� �׸��ڽ��ϴ�.
            Handles.SphereHandleCap(0, lastWaypoint.GetVisualPos(), Quaternion.identity, headquater.waypointSize * 2f, EventType.Repaint);
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            SceneView.RepaintAll();
        }
        // ��� ��������Ʈ�κ��� �Ǻ����� ���� �浹�Ǵ� ��������Ʈ�� lastWaypoint�� ����.
        if (lastWaypoint == null)
        {
            // ��� ��������Ʈ���� ���� ���� ���콺 ��ġ���� ������ ���̰� �浹�Ǵ��� Ȯ��.
            lastWaypoint = headquater.GetAllWaypoints().FirstOrDefault(
                i => EditorHelper.SphereHit(i.GetVisualPos(), 
                headquater.waypointSize, ray));
        }
        // HQ�� ���� �������� ���׸�Ʈ�� ���� ������ ���׸�Ʈ�� ��ü�մϴ�.
        if (lastWaypoint != null && @event.type == EventType.MouseDown)
        {
            headquater.curSegment = lastWaypoint.segment;
        }
        // ���� ��������Ʈ �缳��. ���콺�� �̵��ϸ� ������ Ǯ������.
        else if (lastWaypoint != null && @event.type == EventType.MouseMove)
        {
            lastWaypoint = null;
        }

    }
    // �ùķ����� ���� �������� �缳���� �ؼ� ������ ���� �� ���ư����� ���ݴϴ�.
    void RestructureSystem()
    {
        // ������ ��������Ʈ�� �̸� �ٲٱ�, ������ ����.
        List<TrafficSegment> segmentLists = new List<TrafficSegment>();
        int itSeg = 0;
        foreach (Transform trans in headquater.transform.GetChild(0).transform)
        {
            TrafficSegment segment = trans.GetComponent<TrafficSegment>();
            if (segment != null)
            {
                List<TrafficWaypoint> waypointList = new List<TrafficWaypoint>();
                segment.ID = itSeg;
                segment.gameObject.name = "Segment-" + itSeg;

                int itWay = 0;
                foreach (Transform trans2 in segment.transform)
                {
                    TrafficWaypoint waypoint = trans2.GetComponent<TrafficWaypoint>();
                    if (waypoint != null)
                    {
                        waypoint.Refresh(itWay, segment);
                        waypointList.Add(waypoint);
                        itWay++;
                    }
                }
                segment.wayPoints = waypointList;
                segmentLists.Add(segment);
                itSeg++;
            }
        }
        // ���׸�Ʈ�� ���������� ����Ʈ���� ������ ���� ���ְ�, ������.
        foreach (TrafficSegment segment in segmentLists)
        {
            List<TrafficSegment> nextSegmentsList = new List<TrafficSegment>();
            foreach (TrafficSegment nextSegment in segment.nextSegments)
            {
                if (nextSegment != null)
                {
                    nextSegmentsList.Add(nextSegment);
                }
            }

            segment.nextSegments = nextSegmentsList;
        }
        // ���θ��� ���׸�Ʈ ����Ʈ�� �ٽ� HQ�� �Ҵ��մϴ�.
        headquater.segments = segmentLists;
        // �����ε� ����������.
        List<TrafficIntersection> intersectionList = new List<TrafficIntersection>();
        int itInter = 0;
        foreach (Transform trasInter in headquater.transform.GetChild(1).transform)
        {
            TrafficIntersection intersection = trasInter.GetComponent<TrafficIntersection>();
            if (intersection != null)
            {
                intersection.ID = itInter;
                intersection.gameObject.name = "Intersection-" + itInter;
                intersectionList.Add(intersection);
                itInter++;
            }
        }

        headquater.intersections = intersectionList;

        // ����� �� ������ ���� ���� �׸��� ������ �� ����� ������.
        // ����Ȱ� �ִٰ� ����Ƽ���� �˷��ݴϴ�. ����Ȱ� ��� �ѹ� �׳� ���Ž����ݴϴ�.
        if (!EditorUtility.IsDirty(target))
        {
            EditorUtility.SetDirty(target);
        }
        Debug.Log("[���� �ùķ��̼� ���������� ������Ͽ����ϴ�.]");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        {
            Undo.RecordObject(headquater, "Traffic Inspector Edit");
            TrafficHQInspectorEditor.DrawInspector(headquater, serializedObject, out bool restructureSystem);
            if (restructureSystem)
            {
                RestructureSystem();
            }
        }
        // ���� ������ ��� ���� �ƿ� �ٽ� �ѹ� �׷�����.
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
