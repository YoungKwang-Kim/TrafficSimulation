using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficHeadquater : MonoBehaviour
{
    // ���׸�Ʈ�� ���׸�Ʈ ������ ���� ����.
    public float segDetectThresh = 0.1f;
    // ��������Ʈ�� ũ��.
    public float waypointSize = 0.5f;
    // �浹 ���̾��.
    public string[] collisionLayers;

    public List<TrafficSegment> segments = new List<TrafficSegment>();
    public TrafficSegment curSegment;

    public const string VehicleTagLayer = "AutonomousVehicle"; // ����������.
    // �����ε�.
    public List<TrafficIntersection> intersections = new List<TrafficIntersection>();
    
    // �����Ϳ�, ����� �Ӽ���. ���ο��� �����ϰڽ��ϴ�.
    public enum ArrowDraw
    {
        FixedCount,
        ByLength,
        Off
    }

    public bool hideGizmos = false;
    public ArrowDraw arrowDrawType = ArrowDraw.ByLength;
    public int arrowCount = 1;
    public float arrowDistance = 5f;
    public float arrowSizeWaypoint = 1;
    public float arrowSizeIntersection = 0.5f;

    
    public List<TrafficWaypoint> GetAllWaypoints()
    {
        List<TrafficWaypoint> waypoints = new List<TrafficWaypoint>();
        foreach (var segment in segments)
        {
            waypoints.AddRange(segment.wayPoints);
        }
        return waypoints;
    }

    // data loading -> �Ӽ��� ����.
    [Serializable]
    public class EmergencyData
    {
        public int ID = -1;
        public bool IsEmergency = false;
        public EmergencyData(string id, string emergency)
        {
            ID = int.Parse(id);
            IsEmergency = emergency.Contains("1");
        }
    }

    public class TrafficData
    {
        public List<EmergencyData> datas = new List<EmergencyData>();
    }
    // data ����� UI ��.
    public TMPro.TextMeshProUGUI stateLabel;
    // ���� �������� ��Ʈ �о�� �δ�.
    public SpreadSheedLoader dataLoader;
    // �о�� ������ Ŭ����.
    private TrafficData trafficData;

    private void Start()
    {
        /*
        dataLoader = GetComponent<SpreadSheedLoader>();
        stateLabel = GameObject.FindWithTag("TrafficLabel").GetComponent<TMPro.TextMeshProUGUI>();
        // ���� �ֱ�� ������ �ε��� ��ų����, ���� �ʹ� ���� ����ϰ� �θ��� URL �����ϴ�.
        InvokeRepeating("CallLoaderAndCheck", 5f, 5f);
        */
    }
    private void CallLoaderAndCheck()
    {
        string loadedData = dataLoader.StartLoader();
        stateLabel.text = "Traffic Status\n" + loadedData;
        if (string.IsNullOrEmpty(loadedData))
        {
            return;
        }
        // data -> class ��������.
        trafficData = new TrafficData();
        
        string[] AllRow = loadedData.Split('\n'); // �ٹٲ����� ���е�.
        foreach (string oneRow in AllRow)
        {
            string[] datas = oneRow.Split('\t'); // TapŰ�� �������� ���е�
            EmergencyData data = new EmergencyData(datas[0], datas[1]);
            trafficData.datas.Add(data);
        }
        // data �˻絵 �մϴ�. ���޻�Ȳ �߻��� ����.
        CheckData();
        
    }

    private void CheckData()
    {
        for (int i = 0; i < trafficData.datas.Count; i++)
        {
            EmergencyData data = trafficData.datas[i];
            if (intersections.Count <= i || intersections[i] == null)
            {
                return;
            }

            if (data.IsEmergency)
            {
                intersections[data.ID].IntersectionType = IntersectionType.EMERGENCY;
            }
            else
            {
                intersections[data.ID].IntersectionType = IntersectionType.TRAFFIC_LIGHT;
            }
        }
    }
}
