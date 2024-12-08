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
        // 타겟 없이는 동작하지 않습니다. (실무에서도 꼭 주석을 달아준다.)
        if (target == null)
        {
            return;
        }
        // cube transform
        Vector3 lhs = transform.forward;
        // target 으로 향하는 젝터, 크기를 노멀라이즈해서 방향만 얻습니다.
        Vector3 rhs = (target.position - transform.position).normalized;
        // 내적을 구합니다. 최대 1, 최소 -1.
        float dot = Mathf.Clamp(Vector3.Dot(lhs, rhs), -1, 1);
        // 타겟 포지션으로부터의 역벡터를 구합니다.
        Vector3 lineVector = transform.InverseTransformPoint(target.position);
        // 레이를 그려봅니다.타겟으로 향하는 레이, 큐브의 forward를 나타내는 레이.
        Debug.DrawRay(transform.position, lineVector, Color.red);
        Debug.DrawRay(transform.position, transform.position, Color.cyan);
        // 텍스트로 내적의 값을 출력합니다.
        // .ToString("F1")은 소수점 한자리까지 표현한다는 뜻.
        textLabel.text = dot.ToString("F1");
    }
}
