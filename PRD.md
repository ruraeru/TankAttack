# [인수인계서] 유니티 탱크 게임 프로젝트 개발 및 기능 확장

---

## 1. 프로젝트 인수인계 개요

* **프로젝트명:** Unity 탱크 게임 성능 확장 및 게임 루프 구축
* **작업 목적:** 기존 베이스 프로젝트(`TankExam.zip`) 분석을 바탕으로 씬 전환(Intro/Main/Ending), 포신 각도 제한, 포탄 및 적 AI 다양화, Wave 디펜스, 아이템 시스템 및 독자 기능 추가 요구사항 구현
* **인수인계 범위:** 기존 소스코드 구조 분석, 신규 씬/시스템 설계, 수정 및 신규 구현 스크립트 명세, 잔여 작업 체크리스트

---

## 2. 기존 베이스 소스코드 분석 현황

기존 예제 프로젝트에서 제공된 7개 스크립트의 기능과 향후 수정 방향입니다.

| 스크립트명 | 현재 주요 기능 및 동작 방식 | 비고 / 수정 방향 |
| --- | --- | --- |
| **`TurretCtrl.cs`** | 8번 레이어(Terrain)를 향한 Raycast 충돌점 기준, Local 좌표 변환(`InverseTransformPoint`) 후 터렛 Y축 회전

 | 기본 동작 유지 (회전 속도 및 조작감 보정) |
| **`TankMove.cs`** | `Input.GetAxis`("Horizontal", "Vertical") 기준 탱크 이동 및 회전, `Rigidbody.centerOfMass` 하향 설정

 | 아이템 획득 시 이동 속도(`moveSpeed`) 버프 연동

 |
| **`TankAnim.cs`** | `_MainTex` 및 `_BumpMap` 오프셋을 조절하여 궤도 스크롤 애니메이션 표현

 | 기본 동작 유지

 |
| **`SmoothFollow.cs`** | `target`을 기준으로 `distance`, `height`, Lerp 보정을 이용한 카메라 추적

 | 기본 동작 유지

 |
| **`FireCannon.cs`** | `Resources.Load`로 포탄/SFX 로드 후 마우스 좌클릭 시 `firePos`에서 발사

 | **포탄 종류 스위칭** 및 **발사 쿨타임** 로직 추가

 |
| **`CannonCtrl.cs`** | `Mouse ScrollWheel` 입력을 받아 포신 X축 회전

 | **각도 제한(`Mathf.Clamp`)** 로직 구현

 |
| **`Cannon.cs`** | `AddForce` 기반 직진, `OnTriggerEnter` 및 코루틴 기반 폭발 연출 및 이펙트 생성

 | **피해량(Damage) 판정** 및 범위 폭발 계산 추가

 |

---

## 3. 신규 씬 및 시스템 구조 설계

### 3.1 씬 (Scene) 구조 및 데이터 흐름

```
[IntroScene]
 ├── 게임 설명 / 조작법 안내 UI
 └── 게임 시작 버튼 (MainScene 전환)
        │
        ▼
[MainScene]
 ├── GameManager (게임 상태, HP, Wave 통제)
 ├── WaveManager (적 스폰 & 디펜스 진행)
 ├── Player Tank (각도 제한 포신, 포탄 교체, 아이템 버프)
 ├── Enemy Tanks (속도형/중장갑형 AI)
 └── Item Spawner (HP/Speed/Power 아이템)
        │
        ├─► [Player HP == 0] ───────────────► [Ending_Lose Scene]
        └─► [All Waves Cleared / Boss Dead] ──► [Ending_Win Scene]

```

---

## 4. 핵심 수정 및 신규 구현 코드 명세

### 4.1 포신 각도 제한 수정 (`CannonCtrl.cs`)

기존 무제한 회전 스크립트를 상하 각도 제한(`minAngle`, `maxAngle`)이 적용되도록 수정했습니다.

