# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

Unity 6 (`6000.5.10f1`) 탱크 슈터 게임. 게임 스크립트는 `Assets/02.Scripts/`에 있고 총 31개 파일, 약 2050줄이다. UI는 `com.unity.ugui` (legacy `UnityEngine.UI.Text`)를 쓴다. 입력은 legacy `Input` 클래스를 쓴다 (Input System 패키지 아님).

렌더 파이프라인은 **Built-in**이고 컬러 스페이스는 **Gamma**다. URP/HDRP 패키지는 넣지 않는다. 에셋은 Built-in 호환인 것만 쓴다. URP/HDRP 전용 셰이더나 셰이더 그래프는 분홍색으로 깨진다.

빌드/린트/테스트 CLI는 없다. 테스트 코드도 없다. 빌드와 실행은 Unity Editor에서 하고, 스크립트 컴파일 결과는 Editor 콘솔로 확인한다.

## Unity MCP

`.mcp.json`이 `unity-mcp` 서버를 연결한다. Editor가 열려 있을 때 `mcp__unity-mcp__*` 도구로 씬 조회/수정 (`Unity_RunCommand`), 콘솔 로그 (`Unity_GetConsoleLogs`), 캡처를 할 수 있다.

- `Unity_RunCommand` 코드에서 `Image`는 `UnityEngine.UI.Image`로, `Mesh`는 `UnityEngine.Mesh`로 풀네임을 써야 한다 (네임스페이스와 충돌해 CS0118).
- `Unity_RunCommand`에서 `System.Reflection`은 쓸 수 없다 (unauthorized namespace). `result.Log` 포맷에 `{0:P0}` 같은 서식 지정자도 안 먹는다. 값을 문자열로 만들어서 넘긴다.
- `AssetDatabase.DeleteAsset`을 호출하면 "User interactions are not supported for MCP tool calls" 오류가 난다. 파일은 쉘로 지우고, 씬 오브젝트는 `result.DestroyObject`를 쓴다.
- `GetComponent<T>() ?? AddComponent<T>()`는 Unity 오브젝트에서 null 검사를 건너뛴다. 명시적으로 `== null`을 검사한다.
- `.cs`를 수정한 직후에는 컴파일 때문에 "Unity not detected (no fresh discovery files found)" 오류가 난다. 컴파일이 끝날 때까지 기다렸다가 다시 호출한다.
- 프리팹 인스턴스의 자식은 `SetParent`로 옮길 수 없다. 먼저 `PrefabUtility.UnpackPrefabInstance`로 인스턴스의 프리팹 연결을 푼다.
- 에디터 모드에서 `Physics.simulationMode = SimulationMode.Script`로 바꾸고 `Physics.Simulate(dt)`를 돌리면 Play 없이 물리 이동을 검증할 수 있다. 끝나면 위치와 모드를 원래대로 되돌린다.
- `Unity_Camera_Capture`는 Scene View 카메라라서 Screen Space Overlay UI는 찍히지 않는다. UI 확인은 Game 뷰에서 한다. 편집 모드에서 `ParticleSystem.Simulate`한 파티클도 이 캡처에는 안 찍힌다. 파티클은 임시 `Camera`를 만들어 `RenderTexture`에 `cam.Render()` 한 뒤 PNG로 저장해서 확인한다 (저장은 `System.IO`가 된다).
- Unity 6.5에서는 `Object.GetInstanceID()` (CS0619), `FindFirstObjectByType`, `FindObjectsByType(FindObjectsSortMode.None)`이 막히거나 deprecated다. `FindAnyObjectByType`, `FindObjectsByType<T>()`를 쓴다.
- `ProjectSettings/NavMeshAreas.asset`의 에이전트 설정은 `NavMesh.GetSettingsByID(0)`가 구조체 복사본이라 값을 바꿔도 저장되지 않는다. `SerializedObject`로 `m_Settings`를 고친다.
- `RunCommand` 실행 중 `AssetDatabase.SaveAssets()`나 `Refresh()`가 구버전 `ProjectSettings/*.asset` 경고를 쏟아내서 응답이 `UNEXPECTED_ERROR`로 오지만, 로그에 `[Log]` 결과가 있으면 실행은 성공한 것이다.
- 코드로 PNG를 저장하고 `textureType = Sprite`만 지정하면 `spriteImportMode`가 `Multiple`로 남아 Sprite 하위 에셋이 생기지 않는다 (`LoadAssetAtPath<Sprite>`가 null). `spriteImportMode = SpriteImportMode.Single`도 같이 지정한다.
- 콘솔의 `Serialized files [version N] before 2019.1 are deprecated` 경고는 `ProjectSettings/*.asset`이 오래된 포맷이라 생기는 것이며 무시해도 된다.

## 구조

씬은 `Assets/01.Scenes/`의 `IntroScene` (메인 메뉴 + 탱크 전시 배경), `MainScene` (게임 씬), `Ending_Win`, `Ending_Lose` 네 개다. 예전 `MapTestScene` 내용을 `MainScene`으로 옮겼고 옛 `MainScene`(옛 탱크 구조)은 없어졌다. Build Settings 순서는 Intro, Main, Ending_Win, Ending_Lose다. 데이터 흐름은 컴포넌트가 서로를 직접 참조하는 단순한 구조이며, 여러 파일을 봐야 보이는 규칙은 다음과 같다.

