using UnityEngine;
using UnityEditor;

public class VehicleSettingEditor : Editor
{
    /// <summary>
    /// 휠 콜라이더의 기본 설정값을 적용하는 메서드
    /// </summary>
    /// <param name="collider">설정할 휠 콜라이더</param>
    private static void SetupWheelCollider(WheelCollider collider)
    {
        // 휠 콜라이더 기본 속성 설정
        collider.mass = 20f;
        collider.radius = 0.175f;
        collider.wheelDampingRate = 0.25f;
        collider.suspensionDistance = 0.05f;
        collider.forceAppPointDistance = 0f;

        // 서스펜션 스프링 설정
        JointSpring jointSpring = new JointSpring();
        jointSpring.spring = 70000f;
        jointSpring.damper = 3500f;
        jointSpring.targetPosition = 1f;
        collider.suspensionSpring = jointSpring;

        // 바퀴 마찰력 설정
        WheelFrictionCurve frictionCurve = new WheelFrictionCurve();
        frictionCurve.extremumSlip = 1f;
        frictionCurve.extremumValue = 1f;
        frictionCurve.asymptoteSlip = 1f;
        frictionCurve.asymptoteValue = 1f;
        frictionCurve.stiffness = 1f;
        collider.forwardFriction = frictionCurve;
        collider.sidewaysFriction = frictionCurve;
    }

    /// <summary>
    /// Unity 메뉴에 차량 설정 도구를 추가
    /// </summary>
    [MenuItem("Component/TrafficTool/Setup Vehicle")]
    private static void SetupVehicle()
    {
        EditorHelper.SetUndoGroup("Setup Vehicle");
        GameObject selected = Selection.activeGameObject;
        PrefabUtility.UnpackPrefabInstance(selected, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        SetupAnchorAndControl(selected);
        SetupWheels(selected);
        SetupPhysics(selected);
        SetupColliders(selected);
        SetupVehicleLayer(selected);

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
    }

    /// <summary>
    /// 레이캐스트 앵커와 차량 컨트롤 컴포넌트 설정
    /// </summary>
    private static void SetupAnchorAndControl(GameObject vehicle)
    {
        GameObject anchor = EditorHelper.CreateGameObject("Raycast Anchor", vehicle.transform);
        anchor.transform.localPosition = new Vector3(0, 0.3f, 1);
        anchor.transform.localRotation = Quaternion.identity;

        VehicleControl vehicleControl = EditorHelper.AddComponent<VehicleControl>(vehicle);
        vehicleControl.raycastAnchor = anchor.transform;

        TrafficHeadquater headquater = FindObjectOfType<TrafficHeadquater>();
        if (headquater != null)
        {
            vehicleControl.trafficHeadquarter = headquater;
        }
    }

    /// <summary>
    /// 바퀴 메시와 휠 콜라이더 설정
    /// </summary>
    private static void SetupWheels(GameObject vehicle)
    {
        // 바퀴 위치 정의
        string[] wheelNames = { "Tire BackLeft", "Tire BackRight", "Tire FrontLeft", "Tire FrontRight" };
        Transform[] tires = new Transform[4];
        GameObject[] wheels = new GameObject[4];

        // 바퀴 메시와 휠 콜라이더 설정
        for (int i = 0; i < wheelNames.Length; i++)
        {
            // 바퀴 메시 찾기
            tires[i] = vehicle.transform.Find(wheelNames[i]);

            // 휠 콜라이더 생성
            wheels[i] = EditorHelper.CreateGameObject($"{wheelNames[i]} Wheel", vehicle.transform);
            wheels[i].transform.position = tires[i].position;

            // 휠 콜라이더 설정
            WheelCollider wheelCollider = EditorHelper.AddComponent<WheelCollider>(wheels[i]);
            SetupWheelCollider(wheelCollider);

            // 바퀴 메시를 휠 콜라이더의 자식으로 설정
            tires[i].parent = wheels[i].transform;
            tires[i].localPosition = Vector3.zero;
        }

        // 휠 드라이브 컨트롤 추가
        WheelDriverControl wheelDriverControl = EditorHelper.AddComponent<WheelDriverControl>(vehicle);
        wheelDriverControl.Init();
    }

    /// <summary>
    /// Rigidbody 물리 설정
    /// </summary>
    private static void SetupPhysics(GameObject vehicle)
    {
        Rigidbody rb = vehicle.GetComponent<Rigidbody>();
        rb.mass = 900f;
        rb.drag = 0.1f;
        rb.angularDrag = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// 차량 콜라이더 설정
    /// </summary>
    private static void SetupColliders(GameObject vehicle)
    {
        // 트리거 콜라이더
        BoxCollider boxCollider = EditorHelper.AddComponent<BoxCollider>(vehicle);
        boxCollider.isTrigger = true;

        // 본체 콜라이더 설정
        GameObject colliders = CreateChildWithResetTransform("Colliders", vehicle.transform);
        GameObject body = CreateChildWithResetTransform("Body", colliders.transform);

        BoxCollider bodyCollider = EditorHelper.AddComponent<BoxCollider>(body);
        bodyCollider.center = new Vector3(0f, 0.4f, 0f);
        bodyCollider.size = new Vector3(0.95f, 0.54f, 2.0f);
    }

    /// <summary>
    /// 기본 Transform으로 자식 오브젝트 생성
    /// </summary>
    private static GameObject CreateChildWithResetTransform(string name, Transform parent)
    {
        GameObject obj = EditorHelper.CreateGameObject(name, parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        return obj;
    }

    /// <summary>
    /// 차량 레이어 설정
    /// </summary>
    private static void SetupVehicleLayer(GameObject vehicle)
    {
        EditorHelper.CreateLayer(TrafficHeadquater.VehicleTagLayer);
        vehicle.tag = TrafficHeadquater.VehicleTagLayer;
        EditorHelper.SetLayer(vehicle, LayerMask.NameToLayer(TrafficHeadquater.VehicleTagLayer), true);
    }
}