# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

Unity 6 (`6000.5.10f1`) 탱크 슈터 게임. 게임 스크립트는 `Assets/02.Scripts/`에 있고 총 12개 파일, 약 560줄이다. UI는 `com.unity.ugui` (legacy `UnityEngine.UI.Text`)를 쓴다. 입력은 legacy `Input` 클래스를 쓴다 (Input System 패키지 아님).

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
- `Unity_Camera_Capture`는 Scene View 카메라라서 Screen Space Overlay UI는 찍히지 않는다. UI 확인은 Game 뷰에서 한다.
- 콘솔의 `Serialized files [version N] before 2019.1 are deprecated` 경고는 `ProjectSettings/*.asset`이 오래된 포맷이라 생기는 것이며 무시해도 된다.

## 구조

씬은 `Assets/01.Scenes/`의 `IntroScene` (시작 버튼 UI + 탱크 배경), `MainScene`, `MapTestScene` 세 개다. **현재 개발 기준 씬은 `MapTestScene`이다** (새 지형 + 새 탱크). `MainScene`의 탱크는 옛 구조이며 기준으로 삼지 않는다. 데이터 흐름은 컴포넌트가 서로를 직접 참조하는 단순한 구조이며, 여러 파일을 봐야 보이는 규칙은 다음과 같다.

- **플레이어 탱크 (`MapTestScene`)**: 루트 `Tank`에 `Rigidbody`, `BoxCollider`, `TankMove`, `FireCannon`, `AudioSource`가 붙고 태그는 `Player`다. 그 아래 `Tank/Tank` (프리팹 연결을 푼 모델)에 `Body`, `Chain`, `TurretPivot`이 있다. `TurretPivot`(`TurretCtrl`, 마우스 방향 포탑 회전) 아래에 `Head`와 `CannonPivot`(`CannonCtrl`, 마우스 휠로 포신 각도 -8~25도 제한)이 있고, `CannonPivot` 아래에 `Canon`과 `FirePos`(포구, `FireCannon.firePos`)가 있다. 카메라는 `SmoothFollow`로 `Tank/CamPivot`을 따라간다. 포탑과 포신이 같이 돌도록 `Head`와 `Canon`은 반드시 이 피벗들의 자식이어야 한다.
- **이동 (`TankMove`)**: 지형과의 물리 충돌을 끄고 (`Physics.IgnoreCollision`, 중력도 끔) 코드가 `Terrain.SampleHeight`로 지면 높이를 따라간다. 수평 속도는 평지든 경사든 항상 `moveSpeed`다. 지형 가장자리 `edgeMargin` 안쪽까지만 간다. 지형 충돌에 맡기면 박스 콜라이더가 울퉁불퉁한 지면에 걸쳐 뜨고, 능선에서 상승 속도로 날아가고, 경사 제한을 두면 경사 근처에서 멈춘다. 건물과 적과의 충돌은 그대로다.
- **궤도 (`TrackScroll`)**: `Chain` 메시의 UV가 한 점에 몰려 있어서 텍스처 스크롤이 안 보인다. 그래서 `Assets/05.Models/ChainTread.asset`(UV를 탱크 앞뒤 방향으로 다시 투영한 복사본)과 `Assets/04.Images/TrackTread.mat`를 `Chain`에 씌우고, `TrackScroll`이 `Rigidbody` 속도에 맞춰 오프셋을 흘린다. 옛 `TankAnim`은 `MainScene`용이다.
- **포탄 발사**: `FireCannon`은 `Resources.Load("Cannon")`과 `Resources.Load<AudioClip>("CannonFire")`로 에셋을 이름으로 불러온다. 그래서 `Assets/Resources/Cannon.prefab`, `CannonFire.wav`의 이름과 위치를 바꾸면 안 된다. 포탄 프리팹은 `Cannon.cs`가 `Start`에서 `AddForce`로 날리고, 트리거 충돌이나 3초 후에 폭발 이펙트를 만든다.
- **레이어**: `TurretCtrl`은 레이어 8 (`Terrain`)에만 레이캐스트한다 (`1 << 8`). 지형 레이어를 바꾸면 포탑 조준이 멈춘다.
- **태그**: `Player` 태그를 `EnemyAI`와 `Item`이 사용한다. 플레이어 `Tank`에 이 태그가 있어야 한다.
- **GameManager**: `GameManager.instance` 싱글턴이 `playerHP`, 웨이브 진행, 씬 전환을 가진다. 씬 전환은 이름 문자열로 한다.
- **지형 (`MapTestScene`)**: `ArenaTerrain`은 300m × 300m × 높이 50m, 레이어 8이다. 데이터는 `Assets/01.Scenes/MapTestTerrain.asset`이고, 노이즈로 평지와 산을 섞어 코드로 만들었다 (생성 스크립트는 저장하지 않았다). TerrainLayer는 `TerrainSampleAssets/TerrainLayers`의 `Grass_A`, `Grass_Soil`, `Rock`, `Sand`, `Muddy`, `Pebbles_A` 6개를 쓴다. 지형을 바꾼 뒤에는 지형 충돌체 데이터를 다시 할당한다.