- **Intro (`IntroScene`)**: 캔버스는 Scale With Screen Size 1920×1080이다. `IntroMenu` (Canvas)가 `MainMenu` (제목, 출격/조작법/설정/종료 버튼, 최고 기록), `ControlsPanel`, `SettingsPanel` (볼륨 슬라이더, 최고 기록 초기화), `TankSelectPanel`을 하나씩만 연다. ESC는 메인으로 돌아간다. 지형은 `MainScene`과 같은 `Assets/01.Scenes/MapTestTerrain.asset`을 공유한다 (위치 (-150, 0, -150), 머티리얼 템플릿 `Default-Terrain-Standard`를 직접 지정해야 한다. 비우면 분홍색이다). `Spot`의 높이는 중앙 지형 높이 (약 2.57)에 맞췄다 (카메라가 `Spot` 기준으로 돈다). 하늘은 `Cartoon Base BlueSky`다. 배경은 `Showcase` (`IntroShowcase`)가 고른 탱크 프리팹을 `Spot`에 세우고 조작 스크립트를 지운 뒤 포탑만 흔들고, 메인 카메라를 탱크 주위로 돌린다 (`screenShift`로 탱크를 화면 오른쪽에 둔다). 옛 Abrams 탱크와 카메라 `SmoothFollow`는 지웠다. 볼륨은 `GameSettings` (`PlayerPrefs` `MasterVolume`)가 게임 시작 때 `AudioListener.volume`에 적용한다. 버튼 아이콘은 `Game GUI Vol1/WoodenBlocks/PNG`를 올리브색으로 바꾼 `Assets/04.Images/UI/Mil_*.png`다 (색조 0.185, 채도 ×0.385, 명도 ×0.69, 채도 0.15 미만 픽셀은 그대로). Overlay UI 확인은 캔버스를 임시로 Screen Space Camera로 바꿔 `Camera.Render`로 PNG를 찍고, 씬을 저장하지 않고 다시 연다.
- **탱크 선택**: Intro의 출격 버튼이 `IntroMenu.OpenTankSelect` → `TankSelectUI.Open`으로 `TankSelectPanel`을 연다. 카드를 누르면 정보 패널 (이름, `TankInfo.description`, HP/속도/화력 막대, 포탄 목록)과 배경 전시 탱크가 바뀌고, 출격 버튼 (`TankSelectUI.StartGame`)이 선택을 저장하고 `MainScene`을 연다. 화력은 가장 센 포탄의 피해량이다. 카드는 `Assets/03.Prefabs/Tanks/TankCatalog.asset` (`TankCatalog`, ScriptableObject)의 탱크마다 `CardTemplate`을 복제해 실행 시 만든다. 고른 번호는 `TankSelection.Index` (`PlayerPrefs` `SelectedTank`)에 저장되고 `PlayerSpawner`가 같은 카탈로그에서 꺼내 생성한다. 시작 HP는 프리팹의 `TankInfo.maxHP`로 `GameManager.playerMaxHP`/`playerHP`를 덮어쓴다. 현재 0번 `Player_TANK` ("Mk.1 전차": HP 150, 속도 15, 포탄 4종), 1번 `Tanks/Player_Tank006` ("T-006 전차": HP 100, 속도 20, 포탄 3종)이다. 공습은 둘 다 같다. Mk.1의 4번 포탄 "미사일"은 `Assets/03.Prefabs/Shells/Shell_Missile.prefab` (`AirMissile` 복사본, 중력 없음, 직선 비행, 피해 80, 반경 7m, 쿨다운 3초, 이펙트 1.6배)이다. 포탄 선택 키는 `FireCannon.shells` 개수만큼 1~9가 동작하고, HUD `ShellBar`에는 버튼 4개가 있어 `PlayerHUD.Start`가 포탄 수만큼만 켜고 가운데 정렬한다. 포탄을 5종 이상으로 늘리면 HUD 버튼도 추가해야 한다.
  - 새 탱크 추가: 프리팹 루트에 `Rigidbody` (질량 2000, constraints 80), `BoxCollider`, `TankMove`, `FireCannon` (`firePos` 연결), `AudioSource`, `AirStrike`, `TankInfo`, 태그 `Player`를 두고, 포탑에 `TurretCtrl`, 포신에 `CannonCtrl`, 포구 `FirePos`, 루트 아래 `CamPivot`을 만든 뒤 카탈로그에 넣는다. 기존 프리팹 값은 `EditorUtility.CopySerialized`로 복사하면 된다. 카드 아이콘은 임시 카메라로 프리팹을 렌더링한 PNG (`Assets/04.Images/UI/Tank_*.png`)를 `TankInfo.icon`에 넣는다.
  - `Player_Tank006`: `ithappy` `Tank_006`을 풀어서 `Model`로 1.5배 키웠다 (포신 포함 길이 약 18m). `Tank_tower_006`에 `TurretCtrl`, `Tank_gun_006`에 `CannonCtrl`이 바로 붙어 있다 (모델 피벗이 회전 중심이라 피벗 오브젝트가 없다). 궤도 스크롤과 바퀴 회전은 없다.
