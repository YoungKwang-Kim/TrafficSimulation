using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Transform myTransform = null;
    // Ÿ�����κ����� ������ �Ÿ�.
    public float distance = 5f;
    // Ÿ�����κ����� ����.
    public float height = 1.5f; 
    // ���̰� ���� �ӵ�.
    public float heightDamping = 2.0f;
    // ȸ���� ���� �ӵ�.
    public float roatationDamping = 3.0f;
    // Ÿ��.
    public Transform target = null;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = GetComponent<Transform>();
        // Ÿ���� ���ٸ� Player��� �±׸� ������ �ִ� ���ӿ�����Ʈ�� Ÿ���̴�.
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
        // ī�޶� ��ǥ�� �ϰ� �ִ� ȸ���� Y�ప�� ���̰�.
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;
        // ���� ī�޶� �ٶ󺸰� �ִ� ȸ���� Y�ప�� ���̰�.
        float currentRotationAngle = myTransform.eulerAngles.y;
        float currentHeight = myTransform.position.y;
        // ���� ī�޶� �ٶ󺸰� �ִ� ȸ������ ���̰��� �����ؼ� ���ο� ������ ���.
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, roatationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
        // ������ �蘬�� ȸ�������� Quaternion ȸ������ ����.
        Quaternion currentRotation = Quaternion.Euler(0.0f, currentRotationAngle, 0.0f);
        // ī�޶� Ÿ���� ��ġ���� ȸ���ϰ��� �ϴ� ���͸�ŭ �ڷ� �̵��Ѵ�.
        myTransform.position = target.position;
        myTransform.position -= currentRotation * Vector3.forward * distance;
        // �̵��� ��ġ���� ���ϴ� ���̰����� �ö󰣴�.
        myTransform.position = new Vector3(myTransform.position.x, currentHeight, myTransform.position.z);
        // Ÿ���� �׻� �ٶ󺸵��� �Ѵ�. forward -> target
        myTransform.LookAt(target);
    }
}