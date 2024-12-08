using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSegment : MonoBehaviour
{
    // ������ �̵��� ����(segment)��.
    public List<TrafficSegment> nextSegments = new List<TrafficSegment>();
    // �� ���׸�Ʈ�� ID ��.
    public int ID = -1;
    // ������ ���� �ִ� ��������Ʈ�� ���� -> ���� �� ���� 2~3���� ������ �ִ�.
    public List<TrafficWaypoint> wayPoints = new List<TrafficWaypoint> ();

    public bool IsOnSegment(Vector3 pos)
    {
        TrafficHeadquater headquater = GetComponentInParent<TrafficHeadquater> ();

        for (int i = 0; i < wayPoints.Count - 1; i++)
        {
            Vector3 pos1 = wayPoints[i].transform.position;
            Vector3 pos2 = wayPoints[i + 1].transform.position;
            // ù��° ��������Ʈ�� ���� �Ÿ�.
            float d1 = Vector3.Distance(pos1, pos);
            // �ι�° ��������Ʈ�� ���� �Ÿ�
            float d2 = Vector3.Distance(pos2, pos);
            // ù��° ��������Ʈ�� �ι�° ��������Ʈ�� �Ÿ�.
            float d3 = Vector3.Distance(pos1, pos2);

            float diff = (d1 + d2) - d3;
            if (diff < headquater.segDetectThresh && diff > -headquater.segDetectThresh)
            {
                // �ڵ����� �� ��������Ʈ ���̿� ������ �ִ�.
                return true;
            }
        }
        // �ڵ����� �� ��������Ʈ ���̿� �ָ� �ִ�.
        return false;
    }
}