- **플레이어 탱크 (`MainScene`)**: 씬에 미리 두지 않는다. `PlayerSpawner`가 게임 시작 때 (`Awake`) 카탈로그에서 고른 탱크 (기본 0번 `Assets/03.Prefabs/Player_TANK.prefab`)를 `AllyBase/Markers/PlayerSpawn`에 만들고 카메라 `SmoothFollow` 대상을 `CamPivot`으로 건다. 씬 오브젝트가 탱크를 직접 참조하면 안 되고 `Player` 태그로 찾는다 (`PlayerHUD`는 `Start`에서 찾는다). 프리팹의 루트 `Tank`에 `Rigidbody`, `BoxCollider`, `TankMove`, `FireCannon`, `AudioSource`가 붙고 태그는 `Player`다. 그 아래 `Tank/Tank` (프리팹 연결을 푼 모델)에 `Body`, `Chain`, `TurretPivot`이 있다. `TurretPivot`(`TurretCtrl`, 마우스 방향 포탑 회전) 아래에 `Head`와 `CannonPivot`(`CannonCtrl`, 마우스 휠로 포신 각도 -8~25도 제한)이 있고, `CannonPivot` 아래에 `Canon`과 `FirePos`(포구, `FireCannon.firePos`)가 있다. 카메라는 `SmoothFollow`로 `Tank/CamPivot`을 따라간다. 포탑과 포신이 같이 돌도록 `Head`와 `Canon`은 반드시 이 피벗들의 자식이어야 한다.
- **이동 (`TankMove`)**: 지형과의 물리 충돌을 끄고 (`Physics.IgnoreCollision`, 중력도 끔) 코드가 `Terrain.SampleHeight`로 지면 높이를 따라간다. 수평 속도는 평지든 경사든 항상 `moveSpeed`다. Shift를 누른 채 앞뒤로 움직이면 속도 배율 `Boost`가 `boostAccelTime`(1.2초) 동안 `boostMultiplier`(1.7배)까지 오르고, 떼면 `boostReleaseTime`(0.6초) 동안 1로 돌아온다. 아이템 Speed는 `moveSpeed`를 올리므로 가속 배율과 곱해진다. HUD의 `BoostBar` (Su57 아이콘 오른쪽, HP 바 복사본)가 가속 배율과 현재 수평 속도를 보여 준다 (`PlayerHUD.boostFill`, `boostText`). 지형 가장자리 `edgeMargin` 안쪽까지만 간다. 지형 충돌에 맡기면 박스 콜라이더가 울퉁불퉁한 지면에 걸쳐 뜨고, 능선에서 상승 속도로 날아가고, 경사 제한을 두면 경사 근처에서 멈춘다. 건물과 적과의 충돌은 그대로다.
- **차체 기울기 (`HullTilt`)**: 플레이어 탱크 프리팹 루트에 붙어 있다. 차체 앞뒤와 좌우 지면 높이를 샘플링해 보이는 모델 자식 (`Player_TANK`는 `Tank`, `Player_Tank006`은 `Model`)만 pitch/roll로 기울인다. 루트와 콜라이더, 카메라 피벗은 기울지 않는다 (Rigidbody X/Z 회전이 막혀 있고 `TankMove`가 Y 회전만 한다). 적 탱크에는 없다.
- **조준 레이저 (`AimLaser`)**: `PlayerSpawner.Awake`가 탱크를 만들 때 `AddComponent`로 붙인다 (프리팹에는 없다, 그래서 Intro 전시 탱크에는 안 나온다). `LateUpdate`에서 `FireCannon.firePos`부터 `firePos.forward` 직선으로 `LineRenderer`를 그리고 (`Physics.RaycastNonAlloc`, 자기 탱크와 트리거는 무시) 맞은 지점에서 끊고 작은 구(`hitDot`)를 둔다. 맞는 것이 없으면 `maxLength` 150m까지 간다. 중력으로 처지는 포탄의 곡선은 그리지 않는 직선이다. 밝은 하늘에서 Additive 셰이더는 허옇게 날아가서 `Sprites/Default` (정점 색)를 쓴다. 포탄이 가려지지 않도록 반투명하게 얇게 둔다: `color` (형광 분홍 (1, 0.2, 0.75), 알파 0.8. 초록 지형과 파란 하늘에서 구분되는 색), `width` (0.12), `hitDotSize` (0.3)는 공개 필드다. 시작점 `firePos`는 두 탱크 모두 포신 중심선 위다. 선과 점은 `Start`에서 한 번 만들고, 맞은 것이 없으면 점을 숨긴다. `Refresh()`는 없앴으므로 편집 모드에서는 그려지지 않는다 (Play에서 확인).
- **바퀴 (`WheelSpin`)**: `Player_Tank006`의 좌우 바퀴 8개씩을 `Rigidbody` 속도에 맞춰 굴린다. 제자리 회전 때는 좌우가 반대로 돈다. 궤도 메시 텍스처 스크롤은 없다.
- **웨이브 안내**: `WaveSpawner`가 웨이브 시작 때 `WAVE N` (마지막은 `최종 WAVE N`), 클리어 때 `WAVE N 클리어!`를 `WaveHUD.ShowBanner`로 띄운다. `HUDCanvas/WaveBanner` 텍스트가 2.5초 동안 나타났다 사라진다.
- **궤도 (`TrackScroll`)**: `Chain` 메시의 UV가 한 점에 몰려 있어서 텍스처 스크롤이 안 보인다. 그래서 `Assets/05.Models/ChainTread.asset`(UV를 탱크 앞뒤 방향으로 다시 투영한 복사본)과 `Assets/04.Images/TrackTread.mat`를 `Chain`에 씌우고, `TrackScroll`이 `Rigidbody` 속도에 맞춰 오프셋을 흘린다. 옛 `MainScene`용 `TankAnim`은 삭제했다.
- **포탄 발사**: `FireCannon`은 `Resources.Load("Cannon")`과 `Resources.Load<AudioClip>("CannonFire")`로 에셋을 이름으로 불러온다. 그래서 `Assets/Resources/Cannon.prefab`, `CannonFire.wav`의 이름과 위치를 바꾸면 안 된다. 포탄 프리팹은 `Cannon.cs`가 `Start`에서 `AddForce`로 날리고, 트리거 충돌이나 `lifetime` (기본 3초, `Invoke(Timeout)`) 후에 폭발 이펙트를 만든다. 중력으로 처지는 포탄의 낙하 높이 계산은 `Cannon.GravityDrop(speed, mass, dist)` 하나로 모았다 (`CannonCtrl` 자동 조준과 `EnemyAI.Attack`이 같이 쓴다).
  - 포탄 3종은 `FireCannon.shells` (`ShellType`)에 있다. 각 항목의 `shellPrefab`, `explosionPrefab`, `icon`이 비어 있으면 `Resources/Cannon`과 포탄 프리팹의 `expEffect`를 쓴다. 현재 `Assets/03.Prefabs/Shells/Shell_Normal/Fast/Heavy.prefab` (궤적 색, 두께, 충돌 반경만 다르다)과 `Assets/03.Prefabs/ExplosionFX/FX_Normal/Fast/Heavy.prefab`을 연결해 두었다.
  - 속도 6000~12000인 `AddForce`로 날리면 한 스텝에 최대 4.8m를 가서 지형을 뚫는다. `OnTriggerEnter`는 물리 스텝이 끝난 뒤에 불려서 포탄이 이미 지형 속에 있다. 그래서 `Cannon.cs`는 `FixedUpdate`와 `OnTriggerEnter` 양쪽에서 이전 위치부터 현재 위치까지 `Physics.Raycast`로 표면 지점을 찾고, 폭발은 `Explode(point, normal)` 하나로 모은다. 이펙트는 표면에서 `effectLift`(1m)만큼 띄워서 생성한다. 표면에 놓으면 이펙트 아래 절반이 지형에 가린다. `Player` 태그 충돌체는 레이에서 무시한다.
  - 폭발 이펙트 수명은 3초다 (`Destroy(obj, 3.0f)`). 더 길게 하려면 이 값도 늘린다.
