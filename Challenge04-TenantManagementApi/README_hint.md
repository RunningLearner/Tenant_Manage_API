# 5번째 과제: `directclouddev` 테넌트 관리 API 기능 추가하기

지난번에 만든 `TenantManagementApi` 제품에 새로운 기능을 추가해야 한다.

## 과제 수행 절차

1. 이 Git 저장소를 자기 PC로 Clone하여, `sinam-05` 브랜치로 Checkout한다.
2. 해당 브랜치에서 기능 요구사항과 비기능적 요구사항을 모두 충족하는 앱을 작성한다.
3. 모든 요구사항을 해결했으면, `README.md` 등 문서를 작성한다.
4. 작성한 모든 내용을 서버에 Push한다.
5. `sinam-05-result` 이름으로 된 브랜치를 대상으로 Pull Request를 작성한다.
6. Pull Request를 통해 강사의 코드 리뷰를 받고 피드백에 따라 수정한다.
7. 리뷰어가 PR을 승인하여 Pull Request가 통과되면 과제 완료.

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
  - Graph SDK에는 이미 관련 기능이 내장되어 있으므로, 이를 학습하고 옵션을 살펴본다.

## 비기능적 요구사항

- 기존 과제에서 유지했던 각종 컨벤션과 원칙들을 준수해야 한다.
- API는 최대한 RESTful하게 만든다.
- 지난 과제에서는 시간상의 이유로 적용하지 못했던 `README.md` 작성을 이번에는 하도록 한다.

## 힌트

- RESTful 웹 API 디자인과 관련하여 MS에서 제공하는 각종 자료를 참고한다.
  - [모범 사례: RESTful 웹 API 디자인](https://learn.microsoft.com/ko-kr/azure/architecture/best-practices/api-design)
  - [모범 사례: 웹 API 구현](https://learn.microsoft.com/ko-kr/azure/architecture/best-practices/api-implementation)
  - [Azure API Design eBook](https://azure.microsoft.com/mediahandler/files/resourcefiles/api-design/Azure_API-Design_Guide_eBook.pdf) (PDF)
  - [Microsoft Azure REST API Guidelines](https://aka.ms/azapi/guidelines) (MS의 내부 API 설계 지침)
- Graph API에서 `429 Too Many Requests` 응답을 반환하는 것을 로컬에서 시뮬레이션하려면 [Microsoft 365 Developer Proxy](https://github.com/microsoft/m365-developer-proxy)를 사용한다.
- 시간이 남으면 최신 버전의 `Microsoft.AspNetCore.OData` 패키지를 통해 ASP.NET Core Web API에서 OData 규격을 준수하는 RESTful API를 구축하는 방법을 자습한다.
