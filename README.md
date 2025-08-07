# TheCatAndMonsterForest Unity Scripts

Unity 프로젝트의 핵심 게임 로직 및 Firebase 연동 관련 C# 스크립트만 추출한 서브 리포지토리입니다.

> 본 저장소는 에셋, 씬, 프리팹 등은 포함하지 않으며 `Assets/01.Scripts/` 경로의 코드만 포함되어 있습니다.

## 🗂 디렉토리 구조 및 설명
```
01.Scripts/
├── Data/
│ ├── RowData/ # 마스터 데이터 구조 정의 (Firebase )
│ ├── Table/ # 게임 내 구조화된 테이블 데이터
│ ├── User/ # 유저 저장용
│
├── Firebase/ # Firebase 조회, 저장, GA 
│
├── Handler/ # 마스터/유저 데이터 로딩 핸들러
│
├── Manager/
│ ├── InventoryManager/ # 인벤토리 관리 및 관련 서비스
│ │ ├── Cache/ # 인벤토리 캐싱
│ │ └── Services/ # 강화, 분해, 돌파, 자원 등 처리 서비스
│ ├── RewardManager/ # 보상 처리 로직
│ └── ... # ObjectPoolManager 등 공통 매니저
│
├── Pool/ # 오브젝트 풀링 등록기
│
├── Static \ Utils/ # 공통 파서 등 유틸리티
│
├── User/
│ ├── Inventory/ # 유저 보유 인벤토리 데이터 정의
│ │ ├── InventoryItem.cs # 장비/소모품 등 개별 아이템 데이터
│ │ └── InventoryUnit.cs # 유닛(캐릭터) 데이터
```

## 💡 주요 구현 사항
- **데이터 구조화**  
  - `RowData/`: CSV 기반 마스터 데이터 정의
  - `Table/`: 테이블형 관계 구조화 (ex: 가챠 테이블, 유닛 해금 조건 등)
  - `User/`: Firestore 저장용 유저 데이터 구조
  
- **인벤토리 시스템**  
  - `UserInventory`: 아이템, 유닛 등 전체 보유 데이터 포함
  - `InventoryItem`: 장비/소모품 데이터, 강화/돌파 정보 포함
  - `InventoryUnit`: 유저 보유 유닛(고양이) 정보
  - `UserCollected`: 도감 수집 정보 (아이템 수집 여부)
  - `UserStage`: 스테이지 클리어, 보상 수령 여부 관리
  - `InventoryManager`: 아이템 지급, 정렬, 장착, 분해 등을 처리하는 중심 매니저
  - `LimitBreakService`, `EnhancementService`: 강화/돌파 전담 서비스

- **Firebase 연동**
  - `FirestoreManager`: Firebase Auth 구글 및 게스트 로그인
  - `FirestoreUploader`: Firestore 마스터데이터 일괄 업로드
  - `FirestoreHelper`: 인증 및 실시간 데이터 통신 도우미
  - `AnalyticsHelper`: GA4 이벤트 로깅 헬퍼
---

## 📌 주의사항
- Unity 프로젝트 전체가 아닌 코드 전용 리포지토리이므로, 실행에 필요한 에셋은 별도 프로젝트에서 구성 필요
- Firebase 연동을 위한 `google-services.json`은 포함되어 있지 않음
- 실제 테스트는 Unity 환경에서 필요

---

## 🔧 개발 환경

- **Unity Version**: `2022.3.17f1`
- **Firebase SDK for Unity**
- **C# 8.0** / .NET Standard 2.1

---

## 🙌 작업자 메모

이 저장소는 본인이 담당한 Unity 프로젝트의 백엔드 로직 중심으로 구성되어 있으며, 주요 관심사는 아래과 같음

- 데이터 구조의 명확한 분리 (RowData, UserData)
- 서비스 기반 설계 패턴 (단일 책임 원칙 중심)
- Firebase 연동 최적화 및 예외 대응
- 코드 테스트 및 유지보수 용이성

---