- **공습 (`AirStrike`)**: `Player_TANK`에 붙어 있다. G를 누르면 마우스가 가리키는 `Terrain` 지점 위로 `Su57`이 카메라 오른쪽 방향으로 지나가며 `AirMissile.prefab` (`Shell_Heavy` 복사본 + `Missle` 모델)을 8발 떨어뜨린다. 미사일은 `Cannon`을 재사용해서 지형 충돌과 폭발과 적 피해를 처리하고, 아래로 `speed`만큼 밀어서 3초 안에 떨어지게 하지만 안전하게 `Cannon.lifetime`을 6초로 둔다. 쿨다운 25초이고 왼쪽 아래 HP 아이콘 위의 `AirStrikeIcon`(`Assets/04.Images/UI/Su57Icon.png`, Su57을 위에서 렌더링한 PNG)이 아래에서 차오르며 표시하고 중앙에 남은 초 (준비되면 `G`)가 나온다. 미사일은 모델 3배, 폭발 반경 16m, 폭발 이펙트 2.5배 (`Cannon.effectScale`)다. `AirStrike` 값은 `Player_TANK` 프리팹에 저장되므로 코드 기본값만 바꾸면 반영되지 않는다. `Su57`은 +z가 기수이고 모델이 피벗보다 3.9m 위에 그려져서 `planeHeightOffset`으로 보정한다. 수치는 모두 임의 값이다.
- **대공 (헬기 상대)**: `EnemyAI.isAir`인 적에게만 적용된다.
  - 근접 신관: 플레이어 포탄 (`fromEnemy` false)은 매 물리 스텝마다 지나간 구간 둘레 `Cannon.proximityRadius`(6m) 안에 공중 적이 있으면 그 자리에서 터지고, 폭발 반경과 상관없이 그 적에게 직접 피해를 준다 (`TryProximity`, 같은 적은 폭발 반경 피해에서 빼서 두 번 맞지 않는다).
  - 자동 조준: `TurretCtrl`이 마우스 레이 둘레 `airAimRadius`(4m) 안의 공중 적을 찾으면 포탑을 그쪽으로 돌리고 `CannonCtrl.SetAutoTarget`으로 포신 각도를 자동으로 맞춘다 (현재 포탄이 중력을 받으면 `Cannon.GravityDrop`만큼 높여 잡는다). 헬기 조준과 지형 조준은 같은 `TurretCtrl.RotateToward`로 돈다. 공중 적을 벗어나면 휠 조작으로 돌아간다. 플레이어 탱크 포신 최대각은 45°다 (두 프리팹의 `CannonCtrl.maxAngle`).