```csharp
using UnityEngine;

public class CannonCtrl : MonoBehaviour
{
    public float rotSpeed = 100.0f;[cite: 6]
    public float minAngle = -10.0f; // 포신 최소 각도 (지면 방향)
    public float maxAngle = 30.0f;  // 포신 최대 각도 (하늘 방향)

    private float currentAngle = 0.0f;

    void Update()
    {
        // 마우스 스크롤 입력을 통한 각도 변경[cite: 6]
        float scroll = Input.GetAxis("Mouse ScrollWheel");[cite: 6]
        currentAngle -= scroll * rotSpeed * Time.deltaTime * 100.0f;
        
        // 각도 범위 제한
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // X축 오프셋 회전 적용[cite: 6]
        transform.localRotation = Quaternion.Euler(currentAngle, 0, 0);
    }
}

```

---

### 4.2 게임 매니저 및 씬 관리 (`GameManager.cs`)

전체 게임 상태 관리, HP 관리, 씬 전환 및 승리/패배 판정을 담당합니다.

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerMaxHP = 100;
    public int playerCurrentHP;
    public int currentWave = 1;
    public int maxWave = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerCurrentHP = playerMaxHP;
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void TakeDamage(int damage)
    {
        playerCurrentHP -= damage;
        if (playerCurrentHP <= 0)
        {
            playerCurrentHP = 0;
            OnPlayerDead();
        }
    }

    public void OnPlayerDead()
    {
        ChangeScene("Ending_Lose");
    }

    public void OnWaveClear()
    {
        if (currentWave >= maxWave)
        {
            ChangeScene("Ending_Win");
        }
        else
        {
            currentWave++;
        }
    }
}

```

---

### 4.3 다변화 적 AI 탱크 (`EnemyAI.cs`)

NavMesh 기반 추적 및 사격을 수행하며, 스펙에 따라 2종류 이상의 적을 구성합니다.

```csharp
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { FastStriker, HeavyArtillery }

public class EnemyAI : MonoBehaviour
{
    public EnemyType enemyType;
    public float hp = 50f;
    public float attackRange = 15f;
    public float attackCooldown = 2f;
    public float damage = 10f;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        // 적 종류별 스펙 차별화
        if (enemyType == EnemyType.FastStriker)
        {
            agent.speed = 7.0f;
            hp = 40f;
            attackRange = 10f;
            damage = 8f;
        }
        else if (enemyType == EnemyType.HeavyArtillery)
        {
            agent.speed = 2.5f;
            hp = 120f;
            attackRange = 22f;
            damage = 25f;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist <= attackRange)
        {
            agent.isStopped = true;
            transform.LookAt(playerTarget);
            
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
    }

    void Attack()
    {
        GameManager.Instance.TakeDamage((int)damage);
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}

```

---

### 4.4 아이템 시스템 (`Item.cs`)

습득 시 체력 회복 및 이동 속도 증가 효과를 적용합니다.

```csharp
using UnityEngine;

public enum ItemType { Health, SpeedUp }

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public float value = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (itemType == ItemType.Health)
            {
                GameManager.Instance.playerCurrentHP = Mathf.Min(
                    GameManager.Instance.playerCurrentHP + (int)value, 
                    GameManager.Instance.playerMaxHP
                );
            }
            else if (itemType == ItemType.SpeedUp)
            {
                TankMove moveScript = other.GetComponent<TankMove>();
                if (moveScript != null)
                {
                    moveScript.moveSpeed += value;[cite: 2]
                }
            }
            Destroy(gameObject);
        }
    }
}

```

---

## 5. 잔여 개발 및 작업 체크리스트

| 구 분 | 단위 작업 내용 | 진행 상황 |
| --- | --- | --- |
| **씬 구성** | `IntroScene`, `Ending_Win`, `Ending_Lose` 씬 생성 및 UI 버튼 이벤트 연결 | 미완료 |
| **지형 & NavMesh** | `Terrain` 레이어(8번) 설정 및 `Navigation > Bake` 진행

 | 미완료 |
| **태그(Tag) 설정** | 플레이어 오브젝트에 `Player` 태그 적용 | 미완료 |
| **적 스폰 로직** | `WaveManager` 구현 및 Wave별 적 생성 위치/수량 제어 스크립트 작성 | 미완료 |
| **추가 기능 구현** | 독자 추가 기능 1종 (공습 레이저 지원 / 연막탄 스킬 중 선택) 작성 | 미완료 |
| **빌드 & 테스트** | 씬 간 루프 테스트(Intro ➔ Main ➔ Ending ➔ Replay) 및 단위 테스트 | 미완료 |
