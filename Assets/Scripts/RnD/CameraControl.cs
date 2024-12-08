using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Transform myTransform = null;
    // 타겟으로부터의 떨어진 거리.
    public float distance = 5f;
    // 타겟으로부터의 높이.
    public float height = 1.5f; 
    // 높이값 변경 속도.
    public float heightDamping = 2.0f;
    // 회전값 변경 속도.
    public float roatationDamping = 3.0f;
    // 타겟.
    public Transform target = null;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = GetComponent<Transform>();
        // 타겟이 없다면 Player라는 태그를 가지고 있는 게임오브젝트가 타겟이다.
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        // 카메라가 목표로 하고 있는 회전의 Y축값과 높이값.
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;
        // 현재 카메라가 바라보고 있는 회전의 Y축값과 높이값.
        float currentRotationAngle = myTransform.eulerAngles.y;
        float currentHeight = myTransform.position.y;
        // 현재 카메라가 바라보고 있는 회전값과 높이값을 보간해서 새로운 값으로 계산.
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, roatationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
        // 위에서 계삱한 회전값으로 Quaternion 회전값을 생성.
        Quaternion currentRotation = Quaternion.Euler(0.0f, currentRotationAngle, 0.0f);
        // 카메라가 타겟의 위치에서 회전하고자 하는 벡터만큼 뒤로 이동한다.
        myTransform.position = target.position;
        myTransform.position -= currentRotation * Vector3.forward * distance;
        // 이동한 위치에서 원하는 높이값으로 올라간다.
        myTransform.position = new Vector3(myTransform.position.x, currentHeight, myTransform.position.z);
        // 타겟을 항상 바라보도록 한다. forward -> target
        myTransform.LookAt(target);
    }
}