- **레이어**: `TurretCtrl`은 레이어 8 (`Terrain`)에만 레이캐스트한다 (`1 << 8`). 지형 레이어를 바꾸면 포탑 조준이 멈춘다.
- **태그**: `Player` 태그를 `EnemyAI`와 `Item`이 사용한다. 플레이어 `Tank`에 이 태그가 있어야 한다.
- **GameManager**: `GameManager.instance` 싱글턴이 `playerHP`, 웨이브 진행, 씬 전환을 가진다. 씬 전환은 이름 문자열로 한다. `MainScene`에도 `GameManager` 오브젝트를 넣었다.
- **엔딩 (`Ending_Win`, `Ending_Lose`)**: 씬마다 `EndingUI` (`win` 플래그) 오브젝트 하나뿐이고 화면은 `Build()`가 `Start`에서 코드로 만든다. 가운데 올리브 패널 (1100×760, 위아래 강조선), 헤더 아이콘, 제목 (88), 부제, 점수 (64, 0에서 올라가는 연출), 최고 기록 (40), `NEW RECORD!` (깜빡임), 버튼 둘 (다시 하기 → `MainScene`, 처음으로 → `IntroScene`)이다. 아이콘은 `headerIcon` (승리 `Mil_Trophy`, 패배 `Mil_Cancel`), `retryIcon` (`Mil_Restart`), `homeIcon` (`Mil_Home`) 필드에 씬에서 직접 연결한다. 하늘은 `skybox` 필드 (승리 `Epic_BlueSunset`, 패배 `Deep Dusk`)를 `RenderSettings.skybox`에 넣고 카메라를 Skybox 모드로 쓴다 (비우면 단색). 하늘 위에 어두운 덮개는 깔지 않는다 (가운데 패널만 어둡다). 점수는 `ScoreBoard` (`PlayerPrefs` `BestScore`)에서 읽는다. 편집 모드에서 `Build()`를 불러 캔버스를 Screen Space Camera로 바꾸고 `CanvasGroup.alpha`를 1로 둔 뒤 `Camera.Render`로 PNG를 찍어 레이아웃을 확인했다 (씬은 저장하지 않고 다시 연다).
- **NavMesh (`MainScene`)**: `com.unity.ai.navigation` 2.0.16 패키지의 `NavMeshSurface`가 `NavMesh` 오브젝트에 붙어 있고, 데이터는 `Assets/01.Scenes/MainScene/NavMesh-NavMesh.asset`이다. 산이 가팔라서 에이전트 최대 경사를 기본 45°에서 60°로 올렸다 (`ProjectSettings/NavMeshAreas.asset`). 올린 뒤에야 `EnemySpawn_1~4`에서 `PlayerSpawn`까지 `PathComplete`가 나온다. 지형이나 건물을 바꾸면 `NavMeshSurface.BuildNavMesh()`로 다시 굽는다.
- **HUD (`MainScene`)**: `HUDCanvas` 하나에 `PlayerHUD` (HP 바, 포탄 버튼 3개와 쿨다운, 탄 정보 패널), `WaveHUD` (웨이브, 적 수), `PauseMenu` (ESC, 우상단 `PauseButton`)가 붙어 있다. 레퍼런스 해상도 1920×1080이다. `PauseMenu`는 `Time.timeScale`을 0으로 만들고 씬을 나갈 때 1로 되돌린다. 팩에 패널과 바 틀이 없어서 틀은 Unity 기본 `UISprite`를 올리브 색으로 칠했고, 아이콘만 팩에서 가져왔다.
- **폭발 이펙트 (`Assets/03.Prefabs/ExplosionFX`)**: Particle Pack의 `SmallExplosion`, `TinyExplosion`, `DustExplosion`을 Built-in용으로 바꾼 독립 복사본이다. 원본 머티리얼이 URP 셰이더라서 그대로는 Built-in에서 깨진다. 머티리얼은 `Legacy Shaders/Particles/Additive`와 `Alpha Blended`로 바꿨고 (`Materials/Mil_*.mat`), 쓰는 텍스처는 `Textures/`로 복사했다. 불꽃은 `ExplosionEmission` 글로우 맵을 쓰고, 불덩이 파티클의 색 곡선이 검정이던 것을 노랑에서 주황으로 바꿨다 (검정에 Additive를 쓰면 안 보인다). `FX_Heavy`에는 `FX_Normal` 복사본 `FireCore`가 들어 있고, `sortingFudge`로 불꽃을 먼지 앞에 그린다. 왜곡(distortion) 이펙트는 뺐다.
- **하늘**: `MainScene`은 `Epic_BlueSunset` 스카이박스를 쓴다 (`Assets/AllSkyFree`, Built-in `Skybox/6 Sided`, `Mobile/Skybox`). 스카이박스를 바꾸면 `DynamicGI.UpdateEnvironment()`로 앰비언트를 갱신한다.
- **지형 (`MainScene`)**: `ArenaTerrain`은 300m × 300m × 높이 50m, 레이어 8이다. 데이터는 `Assets/01.Scenes/MapTestTerrain.asset`이고, 노이즈로 평지와 산을 섞어 코드로 만들었다 (생성 스크립트는 저장하지 않았다). TerrainLayer는 `TerrainSampleAssets/TerrainLayers`의 `Grass_A`, `Grass_Soil`, `Rock`, `Sand`, `Muddy`, `Pebbles_A` 6개를 쓴다. 지형을 바꾼 뒤에는 지형 충돌체 데이터를 다시 할당한다. 평지는 약 25%이고 산, 구릉과 섞여 있다. 중앙 30m는 항상 평지, 가장자리 30m는 항상 산이다.
- **웨이브 스포너 (`MainScene`)**: `WaveSpawner`가 `enemies` (`EnemyEntry`: 프리팹, 등장 웨이브 `minWave`, 가중치 `weight`)와 `EnemySpawn_1~4`를 참조한다. 시작 5초 뒤 1웨이브가 나오고, 스폰 지점을 돌아가며 1.5초 간격으로 생성한다 (NavMesh 위로 `SamplePosition`). 웨이브는 5개 (`GameManager.maxWave`)이고 적 수는 `3 + 2 × (웨이브 - 1)`이다. 그 웨이브에 처음 열린 종류는 1대씩 먼저 넣고 나머지는 열린 종류 중 가중치로 뽑아 섞는다. 등장: 1웨이브 Normal, 2웨이브 Speed, 3웨이브 Truck과 Artillery, 4웨이브 Heavy, 5웨이브 Heli. 웨이브 적이 전멸하면 `GameManager.OnWaveClear()`를 부르고 5초 쉰다. 마지막 웨이브를 깨면 `Ending_Win`으로 간다.
- **적 (`EnemyAI`, `Assets/03.Prefabs/Enemies/Enemy_*.prefab`)**: 능력치는 모두 프리팹 값이다 (`moveSpeed`, `hp`, `attackRange`, `attackCooldown`, `damage`, `scoreValue`, `shellPrefab`). 공격 방식 `attackMode`는 세 가지다.
  - `Shell`: 사거리 안에서 멈추고 `FirePos`에서 포탄을 쏜다. 포탄이 중력으로 처지는 만큼 위로 보정한다 (`Cannon.GravityDrop`, 움직이는 플레이어 앞서 쏘기는 없다). 발사 가능 검사와 포탄 생성은 `CanShoot`/`SpawnShell`로 `Attack`과 `FireArtillery`가 같이 쓴다. 시야 체크는 없다.
  - `Ram`: 멈추지 않고 돌진하다가 플레이어 콜라이더 표면까지 `explodeRange` 안이면 반경 `explodeRadius`에 `damage`를 주고 자폭한다 (점수 없음).
  - `Artillery`: 사거리 안에서 멈추고 발사각 50°로 플레이어 발밑 지면에 맞는 초속을 계산해 포탄 `Rigidbody.linearVelocity`에 넣는다 (`Cannon.speed = 0`). 착탄 지점에 `ArtilleryMarker` 원판을 비행 시간 동안 띄운다. 편집 모드 시뮬레이션에서 착탄 오차 0.3m 이하를 확인했다. 높은 산이 사이에 있으면 산에 맞는다.
  - 6종: `Enemy_Normal` (Abrams, 짙은 갈색, HP 100, 속도 3, 90m), `Enemy_Speed` (T-006 모델 1.5배, 붉은색, HP 60, 속도 8, 고속탄, 70m), `Enemy_Heavy` (Mk.1 모델 1.2배, 검은 회색, HP 220, 속도 2, 중포탄, 110m), `Enemy_Artillery` (Abrams, 노란 올리브색, 포신 -35°, HP 90, 150m 곡사), `Enemy_Truck` (Truck, 빨강, HP 60, 속도 10, 자폭 피해 35), `Enemy_Heli` (AH64, 어두운 회색, `NavMeshAgent.baseOffset` 15로 공중, 날개 `Spin`, HP 60, 쿨다운 3초, 100m, `isAir`). 수치는 임의 값이다.
  - 색은 원본 머티리얼을 건드리지 않고 `Enemies/Materials/<종류>_*.mat` 복사본의 `_Color`를 곱해서 바꿨다. 플레이어 모델을 쓰는 적은 조작 스크립트를 모두 지웠다 (`Enemy_Speed`는 `WheelSpin`만 남기고 `NavMeshAgent.velocity`로 굴린다).
  - `Cannon.fromEnemy`가 true면 포탄이 적을 무시하고 `Player` 태그에 피해를 준다. 플레이어 피해는 `GameManager.DamagePlayer(int)` 하나로 모았다. 플레이어 포탄이 `TakeDamage`로 HP를 0으로 만들면 `Die()`가 `scoreValue`를 더하고 폭발 이펙트 (`deathEffect`, 비면 포탄의 `expEffect`)를 낸다.
  - 새 적 추가: 루트에 `NavMeshAgent`, `BoxCollider` (모델 크기), `EnemyAI`, 포구 `FirePos`를 두고 프리팹을 만든 뒤 `WaveSpawner.enemies`에 넣는다.
