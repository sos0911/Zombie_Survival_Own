using System.Collections.Generic;
using UnityEngine;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviour {
    public Enemy enemyPrefab; // 생성할 적 AI

    public Transform[] spawnPoints; // 적 AI를 소환할 위치들

    public float damageMax = 40f; // 최대 공격력
    public float damageMin = 20f; // 최소 공격력

    public float healthMax = 200f; // 최대 체력
    public float healthMin = 100f; // 최소 체력

    public float speedMax = 3f; // 최대 속도
    public float speedMin = 1f; // 최소 속도

    public Color strongEnemyColor = Color.red; // 강한 적 AI가 가지게 될 피부색

    private List<Enemy> enemies = new List<Enemy>(); // 생성된 적들을 담는 리스트
    private int wave; // 현재 웨이브
    // 웨이브도 자동 0으로 초기화되나?

    private void Update() {
        // 게임 오버 상태일때는 생성하지 않음
        if (GameManager.instance != null && GameManager.instance.isGameover)
        {
            return;
        }

        // 적을 모두 물리친 경우 다음 스폰 실행
        if (enemies.Count <= 0)
        {
            SpawnWave();
        }

        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 표시
    private void UpdateUI() {
        // 현재 웨이브와 남은 적의 수 표시
        UIManager.instance.UpdateWaveText(wave, enemies.Count);
    }

    // 현재 웨이브에 맞춰 적을 생성
    private void SpawnWave() {
        wave++;

        // wave가 커질수록 생성되는 적도 많아짐
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        // spawnCount번만큼 적 생성
        for (int i = 0; i < spawnCount; i++)
        {
            // enemyIntensity = 적의 강함 정도
            float enemyIntensity = Random.Range(0f, 1f);
            CreateEnemy(enemyIntensity);
        }
    }

    // 적을 생성하고 생성한 적에게 추적할 대상을 할당
    private void CreateEnemy(float intensity) {
        // intensity = 적의 강함 정도
        // lerp : 보간 함수
        float health = Mathf.Lerp(healthMin, healthMax, intensity);
        float damage= Mathf.Lerp(damageMin, damageMax, intensity);
        float speed= Mathf.Lerp(speedMin, speedMax, intensity);

        Color skincolor = Color.Lerp(Color.white, strongEnemyColor, intensity);

        // 0~spawnPoints.Length-1 index 중 하나 잡아서 랜덤장소스폰
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // rotation도 정중앙을 바라보게 세팅됨
        Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        enemy.Setup(health, damage, speed, skincolor);

        // 적 리스트에 추적 가능하게끔 추가함
        enemies.Add(enemy);

        // event에 추가한 함수 순서대로 실행됨
        // event를 활용함으로써 죽을 때 처리할 게 바뀌더라도 player나 enemy부분은 건들지 않아도 ok.
        // 중요! event list에는 입출력이 없는 함수만 추가가능해서 람다 함수를 먹여야 입출력이 있는 애들을 컨트롤가능
        // 원래 행하고 싶은 함수를 입출력 없는 익명함수로 감싼다고 생각하면 편함
        enemy.onDeath += () => enemies.Remove(enemy);
        // 죽은 뒤 10초후에 없어지는듯.
        // setactive(false)보다 이게 효율적일까?
        enemy.onDeath += () => Destroy(enemy.gameObject, 10f);
        enemy.onDeath += () => GameManager.instance.AddScore(100);

    }
}