using System.Collections;
using UnityEngine;

// 총을 구현한다
public class Gun : MonoBehaviour {
    // 총의 상태를 표현하는데 사용할 타입을 선언한다
    public enum State {
        Ready, // 발사 준비됨
        Empty, // 탄창이 빔
        Reloading // 재장전 중
    }

    public State state { get; private set; } // 현재 총의 상태

    public Transform fireTransform; // 총알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 총알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기
    public AudioClip shotClip; // 발사 소리
    public AudioClip reloadClip; // 재장전 소리

    public float damage = 25; // 공격력
    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄약
    public int magCapacity = 25; // 탄창 용량
    public int magAmmo; // 현재 탄창에 남아있는 탄약


    public float timeBetFire = 0.12f; // 총알 발사 간격
    public float reloadTime = 1.8f; // 재장전 소요 시간
    private float lastFireTime; // 총을 마지막으로 발사한 시점


    private void Awake() {
        // 사용할 컴포넌트들의 참조를 가져오기
        gunAudioPlayer = GetComponent<AudioSource>();
        // audiosource에 audioclip을 넣을 줄 알았더니 그건 아니다.
        bulletLineRenderer = GetComponent<LineRenderer>();

        // 사용할 점을 2개로 변경
        bulletLineRenderer.positionCount = 2;
        // component는 enabled, gameobject는 setactive()로 비활성화 유무 결정
        bulletLineRenderer.enabled = false;
    }

    private void OnEnable() {
        // 총 상태 초기화
        magAmmo = magCapacity;
        state = State.Ready;
        lastFireTime = 0;
    }

    // 발사 시도
    public void Fire() {
        if(state==State.Ready && Time.time >= lastFireTime + timeBetFire)
        {
            lastFireTime = Time.time;
            Shot();
        }
    }

    // 실제 발사 처리
    private void Shot() {
        RaycastHit hit;
        Vector3 hitposition = Vector3.zero;

        if(Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance))
        {
            // 뭔가 충돌되었으면 idamageable component를 가져오려고 시도해봄
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            // idamageable을 구현하는 것이라면 데미지를 줌
            if (target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }
            hitposition = hit.point;
        }
        else
        {
            // 충돌x >> 일단 hitposition을 총알궤적 끝부분으로 지정
            hitposition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        StartCoroutine(ShotEffect(hitposition));

        magAmmo--;
        if (magAmmo <= 0)
            // 탄창 빔
            state = State.Empty;

    }

    // 발사 이펙트와 소리를 재생하고 총알 궤적을 그린다
    private IEnumerator ShotEffect(Vector3 hitPosition) {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(shotClip);
        // 선 시작점 세팅
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        // 선 끝나는점 세팅
        bulletLineRenderer.SetPosition(1, hitPosition);

        // 라인 렌더러를 활성화하여 총알 궤적을 그린다
        bulletLineRenderer.enabled = true;

        // 0.03초 동안 잠시 처리를 대기
        // 코루틴의 처리가 잠시 대기중인거지 다른 코드는 작동가능
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성화하여 총알 궤적을 지운다
        bulletLineRenderer.enabled = false;
    }

    // 재장전 시도
    public bool Reload() {
        // 이미 재장전 중이거나 남은 탄알이 없거나 이미 탄창이 꽉찬경우 불가능
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity)
            return false;

        StartCoroutine(ReloadRoutine());
        return true;
    }

    // 실제 재장전 처리를 진행
    // 구현상 한번 재장전을 시작하면 멈출수없음
    private IEnumerator ReloadRoutine() {
        // 현재 상태를 재장전 중 상태로 전환
        state = State.Reloading;
        
        // playoneshot >> 이미 재생되는 clip이 있어도 소리중첩가능
        gunAudioPlayer.PlayOneShot(reloadClip);

        yield return new WaitForSeconds(reloadTime);

        int ammoToFill = magCapacity - magAmmo;

        // 남아있는걸로 꽉채울 수는 없을때 채울양 조절
        if (ammoRemain < ammoToFill)
            ammoToFill = ammoRemain;

        magAmmo += ammoToFill;
        ammoRemain -= ammoToFill;

        // 총의 현재 상태를 발사 준비된 상태로 변경
        state = State.Ready;
    }
}