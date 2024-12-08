using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public static class TrafficHQInspectorEditor
{
    public static void DrawInspector(TrafficHeadquater trafficHeadquater,SerializedObject serializedObject, out bool restructureSystem)
    {
        // ����� ����.
        InspectorHelper.Header("����� ����.");
        InspectorHelper.Toggle("����� ������?", ref trafficHeadquater.hideGizmos);
        // ȭ��ǥ ����.
        InspectorHelper.DrawArrowTypeSelection(trafficHeadquater);
        InspectorHelper.FloatField("��������Ʈ ũ��.", ref trafficHeadquater.waypointSize);
        EditorGUILayout.Space();
        // �ý��� ����.
        InspectorHelper.Header("�ý��� ����.");
        InspectorHelper.FloatField("���� ���� �ּ� �Ÿ�", ref trafficHeadquater.segDetectThresh);
        InspectorHelper.PropertyField("�浹 ���̾��, ", "collisionLayers", serializedObject);
        EditorGUILayout.Space();
        // ����.
        InspectorHelper.HelpBox("Ctrl + ���콺 ���� ��ư : ���׸�Ʈ ���� \n " + "Shift + ���콺 ���� ��ư : ��������Ʈ ���� \n" + 
            "Alt + ���콺 ���� ��ư : ������ ����");
        InspectorHelper.HelpBox("������ �߰��Ѵ�� ��������Ʈ�� ���� �̵��ϰ� �˴ϴ�.");

        EditorGUILayout.Space();
        restructureSystem = InspectorHelper.Button("���� �ùķ��̼� �ý��� �籸��.");
    }


}