- **기지 (`MainScene`)**: 사용자가 좌표를 주면 그 주변 지형 높이와 경사를 먼저 조회하고, 평지에 맞춰 30m × 30m 부지를 만든다. 지정 점이 산비탈이면 부지 중심을 평지 쪽으로 옮긴다.
  - `AllyBase`: 중심 (-93, -82), 게이트는 동쪽(+x). 파란 깃발 (`Assets/04.Images/AllyFlag.mat`). `Markers/PlayerSpawn` (-85, 3.2, -82, 동쪽을 봄).
  - `EnemyBase`: 중심 (96, 90), 게이트는 서쪽(-x). 빨간 깃발 (`Assets/04.Images/EnemyFlag.mat`). 정차한 `Tank_006` 한 대는 AI가 없는 장식이다. `Markers/EnemySpawn_1~4`가 게이트 안쪽에 있다.
  - 두 기지는 `Buildings`, `Walls`, `Props`, `Markers` 하위로 나뉜다. 건물은 모두 `ithappy` 프리팹이고, 프리팹에 콜라이더가 없어서 `BoxCollider`를 직접 붙였으며 `Static`이다. 건물 겹침은 육안으로 다 확인하지 못했다.

## 에셋

- `LowPolyMiliteryVehicles`: 플레이어 탱크 모델 (`Tank.prefab`을 풀어서 사용). Built-in `Standard` 셰이더.
- `ithappy/Military_Free`: 건물, 소품, 차량. URP 기준으로 배포되어서 `Render_Pipeline_Convert/Unity_6_Built-In_source.unitypackage`를 임포트해 머티리얼을 `Standard`로 바꿨다. 이 폴더의 프리팹에는 콜라이더가 없다. 배치할 때 직접 붙여야 한다.
- `TerrainSampleAssets`: Unity 공식 팩이다. **TerrainLayer 텍스처만 쓴다.** 풀, 덤불 프리팹과 머티리얼 (`Shader Graphs/TerrainGrass`)은 URP/HDRP 전용이라 Built-in에서 분홍색으로 깨진다. 쓰지 않는다. 폴더가 1.9GB다 (대부분 `Textures/*.tif`).
- `Game GUI Vol1`: UI 스프라이트 (4개 테마, 아이콘만 있고 패널/바 틀은 없다). `MainScene` HUD와 Intro에 `WoodenBlocks` 테마를 썼다. 주황 아이콘을 올리브색으로 색조 변환한 복사본이 `Assets/04.Images/UI/Mil_*.png`다 (`Play`, `Restart`, `Home`, `Pause`, `Trophy`, `Info`, `Plus`, `Settings`, `SoundON`, `Cancel`, `LeftArrow`, `Yes`, `Star`).
- `UnityTechnologies/ParticlePack` (Unity 공식 Particle Pack): 약 200MB이고 머티리얼이 URP 셰이더다. 쓰는 이펙트와 텍스처는 `Assets/03.Prefabs/ExplosionFX`로 변환해 두었고, 팩 폴더는 `.gitignore`로 올리지 않는다 (지워도 된다).
- `AllSkyFree` (Asset Store, 무료): 스카이박스 10종, 약 320MB. 쓰는 3종 (`Epic_BlueSunset`, `Deep Dusk`, `Cartoon Base BlueSky`)의 6면 텍스처만 `.gitignore` 예외로 올리고 나머지와 Equirect 텍스처는 올리지 않는다. 다른 하늘을 쓰려면 `.gitignore`에 예외를 추가한다.
- `Inguz Media Studio/Free 2D Impact FX`: 2D용이라 쓰지 않는다. 지워도 된다.
- `Low Poly Powerups` (Asset Store, 무료): 아이템 모델. `Assets/03.Prefabs/Items/Item_HP/Speed/Power.prefab`이 `1 Builtin` 프리팹의 `Health`, `Speed`, `Damage` 모델을 3배로 키워 쓴다. `Multiattack`, `Shield` 모델은 아직 안 쓴다. `2 URP` 폴더는 쓰지 않는다.

