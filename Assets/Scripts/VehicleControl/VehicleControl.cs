using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using JetBrains.Annotations;

public class VehicleControl : MonoBehaviour
{
    private WheelDriverControl wheelDriverControl;
    private float initMaxSpeed = 0f;
    // 자동차가 이동할 타겟 데이터 구조체.
    public struct Target
    {
        public int segment;
        public int waypoint;
    }
    // 자동차의 상태.
    public enum Status
    {
        GO,
        STOP,
        SLOW_DOWN,
    }
    [Header("교통 관제 시스템.")] [Tooltip("현재 활성화된 교통 시스템.")]
    public TrafficHeadquater trafficHeadquarter;
    [Tooltip("차량이 목표에 도달한 시기를 확인합니다. 다음 웨이포인트를 더 일찍 예상하는데 사용할 수 있습니다. (이 숫자가 높을수록 더 빨리 예상됩니다.)")]
    public float waypointThresh = 2.5f;

    [Header("감지 레이더")] [Tooltip("레이를 쏠 앵커.")]
    public Transform raycastAnchor;
    [Tooltip("레이의 길이.")]
    public float raycastLength = 3f;
    [Tooltip("레이 사이의 간격.")]
    public float raycastSpacing = 3f;
    [Tooltip("생성될 레이의 수.")]
    public int raycastNumber = 8;
    [Tooltip("감지되는 차량이 이 거리 미만이면 정지합니다..")]
    public float emergentcyBrakeThresh = 1.5f;
    [Tooltip("감지되는 차량이 이 거리보다 낮거나 거리보다 높을 경우 자동차의 속도가 느려집니다..")]
    public float slowDownThresh = 5f;

    public Status vehicleStatus = Status.GO;
    private int pastTargetSegment = -1;
    private Target currentTarget;
    private Target nextTarget;

    void Start()
    {
        wheelDriverControl = GetComponent<WheelDriverControl>();
        initMaxSpeed = wheelDriverControl.maxSpeed;

        if (raycastAnchor == null && transform.Find("Raycast Anchor") != null)
        {
            raycastAnchor = transform.Find("Raycast Anchor");
        }
        //시작하면 내가 어디에 있는지, 어디로 가야하는지 한번 찾아봅니다.
        SetWaypointVegicleIsOn();
    }

    void Update()
    {
        /* 테스트 코드. 주석처리 합니다.
        float accelation = 5f;
        float brake = 0f;
        float steering = 0f;
        wheelDriverControl.maxSpeed = initMaxSpeed;
        wheelDriverControl.Move(accelation, steering, brake);
        */

        if (trafficHeadquarter == null)
        {
            return;
        }
        // 이동해야할 타겟 웨이포인트와 가까운지, 가깝다면 다음 웨이포인트 선정까지.
        WayPointChecker();
        // 자율 주행.
        MoveVehicle();
    }

