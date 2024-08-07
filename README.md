# 테넌트 관리 API

-   Graph Api를 통해 유저와 그룹의 데이터를 가져옵니다.
-   가져온 데이터를 sqlite에 저장합니다.
-   유저와 그룹의 정보를 추가, 조회, 수정, 삭제할 수 있습니다.

## 실행방법

```
// 어플리케이션을 실행합니다.
dotnet run
```

## 기능 요구사항

- 기존에 만들어진 `TenantManagementApi` 제품에 다음과 같은 기능을 추가해야 한다.
  - 모든 사용자 계정 정보 불러오기
  - 모든 그룹 정보 불러오기
- 모든 사용자/그룹 정보를 제공할 때 페이징을 적용해야 한다.
  - API 호출 1번에 제공되는 정보의 개수가 최대 50개를 넘으면 안 된다.
  - API 호출 1번에 제공되는 정보의 기본 개수는 10개로 한다.
  - 당연히 이를 초과하는 다음 번 페이지를 제공하는 링크 정보를 함께 제공해야 한다.
- 클라이언트에서 우리 API를 호출할 때마다 Graph API 호출이 발생하면 안 된다.
  - 필요한 정보를 불러오는 기능을 전담하는 별도의 백그라운드 서비스를 만든다.
  - 백그라운드 서비스는 5분마다 Graph API로 정보를 불러와서 SQLite DB에 저장한다.
  - 클라이언트에서 모든 사용자/그룹 정보를 요청할 경우, SQLite DB에 저장된 내용으로 서빙한다.
- Graph API에서 `429 Too Many Requests` 코드를 반환했을 때, 이에 적절히 대응할 수 있어야 한다.

