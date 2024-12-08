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
    // �ڵ����� �̵��� Ÿ�� ������ ����ü.
    public struct Target
    {
        public int segment;
        public int waypoint;
    }
    // �ڵ����� ����.
    public enum Status
    {
        GO,
        STOP,
        SLOW_DOWN,
    }
    [Header("���� ���� �ý���.")] [Tooltip("���� Ȱ��ȭ�� ���� �ý���.")]
    public TrafficHeadquater trafficHeadquarter;
    [Tooltip("������ ��ǥ�� ������ �ñ⸦ Ȯ���մϴ�. ���� ��������Ʈ�� �� ���� �����ϴµ� ����� �� �ֽ��ϴ�. (�� ���ڰ� �������� �� ���� ����˴ϴ�.)")]
    public float waypointThresh = 2.5f;

    [Header("���� ���̴�")] [Tooltip("���̸� �� ��Ŀ.")]
    public Transform raycastAnchor;
    [Tooltip("������ ����.")]
    public float raycastLength = 3f;
    [Tooltip("���� ������ ����.")]
    public float raycastSpacing = 3f;
    [Tooltip("������ ������ ��.")]
    public int raycastNumber = 8;
    [Tooltip("�����Ǵ� ������ �� �Ÿ� �̸��̸� �����մϴ�..")]
    public float emergentcyBrakeThresh = 1.5f;
    [Tooltip("�����Ǵ� ������ �� �Ÿ����� ���ų� �Ÿ����� ���� ��� �ڵ����� �ӵ��� �������ϴ�..")]
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
        //�����ϸ� ���� ��� �ִ���, ���� �����ϴ��� �ѹ� ã�ƺ��ϴ�.
        SetWaypointVegicleIsOn();
    }

    void Update()
    {
        /* �׽�Ʈ �ڵ�. �ּ�ó�� �մϴ�.
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
        // �̵��ؾ��� Ÿ�� ��������Ʈ�� �������, �����ٸ� ���� ��������Ʈ ��������.
        WayPointChecker();
        // ���� ����.
        MoveVehicle();
    }

    int GetNextSegmentID()
    {
        // hq�� ��� �ִ� ���� �߿� ���� ������ �����ִ� ���׸�Ʈ�� ���� �ִ� ���� �������� ���ɴϴ�.
        List<TrafficSegment> nextSegments = trafficHeadquarter.segments[currentTarget.segment].nextSegments;
        if (nextSegments.Count == 0)
        {
            return 0;
        }

        int randomCount = Random.Range(0, nextSegments.Count - 1);
        return nextSegments[randomCount].ID;
    }
    // ���� ����Ǿ��� �� ���� ������ ��� ������ ��� ��������Ʈ�� ���� �����ϴ��� ������ �Ǵ�.Start �Լ��� ȣ��.
    void SetWaypointVegicleIsOn()
    {
        foreach (var segment in trafficHeadquarter.segments)
        {
            // ���� ���� �� ���� �ȿ� �ִ��� Ȯ��.
            if (segment.IsOnSegment(transform.position))
            {
                currentTarget.segment = segment.ID;
                // ���� ������ ������ ���� ����� ��������Ʈ ã��
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
        // ���� target ã��.
        nextTarget.waypoint = currentTarget.waypoint + 1;
        nextTarget.segment = currentTarget.segment;
        // ���� ������ ���� Ÿ���� waypoint�� ������ ����ٸ�, �ٽ� ó�� 0��° ��������Ʈ. ���� ���׸�Ʈ ���̵� ���մϴ�.
        if (nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].wayPoints.Count)
        {
            nextTarget.waypoint = 0;
            nextTarget.segment = GetNextSegmentID();
        }
    }
    // ���� �̵��� ��������Ʈ�� üũ�Ͽ� Ÿ���� ����.
    void WayPointChecker()
    {
        GameObject waypoint = trafficHeadquarter.segments[currentTarget.segment].
            wayPoints[currentTarget.waypoint].gameObject;
        // ������ �������� �� ���� ��������Ʈ���� ��ġ�� ã�� ���� ��������Ʈ���� �Ÿ� ���.
        Vector3 wpDist = transform.InverseTransformPoint(
            new Vector3(waypoint.transform.position.x, 
            transform.position.y, 
            waypoint.transform.position.z) );

        // ���� ���� Ÿ������ �ϰ� �ִ� ��������Ʈ�� ���� �Ÿ� ���Ϸ� �����ٸ�.
        if(wpDist.magnitude < waypointThresh)
        {
            currentTarget.waypoint++;
            // ���� ������ ���� �ִ� ��������Ʈ�� �� ���Ҵٸ� ���� ������ ��������Ʈ�� Ÿ���� ����
            if(currentTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].wayPoints.Count)
            {
                pastTargetSegment = currentTarget.waypoint;
                currentTarget.segment = nextTarget.segment;
                currentTarget.waypoint = 0;
            }
            // ���� Ÿ���� ��������Ʈ�� ã��.
            nextTarget.waypoint = currentTarget.waypoint + 1;
            if (nextTarget.waypoint >= trafficHeadquarter.segments[currentTarget.segment].wayPoints.Count)
            {
                nextTarget.waypoint = 0;
                nextTarget.segment = GetNextSegmentID();
            }
        }
    }

    // ���� ĳ���� �Լ�. -> �浹 ���̾�� �ڵ��� ���̾ ���� ĳ�����մϴ�.
    void CastRay(Vector3 anchor, float angle, Vector3 dir, float length, out GameObject outObstacle, out float outHitDistance)
    {
        outObstacle = null;
        outHitDistance = -1;

        Debug.DrawRay(anchor, Quaternion.Euler(0f, angle, 0f) * dir * length, Color.red);
        // �ϴ� �ڵ��� ���̾.
        int layer = 1 << LayerMask.NameToLayer(TrafficHeadquater.VehicleTagLayer);
        int finalMask = layer;

        // �߰� �浹ü�� ���̾ ������ �߰�.
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

    // ����ĳ������ �ؼ� �浹ü�� ������ �Ÿ��� ������ �Լ�.
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

    // �� ���� ����(segment)�� ������ �Լ�.
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

    // ������ ���������� �ϰ� �ͽ��ϴ�.
    void MoveVehicle()
    {
        // �⺻������ Ǯ ����, �� �극��ũ, �� �ڵ鸵.
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
        // ȸ���� �ؾ��ϴ��� ���.
        float nextSteering = Mathf.Clamp(transform.InverseTransformDirection(nextVector3.normalized).x, -1, 1);
        // ���� ���� ���� �Ѵٸ�.
        if (vehicleStatus == Status.STOP)
        {
            acc = 0f;
            brake = 1f;
            wheelDriverControl.maxSpeed = Mathf.Min(wheelDriverControl.maxSpeed / 2f, 5f);
        }
        else
        {
            // ----------------------------------------------- �����δ� �� �̵�����.
            // �ӵ��� �ٿ��� �ϴ� ���.
            if (vehicleStatus == Status.SLOW_DOWN) 
            {
                acc = 0.3f;
                brake = 0f;
            }
            // ȸ���� �ؾ� �Ѵٸ� �ӵ��� ����.
            if (nextSteering > 0.3f || nextSteering < -0.3f)
            {
                wheelDriverControl.maxSpeed = Mathf.Min(wheelDriverControl.maxSpeed, wheelDriverControl.steeringSpeedMax);
            }
            // 2. ����ĳ��Ʈ�� ������ ��ֹ��� �ִ��� Ȯ��.
            float hitDist;
            GameObject obstacle = GetDetectObstacles(out hitDist);
            // ���� �浹�� �Ǿ��ٸ�.
            if (obstacle != null)
            {
                WheelDriverControl obstacleVehicle = null;
                obstacleVehicle = obstacle.GetComponent<WheelDriverControl>();
                // �ڵ������.
                if (obstacleVehicle != null )
                {
                    // ������������ Ȯ��.
                    float dotFront = Vector3.Dot(transform.forward,
                        obstacleVehicle.transform.forward);
                    // ������ �� ������ �ӵ��� �� ���� �ӵ����� ������ �ӵ��� ���δ�.
                    if (dotFront > 0.8f && obstacleVehicle.maxSpeed < wheelDriverControl.maxSpeed)
                    {
                        // �ӵ��� ���� ��, �ƹ��� �۾Ƶ� 0.1���ٴ� ũ�� ����.
                        float speed = Mathf.Max(wheelDriverControl.GetSpeedMS(obstacleVehicle.maxSpeed) - 0.5f, 0.1f);
                        wheelDriverControl.maxSpeed = wheelDriverControl.GetSpeedUnit(speed);
                    }
                    // �� ������ �ʹ� �����鼭 ���� ������ ���ϰ� ������ �ϴ� �����.
                    if (dotFront > 0.8f && hitDist < emergentcyBrakeThresh)
                    {
                        acc = 0f;
                        brake = 1f;
                        // �ƹ��� �ӵ��� �ٿ��� �����ӵ������� ���Դϴ�.
                        wheelDriverControl.maxSpeed = Mathf.Max(wheelDriverControl.maxSpeed / 2f, wheelDriverControl.minSpeed);
                    }
                    // �� ������ �ʹ� �����鼭 ���� ������ ���ϰ� ���� ���� ���, ���鿡�� �޷����� ���� �ε����� ���� ���.
                    else if (dotFront <= 0.8f && hitDist < emergentcyBrakeThresh)
                    {
                        acc = -0.3f;
                        brake = 0f;
                        wheelDriverControl.maxSpeed = Mathf.Max(wheelDriverControl.maxSpeed / 2f, wheelDriverControl.minSpeed);
                        // �����̿� �ִ� ������ �����ʿ� ���� �� �ְ� ���ʿ� ���� �� �ֱ� ������ �׿� ���� ȸ���� �ϰڽ��ϴ�.
                        float dotRight = Vector3.Dot(transform.forward, obstacleVehicle.transform.forward);
                        // ������.
                        if (dotRight > 0.1f)
                        {
                            steering = -0.3f;
                        }
                        // ����.
                        else if (dotRight < -0.1f)
                        {
                            steering = 0.3f;
                        }
                        // ���.
                        else
                        {
                            steering = -0.7f;
                        }
                    }
                    // �� ������ ��������� �ӵ��� ������.
                    else if (hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
                // ��ֹ��� ���.
                else
                {
                    // �ʹ� ������ ��� ����.
                    if (hitDist < emergentcyBrakeThresh) 
                    {
                        acc = 0f;
                        brake = 1f;
                        wheelDriverControl.maxSpeed = Mathf.Max(wheelDriverControl.maxSpeed / 2f, wheelDriverControl.minSpeed);
                    }
                    // �׷��� ������ ���� �Ÿ� ���Ϸ� ��������� õõ�� �̵�.
                    else if (hitDist < slowDownThresh)
                    {
                        acc = 0.5f;
                        brake = 0f;
                    }
                }
            }

            // ��θ� �������� ������ �����ؾ��ϴ��� Ȯ��.
            if (acc > 0f)
            {
                Vector3 nextVector = trafficHeadquarter.segments[currentTarget.segment].wayPoints[currentTarget.waypoint].transform.position - transform.position;
                steering = Mathf.Clamp(transform.InverseTransformDirection(nextVector.normalized).x, -1, 1);
            }
        }
        // ���� �̵�.
        wheelDriverControl.Move(acc, steering, brake);



    }









}

