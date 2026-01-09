## What Problem It Solves
解决工业现场“上位机-执行机”配置数据不可信、不一致、不闭环的问题。

## Typical Scenario
- C#/WPF 上位机
- LabVIEW 执行机
- 文件交互
- 参数频繁变更

## Core Design
- Code-as-Protocol（Shared DLL）
- Encrypted Config（AES + JSON）
- Closed Loop Feedback（软拦截）

## What It Is NOT
- Not real-time control
- Not MES
- Not big data storage
