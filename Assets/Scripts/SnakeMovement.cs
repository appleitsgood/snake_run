using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeMovement : MonoBehaviour
{

    public Transform head;
    public List<Transform> bodyParts;

    public float speed = 5f;

    private List<Vector3> positionHistory = new List<Vector3>(); // head가 지나간 위치들을 저장하는 리스트
    public int frameGap = 3;    // 프레임 업데이트 간격
    public int capacity = 60;   // positionHistory 메모리 크기


    void Start()
    {
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);

            if (child != head) { bodyParts.Add(child); }
        }
    }

    void FixedUpdate() {
        MoveHead();
        SaveHeadPosition();
        MoveBody();
    }

    void MoveHead() {

        // 카메라와 월드 평면 사이의 거리를 z값으로 넣어 평면 좌표로 변환
        Vector2 mousePos2d = Mouse.current.position.ReadValue();
        Vector3 mousePos3d = new Vector3(
            mousePos2d.x,
            mousePos2d.y,
            -Camera.main.transform.position.z
        ); 

        // 마우스 화면 좌표 → 월드 좌표로 변환
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos3d);
        targetPos.z = 0f;

        // 마우스 추적
        head.position = Vector2.MoveTowards(
            head.position, 
            targetPos, 
            speed * Time.deltaTime
        );

    }


    void SaveHeadPosition()
    {
        positionHistory.Insert(0, head.position); // head의 현재 위치를 가장 앞에 저장
        if (positionHistory.Count > capacity) { positionHistory.RemoveAt(positionHistory.Count - 1); }
    }

    void MoveBody()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            int historyIndex = (i + 1) * frameGap;
            if (historyIndex < positionHistory.Count) { bodyParts[i].position = positionHistory[historyIndex]; }
        }
    }

}