## 알려진 미완성 부분

- 필드 아이템은 `ItemSpawner`가 15초 간격으로 최대 5개 만든다. 효과 수치 (HP +30, Speed +3, Power +25%)는 임의 값이다.
- 포탄 모델은 아직 임시다. 3종 모두 메시 없이 궤적 (`TrailRenderer`)만 있다. 탱크 포탄 모델을 받으면 `Shell_*.prefab`에 자식으로 붙이면 된다. 탄 정보 패널의 아이콘은 `Info` 하나이고 `ShellType.icon`은 비어 있다.
- HUD 레이아웃, 폭발 이펙트의 실제 움직임, ESC 일시정지, 고속탄이 지형에서 터지는 위치는 Play에서 확인하지 못했다. 이펙트 모양은 편집 모드에서 카메라로 렌더링해서 확인했다.
- 적은 `WaveSpawner`가 생성한다. 씬에 미리 배치하지 않는다.
- `Item`의 `ItemType.Power`는 처리 분기가 없고, HP 회복이 `playerMaxHP`로 제한되지 않는다. 아이템을 생성하는 곳도 없다.
- 플레이어 탱크 종류는 하나뿐이다. 차체가 지면 경사에 맞춰 기울지 않는다.
- Photon (PUN)은 코드 파일이 모두 삭제되어 `Assets/Photon/`에 설정 에셋 2개 (`PhotonAppSettings.asset`, `PhotonServerSettings.asset`)만 남아 있다. 게임 스크립트는 Photon을 쓰지 않는다. `Assets/_Recovery`, `Assets/_TerrainAutoUpgrade`는 Unity가 만든 폴더다.
- `ProjectSettings/GraphicsSettings.asset`, `VFXManager.asset`, `ShaderGraphSettings.asset`에는 지운 URP/HDRP 패키지의 흔적이 남아 있다. 이 변경은 커밋하지 않고 있다. 렌더 파이프라인은 Built-in이다.

## 진행 상황과 다음 할 일

과제 조건 (Intro 씬, 게임 구현, Ending 씬) 대비 현재 상태다.

- 완료: 평지와 산이 섞인 지형, 플레이어 탱크 이동/포탑/포신(각도 제한)/발사/궤도, 포탄 (키 1~4, 탱크별 3~4종), 적 AI 3종 (포탄 공격, 사망, 종류별 색과 크기), 웨이브 스포너, NavMesh, 필드 아이템 3종 (HP 상한, Power 포함), 아군/적 기지, HUD, 점수와 최고 기록, 엔딩 씬 2개와 Build Settings, 플레이어 동적 생성, G키 공습, 탱크 선택 2종과 탱크별 스펙, Intro 메인 메뉴 (조작법, 설정, 탱크 선택, 전시 배경), 씬별 스카이박스 (Main, 인트로, 엔딩 승리/패배), 적 6종과 5웨이브, 조준 레이저 (`AimLaser`).
- 다음 할 일:
  1. 새 적 6종 밸런스 조정 (Play 확인 후).
  2. 기획서 (Figma Slides) 마무리: 탱크 선택, HUD 슬라이드의 Play 화면 캡처 넣기, 표지 학과·학번·이름 채우기.
  3. 마무리: 전체 플레이 테스트, Build 확인.

## 기획서 (Figma Slides)

과제 제출용 게임 기획서다. 파일 `TankAttack 게임 기획서` (`https://www.figma.com/slides/9a1t7nvdYO4SJeqmbutnnS`, 팀 `PJ`)이고 저장소에는 올리지 않는다. 예시 PDF (`example.pdf`, 65MB)는 구성만 참고했고 커밋하지 않는다.

