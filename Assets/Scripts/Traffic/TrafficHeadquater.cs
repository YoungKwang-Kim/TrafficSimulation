using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficHeadquater : MonoBehaviour
{
    // 세그먼트와 세그먼트 사이의 검출 간격.
    public float segDetectThresh = 0.1f;
    // 웨이포인트의 크기.
    public float waypointSize = 0.5f;
    // 충돌 레이어들.
    public string[] collisionLayers;

    public List<TrafficSegment> segments = new List<TrafficSegment>();
    public TrafficSegment curSegment;

    public const string VehicleTagLayer = "AutonomousVehicle"; // 자율주행차.
    // 교차로들.
    public List<TrafficIntersection> intersections = new List<TrafficIntersection>();
    
    // 에디터용, 기즈모 속성들. 본부에서 조절하겠습니다.
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

    // data loading -> 속성들 정의.
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
    // data 출력한 UI 라벨.
    public TMPro.TextMeshProUGUI stateLabel;
    // 구글 스프레드 시트 읽어올 로더.
    public SpreadSheedLoader dataLoader;
    // 읽어온 데이터 클래스.
    private TrafficData trafficData;

    private void Start()
    {
        /*
        dataLoader = GetComponent<SpreadSheedLoader>();
        stateLabel = GameObject.FindWithTag("TrafficLabel").GetComponent<TMPro.TextMeshProUGUI>();
        // 일정 주기로 데이터 로딩을 시킬껀데, 주의 너무 자주 빈번하게 부르면 URL 막힙니다.
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
        // data -> class 담을께요.
        trafficData = new TrafficData();
        
        string[] AllRow = loadedData.Split('\n'); // 줄바꿈으로 구분됨.
        foreach (string oneRow in AllRow)
        {
            string[] datas = oneRow.Split('\t'); // Tap키를 기준으로 구분됨
            EmergencyData data = new EmergencyData(datas[0], datas[1]);
            trafficData.datas.Add(data);
        }
        // data 검사도 합니다. 응급상황 발생시 세팅.
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
