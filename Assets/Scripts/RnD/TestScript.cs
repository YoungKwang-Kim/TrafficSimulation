using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestScript : MonoBehaviour
{
    public TextMeshProUGUI textLabel;
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        // Ÿ�� ���̴� �������� �ʽ��ϴ�. (�ǹ������� �� �ּ��� �޾��ش�.)
        if (target == null)
        {
            return;
        }
        // cube transform
        Vector3 lhs = transform.forward;
        // target ���� ���ϴ� ����, ũ�⸦ ��ֶ������ؼ� ���⸸ ����ϴ�.
        Vector3 rhs = (target.position - transform.position).normalized;
        // ������ ���մϴ�. �ִ� 1, �ּ� -1.
        float dot = Mathf.Clamp(Vector3.Dot(lhs, rhs), -1, 1);
        // Ÿ�� ���������κ����� �����͸� ���մϴ�.
        Vector3 lineVector = transform.InverseTransformPoint(target.position);
        // ���̸� �׷����ϴ�.Ÿ������ ���ϴ� ����, ť���� forward�� ��Ÿ���� ����.
        Debug.DrawRay(transform.position, lineVector, Color.red);
        Debug.DrawRay(transform.position, transform.position, Color.cyan);
        // �ؽ�Ʈ�� ������ ���� ����մϴ�.
        // .ToString("F1")�� �Ҽ��� ���ڸ����� ǥ���Ѵٴ� ��.
        textLabel.text = dot.ToString("F1");
    }
}