- **구성**: 표지, 게임 기획, 기능 구현 여부 (✔/△), 목차, 섹션 1~4 (게임 흐름과 씬, 조작과 액션, 전투와 콘텐츠, UI와 부가 시스템), 섹션 5 "구현 방법 (주요 코드)" (코드 슬라이드: 이동, 조준, 레이저, 포탄 충돌, 근접 신관, 자주포 탄도, 웨이브, 공습, 적 AI, 점수). 적 6종은 슬라이드당 3종씩 2장이다. 표지와 섹션 구분 슬라이드는 `Epic_BlueSunset` 하늘 사진 배경이고 내용 슬라이드는 흰 배경이다.
- **스크린샷**: 대부분 Unity 편집 모드에서 임시 `Camera.Render`로 찍은 PNG를 `upload_assets`로 `IMG_*` 프레임에 올린 것이다 (씬은 저장하지 않고 다시 연다). 플레이 화면이 필요한 탱크 선택과 HUD 슬라이드는 "Play 화면 캡처를 여기에 넣기" 빈 칸이다 (탱크 선택 UI는 편집 모드에서 카드가 안 만들어져 렌더링이 깨진다).
- **슬라이드 3번 (과제 요구사항 구현 여부)**: 과제 조건 순서로 8항목 (1. Intro 씬, 2-1 배경, 2-2 플레이어 탱크, 2-3 적 탱크, 2-4 난이도 Wave, 2-5 아이템, 2-6 추가 기능, 3. Ending 씬)을 좌우 2열로 적었다. 사용자가 카드 배경 박스와 체크 아이콘을 지워서 텍스트만 왼쪽 정렬로 남아 있다.
- **슬라이드 25 (HUD)**: 왼쪽에 Play 화면 캡처 (`IMG_hud`, 1000×563, FIT이라 위아래 여백이 있다)와 번호 배지 ①~⑤, 그 아래 `HUDNote` (HUD 구성 3줄), 오른쪽에 같은 번호 카드 5장 (`HUDCard1~5`: 웨이브 정보, 공습·HP·가속, 포탄 버튼, 포탄 정보, 일시정지, 카드마다 확대 캡처. 5번은 `IMG_hud` 이미지를 CROP으로 잘랐다)이다.
- **슬라이드 14 (공습)**: 공습이 떨어지는 장면 렌더링 (`IMG_airstrike_drop`)을 큰 이미지로 쓰고, 기존 Su-57 이미지는 오른쪽 아래에 작게 넣었다.
- **슬라이드 17 (포탄 4종)**: 카드마다 탱크가 포탄을 쏘는 렌더링을 위쪽에 넣었다 (`ShellImage1~4`). 포탄 3종은 모델이 없어서 구체와 선으로 궤적을 흉내 냈다. 카드 높이는 680이다. 카드 아래쪽에 피해·반경, 쿨다운, 초당 피해, 일반 적 (HP 100) 격파 발수 4줄을 넣었다 (값은 두 탱크 프리팹의 `FireCannon.shells`). 포탄 수치를 바꾸면 이 슬라이드도 고친다.
- **코드 슬라이드 (28~37번)**: 같은 형식으로 다시 만들었다. 제목 아래 한 줄 요약, 왼쪽 코드 패널 (파일명, 색 구분, 주석에 ①②③④ 번호), 오른쪽에 같은 번호의 설명과 맨 아래 "핵심" 박스다. 코드는 22~27줄로 줄였고, 33번 자주포는 공식을 빼고 쉽게 줄였다. 코드가 길면 패널이 화면 아래 여백 약 45px까지 내려온다. 2026-10-23 스크립트 정리 커밋 (`406682c`)에 맞춰 29 (`RotateToward`), 30 (`AimLaser.LateUpdate`, 화면에 맞추려고 대입 몇 줄을 한 줄로 합쳤다), 33 (`SpawnShell`), 36 (`GravityDrop` 설명)을 고쳤다. 코드 색은 키워드 `#ed9e61`, 타입 `#80ccdb`, 주석 `#8cb273`, 숫자 `#c7db8c`, 기본 `#e0ebd1`이고 ①②③④는 `Noto Sans KR Bold` `#ffd94d`다.
- **수정할 때**: 슬라이드는 `figma.currentPage`의 `SLIDE_GRID` 첫 `SLIDE_ROW` 안에 있다. `getSlideGrid()`는 같은 호출 안에서 최신이 아닐 수 있어 순서 변경은 `row.insertChild`로 한다. 코드 슬라이드는 코드를 표시용으로 줄여 옮겼으니 소스를 바꾸면 슬라이드도 고친다. 글꼴은 `Noto Sans KR`과 `Roboto Mono`다.

## 작업 방식

- Play 모드 조작과 육안 확인은 사용자가 한다. Claude는 MCP의 Scene View 캡처와 에디터 모드 물리 시뮬레이션으로 검증하고, Play에서 확인하지 못한 것은 확인하지 못했다고 말한다.
- 커밋 메시지는 `feat:` 접두어에 한국어로 쓴다. 푸시는 사용자가 요청할 때만 한다.
- 커밋하지 않는 변경: `ProjectSettings/GraphicsSettings.asset`, `VFXManager.asset`, `ShaderGraphSettings.asset`, `DynamicsManager.asset`, `ProjectSettings.asset`, `TankAttack.slnx` (Unity가 다시 저장한 파일이거나 지운 URP/HDRP 흔적이다).
- 에셋을 새로 들일 때는 Built-in 호환 여부를 먼저 확인한다. 큰 에셋은 쓰는 파일만 올리도록 `.gitignore`를 조정한다 (`TerrainSampleAssets` 선례).
