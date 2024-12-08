# Traffic-Simulation
![traffic_thumnail](https://github.com/user-attachments/assets/14638b9e-9069-4681-a34f-5c1d232fd9df)
Unity 기반의 도로 교통 시뮬레이션 시스템으로, 에디터 상에서 도로 설계와 교통 흐름을 구현할 수 있는 프로젝트입니다.

## 📌 프로젝트 소개
도로 교통 패턴을 분석하고 최적화하기 위한 시뮬레이션 시스템으로, Unity Editor를 통해 도로 구조와 교통 흐름을 쉽게 설계하고 테스트할 수 있습니다.

## 🛠 주요 기능
- **세그먼트 설정**: 도로 구간을 정의하고 연결
- **웨이포인트 시스템**: 차량의 이동 경로 설정
- **교차로 관리**: 교차로 지점 설정 및 신호 제어
- **차량 자율 주행**: 설정된 경로를 따라 자동으로 주행
- **교통 신호 제어**: 교차로에서의 신호 체계 구현

## 🗂 주요 스크립트 설명
- `TrafficHeadquater.cs`: 전체 교통 시스템 총괄 관리
- `TrafficIntersection.cs`: 교차로 로직 처리
- `TrafficLightControl.cs`: 신호등 제어 시스템
- `VehicleControl.cs`: 차량 움직임 제어
- `WheelDriverControl.cs`: 차량 바퀴 제어

## 🎯 개발 중점 사항
- Unity Editor 확장을 통한 직관적인 도로 설계 시스템 구현
- 실시간이 아닌 에디터 상에서의 경로 설정 기능
- 자율 주행 차량의 자연스러운 움직임 구현
- 교차로에서의 효율적인 교통 흐름 관리

## 📊 시스템 구조
1. **Editor 시스템**
   - 도로 구조 설계
   - 웨이포인트 배치
   - 교차로 설정

2. **실행 시스템**
   - 차량 자율 주행
   - 교통 신호 제어
   - 충돌 방지 로직

## 🌱 향후 개발 계획
- [ ] AI 기반 교통 흐름 최적화
- [ ] 다양한 차량 타입 추가
- [ ] 보행자 시스템 구현
- [ ] 날씨 영향 시스템 추가
