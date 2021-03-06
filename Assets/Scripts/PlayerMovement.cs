﻿using UnityEngine;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    public float rotateSpeed = 180f; // 좌우 회전 속도


    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터

    private void Start() {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate() {
        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행
        // update()와는 달리 프레임별이 아니라 일정 주기(ex.0.2초)로 실행되는 method
        // fixeddeltatime이 이 일정 주기를 나타냄
        Rotate();
        Move();

        // 애니메이터에 신호전달
        // playerinput의 move() 파라미터 호출 >> get
        playerAnimator.SetFloat("Move", playerInput.move);
    }

    // 입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move() {
        // -1~1 * 방향정규벡터 * speed * 주기
        // 한 프레임동안 이동할 거리와 방향
        Vector3 moveDistance = playerInput.move * transform.forward * moveSpeed * Time.deltaTime;
        
        // 인자는 전역 위치 벡터
        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance);
    }

    // 입력값에 따라 캐릭터를 좌우로 회전
    private void Rotate() {
        float turn = playerInput.rotate * rotateSpeed * Time.deltaTime;

        // 현재 회전상태에서 turn만큼 y축기반으로 더돌음
        // transform.rotation을 변경해도 되지만 물리처리를 무시하고 회전가능하므로 사용x
        playerRigidbody.rotation *= Quaternion.Euler(0, turn, 0f);
    }
}