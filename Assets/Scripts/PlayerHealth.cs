using UnityEngine;
using UnityEngine.UI; // UI 관련 코드

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerHealth : LivingEntity {
    public Slider healthSlider; // 체력을 표시할 UI 슬라이더

    public AudioClip deathClip; // 사망 소리
    public AudioClip hitClip; // 피격 소리
    public AudioClip itemPickupClip; // 아이템 습득 소리

    private AudioSource playerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    private void Awake() {
        // 사용할 컴포넌트를 가져오기
        playerAnimator = GetComponent<Animator>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();

    }

    protected override void OnEnable() {
        // 부활 시 관련 property 리셋기능 수행
        // LivingEntity의 OnEnable() 실행 (상태 초기화)
        // 여기서 health를 지정함
        base.OnEnable();

        healthSlider.gameObject.SetActive(true);
        // livingentity를 상속받아 그 class내 변수인 startinghealth를 바로 사용가능
        healthSlider.maxValue = startingHealth;
        healthSlider.value = health;

        // 플레이어 조작 가능한 컴포넌트 활성화
        // 부활을 염두에 둔 처리 >> scene 자체는 활성화된상태로 시작할것
        playerMovement.enabled = true;
        playerShooter.enabled = true;
    }

    // 체력 회복
    public override void RestoreHealth(float newHealth) {
        // LivingEntity의 RestoreHealth() 실행 (체력 증가)
        base.RestoreHealth(newHealth);

        healthSlider.value = health;

    }

    // 데미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        // 죽지 않았을 때만 타격음 재생
        if (!dead)
            playerAudioPlayer.PlayOneShot(hitClip);

        // LivingEntity의 OnDamage() 실행(데미지 적용)
        // 근데 죽은 상태에서 맞으면 value는 그만큼 또 줄어드는데 괜찮나?
        base.OnDamage(damage, hitPoint, hitDirection);
        healthSlider.value = health;
    }

    // 사망 처리
    public override void Die() {
        // LivingEntity의 Die() 실행(사망 적용)
        base.Die();

        healthSlider.gameObject.SetActive(false);

        playerAudioPlayer.PlayOneShot(deathClip);
        playerAnimator.SetTrigger("Die");

        // 플레이어 조작 컴포넌트 비활성화
        playerMovement.enabled = false;
        playerShooter.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        // 아이템과 충돌한 경우 해당 아이템을 사용하는 처리

        if (!dead)
        {
            // 충돌한 애가 item인지 검사
            IItem item = other.GetComponent<IItem>();
            if (item != null)
            {
                // item 효과를 player에게 적용
                item.Use(gameObject);
                playerAudioPlayer.PlayOneShot(itemPickupClip);
            }
        }
    }
}