using UnityEngine;
using UnityEditor;

public class VehicleSettingEditor : Editor
{
    /// <summary>
    /// �� �ݶ��̴��� �⺻ �������� �����ϴ� �޼���
    /// </summary>
    /// <param name="collider">������ �� �ݶ��̴�</param>
    private static void SetupWheelCollider(WheelCollider collider)
    {
        // �� �ݶ��̴� �⺻ �Ӽ� ����
        collider.mass = 20f;
        collider.radius = 0.175f;
        collider.wheelDampingRate = 0.25f;
        collider.suspensionDistance = 0.05f;
        collider.forceAppPointDistance = 0f;

        // ������� ������ ����
        JointSpring jointSpring = new JointSpring();
        jointSpring.spring = 70000f;
        jointSpring.damper = 3500f;
        jointSpring.targetPosition = 1f;
        collider.suspensionSpring = jointSpring;

        // ���� ������ ����
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
    /// Unity �޴��� ���� ���� ������ �߰�
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
    /// ����ĳ��Ʈ ��Ŀ�� ���� ��Ʈ�� ������Ʈ ����
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
    /// ���� �޽ÿ� �� �ݶ��̴� ����
    /// </summary>
    private static void SetupWheels(GameObject vehicle)
    {
        // ���� ��ġ ����
        string[] wheelNames = { "Tire BackLeft", "Tire BackRight", "Tire FrontLeft", "Tire FrontRight" };
        Transform[] tires = new Transform[4];
        GameObject[] wheels = new GameObject[4];

        // ���� �޽ÿ� �� �ݶ��̴� ����
        for (int i = 0; i < wheelNames.Length; i++)
        {
            // ���� �޽� ã��
            tires[i] = vehicle.transform.Find(wheelNames[i]);

            // �� �ݶ��̴� ����
            wheels[i] = EditorHelper.CreateGameObject($"{wheelNames[i]} Wheel", vehicle.transform);
            wheels[i].transform.position = tires[i].position;

            // �� �ݶ��̴� ����
            WheelCollider wheelCollider = EditorHelper.AddComponent<WheelCollider>(wheels[i]);
            SetupWheelCollider(wheelCollider);

            // ���� �޽ø� �� �ݶ��̴��� �ڽ����� ����
            tires[i].parent = wheels[i].transform;
            tires[i].localPosition = Vector3.zero;
        }

        // �� ����̺� ��Ʈ�� �߰�
        WheelDriverControl wheelDriverControl = EditorHelper.AddComponent<WheelDriverControl>(vehicle);
        wheelDriverControl.Init();
    }

    /// <summary>
    /// Rigidbody ���� ����
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
    /// ���� �ݶ��̴� ����
    /// </summary>
    private static void SetupColliders(GameObject vehicle)
    {
        // Ʈ���� �ݶ��̴�
        BoxCollider boxCollider = EditorHelper.AddComponent<BoxCollider>(vehicle);
        boxCollider.isTrigger = true;

        // ��ü �ݶ��̴� ����
        GameObject colliders = CreateChildWithResetTransform("Colliders", vehicle.transform);
        GameObject body = CreateChildWithResetTransform("Body", colliders.transform);

        BoxCollider bodyCollider = EditorHelper.AddComponent<BoxCollider>(body);
        bodyCollider.center = new Vector3(0f, 0.4f, 0f);
        bodyCollider.size = new Vector3(0.95f, 0.54f, 2.0f);
    }

    /// <summary>
    /// �⺻ Transform���� �ڽ� ������Ʈ ����
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
    /// ���� ���̾� ����
    /// </summary>
    private static void SetupVehicleLayer(GameObject vehicle)
    {
        EditorHelper.CreateLayer(TrafficHeadquater.VehicleTagLayer);
        vehicle.tag = TrafficHeadquater.VehicleTagLayer;
        EditorHelper.SetLayer(vehicle, LayerMask.NameToLayer(TrafficHeadquater.VehicleTagLayer), true);
    }
}