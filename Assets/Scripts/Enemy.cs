using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드를 가져오기

// 적 AI를 구현한다
public class Enemy : LivingEntity {
    public LayerMask whatIsTarget; // 추적 대상 레이어

    private LivingEntity targetEntity; // 추적할 대상
    private NavMeshAgent pathFinder; // 경로계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격시 재생할 파티클 효과
    public AudioClip deathSound; // 사망시 재생할 소리
    public AudioClip hitSound; // 피격시 재생할 소리

    private Animator enemyAnimator; // 애니메이터 컴포넌트
    private AudioSource enemyAudioPlayer; // 오디오 소스 컴포넌트
    private Renderer enemyRenderer; // 렌더러 컴포넌트

    public float damage = 20f; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점

    // 프로퍼티 : 사실 함수인데 변수처럼 쓰게 해주는 개념
    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake() {
        // 초기화
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        enemyAudioPlayer = GetComponent<AudioSource>();

        enemyRenderer = GetComponentInChildren<Renderer>();
    }

    // 적 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newDamage, float newSpeed, Color skinColor) {
        startingHealth = newHealth;
        health = newHealth;

        damage = newDamage;
        // 이동속도 결정
        // 좀비를 움직이게 되는 건 결국 pathfinder
        pathFinder.speed = newSpeed;
        enemyRenderer.material.color = skinColor;
    }

    private void Start() {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션을 재생
        // start()에서 무한루프 코루틴을 시작했으므로 따로 update()를 쓰지않아도 된다.
        enemyAnimator.SetBool("HasTarget", hasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로를 갱신
    private IEnumerator UpdatePath() {
        // 살아있는 동안 무한 루프
        // 한번 타겟팅된 플레이어는 얘가 죽을때까지 따라다닐듯
        while (!dead)
        {
            if (hasTarget)
            {
                pathFinder.isStopped = false;
                pathFinder.SetDestination(targetEntity.transform.position);
            }
            else
            {
                // 추적 대상 발견못함
                pathFinder.isStopped = true;

                // 반경 20f 내의 whatistarget layer를 가진 콜라이더를 모두가져옴
                // 왜냐면 멀티플레이시 여러명이 될 수도 있기 때문이죵..
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                // 거기서 player 찾기(livingentity)
                for (int i = 0; i < colliders.Length; i++)
                {
                    LivingEntity livingentity = colliders[i].GetComponent<LivingEntity>();
                    if (livingentity != null && !livingentity.dead)
                    {
                        targetEntity = livingentity;
                        // target을 지정하면 바로 정지
                        break;
                    }
                }
            }
            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) {
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        // if문 안은 effect, 소리 담당 method
        if (!dead)
        {
            // hiteffect가 재생될 지점을 설정
            hitEffect.transform.position = hitPoint;
            // hiteffect의 회전정도를 설정
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();

            enemyAudioPlayer.PlayOneShot(hitSound);
        }

        // 효과 처리후 그다음 실제 damage처리 진행
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
    public override void Die() {
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        // base.die()의 ondeath()는 언제 설정할까?

        // enemy에는 콜라이더가 2개(box, capsule)
        Collider[] enemyColliders = GetComponents<Collider>();
        // component 비활성화 시에는 enabled 프로퍼티!
        for (int i = 0; i < enemyColliders.Length; i++)
            enemyColliders[i].enabled=false;

        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        enemyAnimator.SetTrigger("Die");
        enemyAudioPlayer.PlayOneShot(deathSound);
    }

    /// <summary>
    /// 트리거 충돌이 일어나는 동안 물리 갱신 주기에 맞춰 실행됨
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other) {
        // 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행   
        // 트리거 설정한 collider는 box collider밖에 없다. 그러므로 box collider가 여기서 쓰임.
        if(!dead && Time.time >= lastAttackTime + timeBetAttack)
        {
            // 추적 대상과 공격 대상을 구분짓는다. targetEntity!=attackTarget
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();

            if(attackTarget!=null && attackTarget == targetEntity)
            {
                lastAttackTime = Time.time;
                Vector3 hitpoint = other.ClosestPoint(transform.position);
                // hitnormal = other 입장에서 피격당한 면의 normal vector(방향)
                Vector3 hitnormal = transform.position - other.transform.position;

                attackTarget.OnDamage(damage, hitpoint, hitnormal);
            }

        }

    }
}