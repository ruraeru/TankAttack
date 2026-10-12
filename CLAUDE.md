# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

Unity 6 (`6000.5.10f1`) 탱크 슈터 게임. 게임 스크립트는 `Assets/02.Scripts/`에 있고 총 10개 파일, 약 350줄이다. UI는 `com.unity.ugui` (legacy `UnityEngine.UI.Text`)를 쓴다. 입력은 legacy `Input` 클래스를 쓴다 (Input System 패키지 아님).

빌드/린트/테스트 CLI는 없다. 테스트 코드도 없다. 빌드와 실행은 Unity Editor에서 하고, 스크립트 컴파일 결과는 Editor 콘솔로 확인한다.

## Unity MCP

`.mcp.json`이 `unity-mcp` 서버를 연결한다. Editor가 열려 있을 때 `mcp__unity-mcp__*` 도구로 씬 조회/수정 (`Unity_RunCommand`), 콘솔 로그 (`Unity_GetConsoleLogs`), 캡처를 할 수 있다.

- `Unity_RunCommand` 코드에서 `Image`는 `UnityEngine.UI.Image`로 풀네임을 써야 한다 (네임스페이스와 충돌해 CS0118).
- `Unity_Camera_Capture`는 Scene View 카메라라서 Screen Space Overlay UI는 찍히지 않는다. UI 확인은 Game 뷰에서 한다.
- 콘솔의 `Serialized files [version N] before 2019.1 are deprecated` 경고는 `ProjectSettings/*.asset`이 오래된 포맷이라 생기는 것이며 무시해도 된다.

## 구조

씬은 `Assets/01.Scenes/`의 `IntroScene` (시작 버튼 UI + 탱크 배경)과 `MainScene` 두 개다. 데이터 흐름은 컴포넌트가 서로를 직접 참조하는 단순한 구조이며, 여러 파일을 봐야 보이는 규칙은 다음과 같다.

- **플레이어 탱크**는 `Tank` 오브젝트 하나에 컴포넌트가 나뉘어 붙는다. `TankMove` (이동/회전, `Horizontal`/`Vertical` 축), `FireCannon` (좌클릭 발사), 자식 `Turret`의 `TurretCtrl` (마우스 방향 포탑 회전), 자식 `Cannon`의 `CannonCtrl` (마우스 휠로 포신 각도), `Track`의 `TankAnim` (궤도 텍스처 스크롤). 카메라는 `SmoothFollow`로 `Tank/CamPivot`을 따라간다.
- **포탄 발사**: `FireCannon`은 `Resources.Load("Cannon")`과 `Resources.Load<AudioClip>("CannonFire")`로 에셋을 이름으로 불러온다. 그래서 `Assets/Resources/Cannon.prefab`, `CannonFire.wav`의 이름과 위치를 바꾸면 안 된다. 포탄 프리팹은 `Cannon.cs`가 `Start`에서 `AddForce`로 날리고, 트리거 충돌이나 3초 후에 폭발 이펙트를 만든다.
- **레이어**: `TurretCtrl`은 레이어 8 (`Terrain`)에만 레이캐스트한다 (`1 << 8`). 지형 레이어를 바꾸면 포탑 조준이 멈춘다.
- **태그**: `Player` 태그를 `EnemyAI`와 `Item`이 사용한다. 플레이어 `Tank`에 이 태그가 있어야 한다.
- **GameManager**: `GameManager.instance` 싱글턴이 `playerHP`, 웨이브 진행, 씬 전환을 가진다. 씬 전환은 이름 문자열로 한다.

## 알려진 미완성 부분

- `GameManager`가 불러오는 `Ending_Win`, `Ending_Lose` 씬은 아직 없다. Build Settings에도 없으면 `LoadScene`이 실패한다.
- `EnemyAI`는 이동만 하고 공격, 피격, 사망 처리가 없다. `Cannon`도 데미지를 주지 않는다. `GameManager.OnPlayerDead`/`OnWaveClear`를 부르는 곳은 아직 없다.
- `Item`의 `ItemType.Power`는 처리 분기가 없다.
- `Assets/Photon/` (PUN)이 들어 있지만 현재 게임 스크립트는 Photon을 쓰지 않는다. 서드파티 코드이므로 수정하지 않는다. `Assets/_Recovery`, `Assets/_TerrainAutoUpgrade`는 Unity가 만든 폴더다.