    int GetNextSegmentID()
    {
        // hq가 들고 있는 구간 중에 현재 차량이 속해있는 세그먼트가 갖고 있는 다음 구간들을 얻어옵니다.
        List<TrafficSegment> nextSegments = trafficHeadquarter.segments[currentTarget.segment].nextSegments;
        if (nextSegments.Count == 0)
        {
            return 0;
        }

        int randomCount = Random.Range(0, nextSegments.Count - 1);
        return nextSegments[randomCount].ID;
    }
    // 앱이 실행되었을 때 현재 차량이 어느 구간에 어느 웨이포인트를 향해 가야하는지 스스로 판단.Start 함수에 호출.
    void SetWaypointVegicleIsOn()
    {
        foreach (var segment in trafficHeadquarter.segments)
        {
            // 현재 차가 이 구간 안에 있는지 확인.
            if (segment.IsOnSegment(transform.position))
            {
                currentTarget.segment = segment.ID;
                // 구간 내에서 시작할 가장 가까운 웨이포인트 찾기
                float minDist = float.MaxValue;
                List<TrafficWaypoint> waypoints = trafficHeadquarter.segments[currentTarget.segment].wayPoints;
                for (int j = 0; j < waypoints.Count; j++)
                {
                    float distance = Vector3.Distance(transform.position, waypoints[j].transform.position);

                    Vector3 lSpace = transform.InverseTransformPoint(waypoints[j].transform.position);
                    if (distance < minDist && lSpace.z > 0f)
                    {
                        minDist = distance;
                        currentTarget.waypoint = j;
                    }
                }
                break;
            }
        }
        // 다음 target 찾기.
        nextTarget.waypoint = currentTarget.waypoint + 1;
        nextTarget.segment = currentTarget.segment;
        // 위에 지정한 다음 타겟의 waypoint가 범위를 벗어났다면, 다시 처음 0번째 웨이포인트. 다음 세그먼트 아이디를 구합니다.
        if (nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].wayPoints.Count)
        {
            nextTarget.waypoint = 0;
            nextTarget.segment = GetNextSegmentID();
        }
    }
    // 다음 이동할 웨이포인트를 체크하여 타겟을 선정.
    void WayPointChecker()
    {
        GameObject waypoint = trafficHeadquarter.segments[currentTarget.segment].
            wayPoints[currentTarget.waypoint].gameObject;
        // 차량을 기준으로 한 다음 웨이포인트와의 위치를 찾기 위해 웨이포인트와의 거리 계산.
        Vector3 wpDist = transform.InverseTransformPoint(
            new Vector3(waypoint.transform.position.x, 
            transform.position.y, 
            waypoint.transform.position.z) );

        // 만약 현재 타겟으로 하고 있는 웨이포인트와 일정 거리 이하로 가깝다면.
        if(wpDist.magnitude < waypointThresh)
        {
            currentTarget.waypoint++;
            // 현재 구간이 갖고 있는 웨이포인트를 다 돌았다면 다음 구간의 웨이포인트로 타겟을 변경
            if(currentTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].wayPoints.Count)
            {
                pastTargetSegment = currentTarget.waypoint;
                currentTarget.segment = nextTarget.segment;
                currentTarget.waypoint = 0;
            }
            // 다음 타겟의 웨이포인트도 찾기.
            nextTarget.waypoint = currentTarget.waypoint + 1;
            if (nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].wayPoints.Count)
            {
                nextTarget.waypoint = 0;
                nextTarget.segment = GetNextSegmentID();
            }
        }
    }

    // 레이 캐스팅 함수. -> 충돌 에이어와 자동차 레이어만 레이 캐스팅합니다.
    void CastRay(Vector3 anchor, float angle, Vector3 dir, float length, out GameObject outObstacle, out float outHitDistance)
    {
        outObstacle = null;
        outHitDistance = -1;

        Debug.DrawRay(anchor, Quaternion.Euler(0f, angle, 0f) * dir * length, Color.red);
        // 일단 자동차 레이어만.
        int layer = 1 << LayerMask.NameToLayer(TrafficHeadquater.VehicleTagLayer);
        int finalMask = layer;

        // 추가 충돌체의 레이어가 있으면 추가.
        foreach (var layerName in trafficHeadquarter.collisionLayers)
        {
            int id = 1 << LayerMask.NameToLayer(layerName);
            finalMask = finalMask | id;
        }

        RaycastHit hit;
        if (Physics.Raycast(anchor, Quaternion.Euler(0f, angle, 0f) * dir, out hit, length, finalMask))
        {
            outObstacle = hit.collider.gameObject;
            outHitDistance = hit.distance;
            // Vector3 hitPosition = hit.point;
        }
    }

    // 레이캐스팅을 해서 충돌체를 얻어오고 거리도 얻어오는 함수.
    GameObject GetDetectObstacles(out float hitDist)
    {
        GameObject obstacleObject = null;
        float minDist = 10000f;
        float initRay = (raycastNumber / 2f) * raycastSpacing;
        float hitDistance = -1f;

        for (float a = -initRay; a <= initRay; a += raycastSpacing)
        {
            CastRay(raycastAnchor.transform.position, a, transform.forward,
                raycastLength, out obstacleObject, out hitDistance);

            if (obstacleObject == null)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, obstacleObject.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
            }
        }

        hitDist = hitDistance;
        return obstacleObject;
    }

    // 이 차량 구간(segment)을 얻어오는 함수.
    public int GetSegmentVehicleIsIn()
    {
        int vehicleSegment = currentTarget.segment;
        bool isOnSegment = trafficHeadquarter.segments[vehicleSegment].IsOnSegment(transform.position);
        if (isOnSegment == false)
        {
            bool isOnPastSegment = trafficHeadquarter.segments[pastTargetSegment].IsOnSegment(transform.position);
            if (isOnPastSegment)
            {
                vehicleSegment = pastTargetSegment;
            }
        }

        return vehicleSegment;
    }

    // 가급적 자율주행을 하고 싶습니다.
    void MoveVehicle()
    {
        // 기본적으로 풀 엑셀, 노 브레이크, 노 핸들링.
        float acc = 1f;
        float brake = 0f;
        float steering = 0;
        wheelDriverControl.maxSpeed = initMaxSpeed;
        if (currentTarget.segment >= trafficHeadquarter.segments.Count || 
            currentTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].wayPoints.Count)
        {
            Debug.LogError(currentTarget.ToString());
        }

        Transform targetTransform = trafficHeadquarter.segments[currentTarget.segment].wayPoints[currentTarget.waypoint].transform;
        Transform nextTargetTransform = trafficHeadquarter.segments[nextTarget.segment].wayPoints[nextTarget.waypoint].transform;
        Vector3 nextVector3 = nextTargetTransform.position - targetTransform.position;
        // 회전을 해야하는지 계산.
        float nextSteering = Mathf.Clamp(transform.InverseTransformDirection(nextVector3.normalized).x, -1, 1);
        // 만약 차가 서야 한다면.
        if (vehicleStatus == Status.STOP)
        {
            acc = 0f;
            brake = 1f;
            wheelDriverControl.maxSpeed = Mathf.Min(wheelDriverControl.maxSpeed / 2f, 5f);
        }
        else
        {
            // ----------------------------------------------- 밑으로는 다 이동관련.
            // 속도를 줄여야 하는 경우.
            if (vehicleStatus == Status.SLOW_DOWN) 
            {
                acc = 0.3f;
                brake = 0f;
            }
            // 회전을 해야 한다면 속도도 조절.
            if (nextSteering > 0.3f || nextSteering < -0.3f)
            {
                wheelDriverControl.maxSpeed = Mathf.Min(wheelDriverControl.maxSpeed, wheelDriverControl.steeringSpeedMax);
            }
            // 2. 레이캐스트로 감지된 장애물이 있는지 확인.
            float hitDist;
            GameObject obstacle = GetDetectObstacles(out hitDist);
            // 무언가 충돌이 되었다면.
            if (obstacle != null)
            {
                WheelDriverControl obstacleVehicle = null;
                obstacleVehicle = obstacle.GetComponent<WheelDriverControl>();
                // 자동차라면.
                if (obstacleVehicle != null )
                {
                    // 앞차인지부터 확인.
                    float dotFront = Vector3.Dot(transform.forward,
                        obstacleVehicle.transform.forward);
                    // 감지된 앞 차량의 속도가 내 차의 속도보다 낮으면 속도를 줄인다.
                    if (dotFront > 0.8f && obstacleVehicle.maxSpeed < wheelDriverControl.maxSpeed)
                    {
                        // 속도를 줄일 때, 아무리 작아도 0.1보다는 크게 조절.
                        float speed = Mathf.Max(wheelDriverControl.GetSpeedMS(obstacleVehicle.maxSpeed) - 0.5f, 0.1f);
                        wheelDriverControl.maxSpeed = wheelDriverControl.GetSpeedUnit(speed);
                    }
                    // 두 차량이 너무 가까우면서 같은 방향을 향하고 있으면 일단 멈춘다.
                    if (dotFront > 0.8f && hitDist < emergentcyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1f;
                        // 아무리 속도를 줄여도 최저속도까지만 줄입니다.
                        wheelDriverControl.maxSpeed = Mathf.Max(wheelDriverControl.maxSpeed / 2f, wheelDriverControl.minSpeed);
                    }
                    // 두 차량이 너무 가까우면서 같은 방향을 향하고 있지 않은 경우, 전면에서 달려오는 차와 부딪히게 생긴 경우.
                    else if (dotFront <= 0.8f && hitDist < emergentcyBrakeThresh)
                    {
                        acc = -0.3f;
                        brake = 0f;
                        wheelDriverControl.maxSpeed = Mathf.Max(wheelDriverControl.maxSpeed / 2f, wheelDriverControl.minSpeed);
                        // 가까이에 있는 차량이 오른쪽에 있을 수 있고 왼쪽에 있을 수 있기 때문에 그에 따라 회전도 하겠습니다.
                        float dotRight = Vector3.Dot(transform.forward, obstacleVehicle.transform.forward);
                        // 오른쪽.
                        if (dotRight > 0.1f)
                        {
                            steering = -0.3f;
                        }
                        // 왼쪽.
                        else if (dotRight < -0.1f)
                        {
                            steering = 0.3f;
                        }
                        // 가운데.
                        else
                        {
                            steering = -0.7f;
                        }
                    }
                    // 두 차량이 가까워지면 속도를 줄이자.
                    else if (hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
                // 장애물인 경우.
                else
                {
                    // 너무 가까우면 긴급 제동.
                    if (hitDist < emergentcyBrakeThresh) 
                    {
                        acc = 0f;
                        brake = 1f;
                        wheelDriverControl.maxSpeed = Mathf.Max(wheelDriverControl.maxSpeed / 2f, wheelDriverControl.minSpeed);
                    }
                    // 그렇지 않으면 일정 거리 이하로 가까워지면 천천히 이동.
                    else if (hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
            }

            // 경로를 따르도록 방향을 조정해야하는지 확인.
            if (acc > 0f)
            {
                Vector3 nextVector = trafficHeadquarter.segments[currentTarget.segment].wayPoints[currentTarget.waypoint].transform.position - transform.position;
                steering = Mathf.Clamp(transform.InverseTransformDirection(nextVector.normalized).x, -1, 1);
            }
        }
        // 차량 이동.
        wheelDriverControl.Move(acc, steering, brake);



    }









}

