# Localization

> Unity Localization Package를 활용한 다국어 지원 시스템
>
> 언어별 사용 문자 추출 및 TMP 폰트 최적화 도구

## 📋 목차

- [개요](#개요)
- [해결한 문제](#해결한-문제)
- [디렉토리 구조](#디렉토리-구조)
- [핵심 기능](#핵심-기능)
- [주요 API](#주요-api)
- [사용 예시](#사용-예시)
- [기술적 의사결정](#기술적-의사결정)

---

## 개요

Localization 시스템은 Unity Localization Package 기반의 다국어 지원 시스템입니다. 단순 번역 제공뿐만 아니라, **언어별 사용 문자 추출**을 통해 TMP 폰트 용량을 최적화하는 에디터 툴을 제공합니다.

### 주요 특징

- ✅ **언어별 사용 문자 추출** - 실제 사용되는 문자만 수집
- ✅ **TMP 폰트 최적화** - Dynamic Font 대비 용량 대폭 감소
- ✅ **안전한 포맷팅** - `string.Format` 기반 플레이스홀더 지원
- ✅ **에디터 툴 자동화** - 반복 작업 시간 단축

---

## 해결한 문제

### 1. 폰트 용량 문제

**문제:**
- Dynamic Font 사용 시 **과도한 용량** 문제 발생
- 모바일 환경에서 다운로드/메모리 부담

**해결:**
- 언어별 실제 사용 문자만 추출
- TMP Font Asset 생성 시 필요한 문자만 포함
- 용량 최적화 + 문자 누락 방지

---

### 2. 번역 누락 추적 어려움

**문제:**
- 어디서 번역이 누락되었는지 파악 어려움
- 실행 중에야 번역 키 오류 발견

**해결:**
- Unity Localization Package의 StringTable 활용
- Inspector에서 누락된 번역 확인 가능
- 중앙 집중식 관리로 일관성 유지

---

### 3. 중복 번역 문제

**문제:**
- 시스템 초기부터 적용하지 않으면 각 위치마다 번역을 따로 관리
- 동일한 문장/단어가 여러 곳에 중복 저장

**해결:**
- StringTable 기반 중앙 관리
- 키 기반 참조로 중복 제거
- 번역 변경 시 모든 위치 자동 반영

---

### 4. 새 프로젝트 세팅 시간

**문제:**
- 다국어 지원 세팅이 반복 작업임에도 일정 시간 소요
- 매번 수동으로 폰트 문자 확인/추가

**해결:**
- 에디터 툴로 자동화 (`Tools/Localization/Export Used Characters`)
- 1클릭으로 모든 언어의 사용 문자 추출
- 새 프로젝트 세팅 시간 단축

---

## 디렉토리 구조

```
Localization/
├── Localizing.cs                          # 헬퍼 API (Get, GetFormat)
└── Editor/
    └── LocalizationFontCharExtractor.cs   # 문자 추출 에디터 툴
```

**핵심 파일:**
- [`Localizing.cs`](Localizing.cs): 번역 조회 헬퍼 API
- [`LocalizationFontCharExtractor.cs`](Editor/LocalizationFontCharExtractor.cs): 언어별 사용 문자 추출 툴

---

## 핵심 기능

### 1. 언어별 사용 문자 추출

**목적:** TMP Font Asset 생성 시 필요한 최소 문자만 포함

**동작 방식:**
1. `Assets/Localization` 폴더의 모든 StringTableCollection 탐색
2. 각 언어(en, ko 등)별로 모든 번역 문자열 수집
3. 중복 제거하여 HashSet으로 저장
4. `Assets/Editor/ExportCharacters/UsedFontCharacters_{locale}.txt` 파일로 출력

**실행 방법:**
- `Tools → Localization → Export Used Characters (Per Locale)`

**출력 예시:**
```
UsedFontCharacters_ko.txt: 가나다라마바사...1234567890!?
UsedFontCharacters_en.txt: ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefg...1234567890!?
```

---

### 2. 번역 조회 헬퍼 (Localizing)

Unity Localization API를 래핑하여 간편한 번역 조회 및 변수 삽입 지원 (예: "레벨 {0} 달성!")

---

## 주요 API

**Localizing:**
- `Get(string table, string key)` - 번역 조회
- `GetFormat(string table, string key, params object[] args)` - 플레이스홀더 포맷팅

**LocalizationFontCharExtractor:**
- `Tools/Localization/Export Used Characters (Per Locale)` - 사용 문자 추출

---

## 사용 예시

### 기본 번역 조회

```csharp
// 단순 번역
string greeting = Localizing.Get("UI", "greeting");
// 결과: "안녕하세요" (한글) / "Hello" (영어)

// 포맷팅
string welcome = Localizing.GetFormat("UI", "welcome_message", playerName);
// StringTable의 "welcome_message": "Welcome, {0}!"
// 결과: "Welcome, John!"
```

---

### TMP 폰트 에셋 생성 워크플로우

1. `Tools → Localization → Export Used Characters (Per Locale)` 실행
2. TMP Font Asset Creator에서 추출된 문자 리스트 사용
3. 언어별 폰트 에셋 생성 (용량 최적화 + 문자 누락 방지)

---

### 플레이스홀더 활용

**StringTable 설정:**
```
Key: level_up_message
Value (ko): "레벨 {0}에 도달했습니다! HP +{1}"
Value (en): "Level {0} reached! HP +{1}"
```

**코드:**
```csharp
int level = 5;
int hpBonus = 100;
string message = Localizing.GetFormat("UI", "level_up_message", level, hpBonus);
// 결과 (ko): "레벨 5에 도달했습니다! HP +100"
// 결과 (en): "Level 5 reached! HP +100"
```

---

## 기술적 의사결정

### 1. Dynamic Font vs Static Font Asset

**문제:** Dynamic Font 사용 시 과도한 용량 문제

**결정:** Static Font Asset + 사용 문자 추출 툴

**이유:**
- Dynamic Font: 전체 문자 포함 → 수십 MB
- Static Font Asset: 필요한 문자만 → 수백 KB
- 모바일 환경에서 다운로드/메모리 부담 감소

**트레이드오프:**
- 장점: 용량 최적화, 로딩 속도 향상
- 단점: 번역 추가 시 폰트 재생성 필요
- 해결: 에디터 툴로 1클릭 자동화

---

### 2. 언어별 분리 추출

각 언어마다 필요한 문자가 다르므로 언어별 별도 파일 출력

---

### 3. Unity Localization Package 활용

**문제:** 자체 구현 vs Unity 공식 패키지

**결정:** Unity Localization Package 사용

**이유:**
- 에디터 통합 (StringTable 편집 GUI)
- Async 로딩 지원
- Addressable과 연동 가능
- 검증된 솔루션

---

### 4. string.Format vs 자체 파싱

**문제:** 플레이스홀더 처리 방법

**결정:** `string.Format` 사용

**이유:**
- C# 표준 API (안정성)
- 번역가가 익숙한 `{0}`, `{1}` 문법
- 추가 구현 불필요

---

## 라이선스

이 프로젝트는 포트폴리오 목적으로 제작되었습니다.
