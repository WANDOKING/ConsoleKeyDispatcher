# AGENTS.md

콘솔 키 입력에 핸들러를 바인딩하는 .NET 8 라이브러리이며, NuGet 패키지 `ConsoleKeyDispatcher`로 배포됩니다.

## 배포 흐름

- 작업은 `develop`(또는 거기서 딴 브랜치)에서 하고, `main`으로 PR을 올립니다.
- `main`에 머지되는 순간 CI가 NuGet에 publish하고 `v{Version}` 태그를 push합니다. 그래서 `ConsoleKeyDispatcher.csproj`의 `<Version>`을 올리는 커밋이 곧 릴리스입니다.
- 버전은 SemVer를 따릅니다. public API의 이름·시그니처·동작이 바뀌면 메이저를 올립니다.
- 버전만 올리는 커밋은 `version: 버전 X.Y.Z 배포 준비` 형식으로 따로 만듭니다.
- nuget.org 패키지는 CI에서만 만듭니다. `ContinuousIntegrationBuild`가 CI에서만 켜져서 PDB의 소스 경로가 `/_/`로 정규화되기 때문입니다. 로컬에서 pack한 패키지에는 로컬 경로가 남습니다.

## 빌드·패키징

- 솔루션이 `.slnx`라서 .NET 10 SDK가 필요합니다. 대상 프레임워크는 net8.0이므로 CI는 테스트용 8.0 런타임과 10.0 SDK를 함께 설치합니다.
- csproj의 패키지 메타데이터를 바꾸면 `dotnet pack` 후 nupkg 안의 nuspec에서 실제로 반영됐는지 확인합니다. 속성 이름이 틀리면 경고 없이 무시됩니다(예: `Tags`는 무시되고 `PackageTags`가 맞습니다).

## 커밋 메시지

`<type>: <한국어 요약>` 형식입니다. type은 `feat`, `fix`, `refactor`, `test`, `docs`, `ci`, `chore`, `version` 중 하나를 씁니다.

## 코드 규칙

- 빌드 경고 0개를 유지합니다. 서드파티 분석기 없이 .NET SDK 내장 분석기(`EnforceCodeStyleInBuild`, `AnalysisLevel`)만 씁니다. 규칙 조정은 억제 속성 대신 `.editorconfig`에서 합니다.
- 동기 API가 비동기 구현을 기다릴 때는 `.Result` 대신 `GetAwaiter().GetResult()`를 써서 예외가 `AggregateException`으로 감싸지지 않게 합니다.
- 콘솔 전용 라이브러리라 `SynchronizationContext`가 없으므로 `await`는 `ConfigureAwait` 없이 씁니다.
- 이벤트 인자는 `EventArgs`를 상속한 일반 클래스와 기존 생성자로 작성합니다. record는 `EventArgs`를 상속할 수 없고, 기본 생성자(primary constructor)는 매개변수가 readonly가 아니어서 쓰지 않습니다.
- XML 문서 주석과 코드 주석은 한국어, 예외 메시지는 영어로 씁니다.
- `ConsoleKeyDispatcher`는 전역 상태를 가진 static 클래스입니다. 새 상태를 추가하면 `Reset()`에서도 초기화하고, 테스트 클래스는 `[TestInitialize]`에서 `Reset()`을 호출합니다.

## 문서

- `README.md`는 NuGet 패키지 페이지에도 그대로 실립니다. public API를 바꾸면 같은 커밋에서 README를 갱신합니다.
- README의 "예제 코드"는 `ConsoleKeyDispatcher.Example/Program.cs`의 `Main` 본문과 같게 유지합니다.

## TODO

- **네임스페이스와 클래스 이름 정리**: 네임스페이스와 클래스가 같은 이름이면 충돌하므로 네임스페이스를 임시로 `ConsoleKeyUtils`로 두었습니다. 그래서 패키지 ID(`ConsoleKeyDispatcher`)와 네임스페이스가 어긋나 있습니다. 네임스페이스를 `ConsoleKeyDispatcher`로 맞추고 클래스를 `KeyDispatcher` 같은 역할 이름으로 바꾸는 안이 유력합니다. 3.0.0에서 호환 래퍼 없이 이름을 바로 바꿉니다.
