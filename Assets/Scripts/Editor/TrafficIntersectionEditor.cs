using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficIntersection))]
public class TrafficIntersectionEditor : Editor
{
    private TrafficIntersection intersection;

    private void OnEnable()
    {
        intersection = target as TrafficIntersection;
    }

    public override void OnInspectorGUI()
    {
        intersection.IntersectionType = (IntersectionType)EditorGUILayout.EnumPopup("������ Ÿ��", intersection.IntersectionType);
        EditorGUI.BeginDisabledGroup(intersection.IntersectionType != IntersectionType.STOP);
        {
            InspectorHelper.Header("�켱 ���� ����.");
            InspectorHelper.PropertyField("�켱 ����", "prioritySegments", serializedObject);
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(intersection.IntersectionType != IntersectionType.TRAFFIC_LIGHT);
        {
            InspectorHelper.Header("��ȣ ������.");
            InspectorHelper.FloatField("��ȣ �ð� (��).", ref intersection.lightDuration);
            InspectorHelper.FloatField("��Ȳ�� �ð�(��).", ref intersection.orangeLightDuration);
            InspectorHelper.PropertyField("ù��° ���� �� �׷�1.", "lightGroup1", serializedObject);
            InspectorHelper.PropertyField("�ι�° ���� �� �׷�2.", "lightGroup2", serializedObject);
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUI.EndDisabledGroup();
    }


}