## 에셋

- `LowPolyMiliteryVehicles`: 플레이어 탱크 모델 (`Tank.prefab`을 풀어서 사용). Built-in `Standard` 셰이더.
- `ithappy/Military_Free`: 건물, 소품, 차량. URP 기준으로 배포되어서 `Render_Pipeline_Convert/Unity_6_Built-In_source.unitypackage`를 임포트해 머티리얼을 `Standard`로 바꿨다. 이 폴더의 프리팹에는 콜라이더가 없다. 배치할 때 직접 붙여야 한다.
- `TerrainSampleAssets`: Unity 공식 팩이다. **TerrainLayer 텍스처만 쓴다.** 풀, 덤불 프리팹과 머티리얼 (`Shader Graphs/TerrainGrass`)은 URP/HDRP 전용이라 Built-in에서 분홍색으로 깨진다. 쓰지 않는다. 폴더가 1.9GB다 (대부분 `Textures/*.tif`).
- `Game GUI Vol1`: UI 스프라이트. 아직 씬에 적용하지 않았다.

## 알려진 미완성 부분

- `GameManager`가 불러오는 `Ending_Win`, `Ending_Lose` 씬은 아직 없다. Build Settings에도 없으면 `LoadScene`이 실패한다.
- `GameManager.OnPlayerDead`는 `EnemyAI`가 호출하지만, `OnWaveClear`를 부르는 곳은 없다. 웨이브 스포너가 없다.
- `MapTestScene`에는 NavMesh를 굽지 않았다. `EnemyAI`는 `NavMeshAgent`를 쓰므로 적이 움직이지 못한다. 스폰 마커와 건물 배치도 아직 없다 (건물과 소품은 사용자가 직접 배치한다).
- `Item`의 `ItemType.Power`는 처리 분기가 없고, HP 회복이 `playerMaxHP`로 제한되지 않는다. 아이템을 생성하는 곳도 없다.
- 플레이어 탱크 종류는 하나뿐이다. 차체가 지면 경사에 맞춰 기울지 않는다.
- Photon (PUN)은 코드 파일이 모두 삭제되어 `Assets/Photon/`에 설정 에셋 2개 (`PhotonAppSettings.asset`, `PhotonServerSettings.asset`)만 남아 있다. 게임 스크립트는 Photon을 쓰지 않는다. `Assets/_Recovery`, `Assets/_TerrainAutoUpgrade`는 Unity가 만든 폴더다.
- `ProjectSettings/GraphicsSettings.asset`, `VFXManager.asset`, `ShaderGraphSettings.asset`에는 지운 URP/HDRP 패키지의 흔적이 남아 있다. 이 변경은 커밋하지 않고 있다. 렌더 파이프라인은 Built-in이다.
