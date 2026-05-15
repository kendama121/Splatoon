# Splatoon 3 武器・サブ・スペシャル 数値データベース

> Unity完全再現用の数値レベル仕様書。
> 一次ソース: Leanny/splat3 任天堂内部GameParameterTable JSON群 (v1.1.10 / Drizzle Season 2024+ 相当)
> 補助ソース: Inkipedia (splatoonwiki.org), Game8, Squidboards, sendou.ink, VS Battles Wiki (Podonklos解析)
>
> ## 重要な単位・スケール換算
>
> | 概念 | 内部値 | 表示値 |
> |------|--------|--------|
> | プレイヤーHP | 1000 | 100.0 |
> | ダメージ値 | 360 | 36.0 |
> | フレーム | 60fps基準 | 1秒 = 60F |
> | 距離単位 (DU) | 内部単位 | おおむね 1 DU ≈ 1 m。マッチング表示射程 ≒ DU/4 |
> | 速度 | DU / フレーム | (DU/F) × 60 = DU/sec |
> | 重力 | 0.016 DU/F² (free) / 0.07 (brake) | 一般弾 |
> | 空気抵抗 | 2% (free) / 36% (brake) | 一般弾 |
>
> ## 弾道の3状態モデル (全シューター・スピナー・チャージャー共通)
>
> 1. **Straight (直進)** 一定速度 `SpawnSpeed` で `GoStraightToBrakeStateFrame` フレーム直進。重力影響なし。
> 2. **Brake (減速)** 速度 = `GoStraightStateEndMaxSpeed` 以下にクランプ後、空気抵抗36%・重力0.07を毎フレーム適用。Y速度が -0.15 を下回るとFreeへ遷移。
> 3. **Free (自由落下)** 空気抵抗2%・重力0.016 (一般弾) を毎フレーム適用。最終的に着弾。
>
> ダメージは弾発射からの経過フレームに基づく ── `ReduceStartFrame` 〜 `ReduceEndFrame` の間で `ValueMax → ValueMin` に線形減衰。
> 「効果射程 (effective range)」 = ReduceStartFrame時点で弾が到達している距離。クロスヘアの位置はこのポイント。
>
> ---

## 0. 共通設計データ

### プレイヤー本体 (data/parameter/1110/misc/SplPlayer.game__GameParameterTable.json)

| 項目 | 値 | 備考 |
|------|----|------|
| 最大HP | 100.0 (内部1000) | 全プレイヤー共通 |
| ヒト形態当たり判定 | カプセル R=0.7 DU / H=1.65 DU | Squidboards実測 |
| イカ形態当たり判定 | 球体 R=0.675 DU | v11.0.0以降 (旧0.8) |
| イカダッシュ速度上限 | 0.288 DU/F (= 17.28 DU/s) | SquidSpeedUpMaxSpeed |
| 高跳び (壁ジャンプ) | 0.145 DU/F | GrindRailPlayerJumpSpeed |
| 壁ジャンプ溜め | Low 45F / Mid 18F / High 5F | LoMidHi はレベル別 |
| 復活ブラスト範囲 | 5.0 DU 半径 | DieBlast |

### 弾道物理共通定数

| 項目 | Free状態 | Brake状態 | 出典 |
|------|---------|----------|------|
| 重力 | 0.016 DU/F² | 0.07 DU/F² | XarrotD解析 / 各JSON FreeGravity |
| 空気抵抗 | 2% (×0.98/F) | 36% (×0.64/F) | XarrotD解析 |
| Straight→Brake遷移閾値 | Y速度 < -0.15 | — | XarrotD解析 |

---

## 1. シューター (Shooter) — 全7+種

> 内部ファイル: `WeaponShooter*.game__GameParameterTable.json`
> 共通スキーマ:
> - `DamageParam.ValueMax/Min` — 表示値の10倍。`ReduceStartFrame/EndFrame` — 減衰の開始・終了F
> - `MoveParam.SpawnSpeed` — 発射初速 (DU/F)、`GoStraightToBrakeStateFrame` — 直進F、`GoStraightStateEndMaxSpeed` — Brake突入時の速度上限
> - `WeaponParam.InkConsume` — 1発の消費 (0.0〜1.0)、`RepeatFrame` — 連射間隔F、`Stand_DegSwerve/Jump_DegSwerve` — 静止時/ジャンプ時の発射ブレ角度
> - `PaintParam.WidthHalfNear/Middle/Far` — 着弾塗り半径 (DU)、`DistanceFar/Middle/Near` — 切替距離

### 1.1 シューター数値表

| 内部名 | 推定対応武器 | 直撃Max(=Min×2) | 減衰F | 初速 DU/F | 直進F | Brake max DU/F | インク消費 | 連射F | 静止ブレ° | ジャンプブレ° | 撃ち歩き DU/F |
|--------|------------|-----------------|------|----------|------|----------------|----------|------|----------|--------------|--------------|
| WeaponShooterNormal | スプラシューター | 36/18 | 8→40 | 2.266 | 4 | 1.493 | 0.92% | (※4-6F) | 4.86 | 11.66 | 0.072 |
| WeaponShooterFirst | わかばシューター | 28/14 | 8→24 | 2.266 | 3 | 1.9513 | 0.43% | 5 | 11.66 | 14.58 | 0.076 |
| WeaponShooterShort | .52ガロン | 38/19 | 6→22 | 2.06 | 2 | 1.835 | 0.80% | 5 | 11.66 | 17.49 | 0.080 |
| WeaponShooterGravity | .96ガロン | 52/30 | 11→27 | 3.06 | 3 | 1.667 | 1.50% | 9 | 6.0 | 12.0 | 0.060 |
| WeaponShooterBlaze | プライムシューター系 | 24/12 | 8→24 | 2.266 | 3 | 1.9513 | 0.50% | (4) | 12.63 | 15.54 | 0.072 |
| WeaponShooterFlash | スプラッシュ系? | 28/14 | 4→20 | 4.0376 | 2 | 1.6289 | 0.80% | 5 | 0.0 | 0.0 | 0.072 |
| WeaponShooterPrecision | ジェットスイーパー | 38/19 | 8→24 | 3.05 | 4 | 2.8303 | 0.80% | (8) | 0.0 | 0.0 | 0.066 |
| WeaponShooterLong | エクスシューター系 | 32/16 | 9→25 | 3.36 | 5 | 2.232 | 1.60% | 8 | 2.5 | 8.0 | 0.060 |

備考:
- WeaponShooterNormal (スプラシューター) は `PostDelay/RepeatFrame` が標準JSONに直接含まれず、CompositeのMainShooterから推測。Inkipediaは6F/連射 (10発/秒) と公表。
- 確殺距離 = `(SpawnSpeed × GoStraightToBrakeStateFrame) + (Brake状態でMax→Min閾値の50%地点)` で算出。50%閾値を超えると2発必要。
- `PaintParam` のDistanceMiddle以降は塗り幅の遠近補間。Splattershot: Near=1.93, Middle=1.93, Far=1.71。

### 1.2 ボトルガイザー (`WeaponShooterFlash` Variable機構)

WeaponShooterFlashとExplosive系には `VariableXxxParam` 群があり、これは「サブモード」(L/H3みたいに3連射などのバースト)を表現する。
- `VariableShotRepeatStartFrame` でモード切替トリガーフレーム指定
- `VariableDamageParam` で別ダメージ — 例: 30/15 (Variable), 38/19 (通常)

### 1.3 N-ZAP/L3/H3 系

JSON上は `WeaponShooterLong`(=エクスプロッシャーかL3) など。N-ZAPは別ファイル不明 — `WeaponShooterFlash` の派生武器とInkipediaが扱う。

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponShooterNormal.game__GameParameterTable.json
- https://splatoonwiki.org/wiki/Main_weapon_data
- https://splatoonwiki.org/wiki/Splattershot
- https://splatoonwiki.org/wiki/Aerospray_MG

---

## 2. ローラー (Roller) — 全5種

> 共通スキーマ:
> - `BodyParam.Damage` — 轢殺ダメージ (内部値)、`BodyParam.PaintParam.WidthHalfMax` — コロガシ塗り幅、`BodyParam.PaintParam.SpeedMax` — コロガシ最高速 (DU/F)
> - `WeaponRollParam.SpeedDash/SpeedNormal` — ダッシュコロガシ/通常コロガシ速度、`DashFrame` — ダッシュ持続F
> - `WeaponVerticalSwingParam.SwingFrame/InkConsume` — タテ振りF/消費、`WeaponWideSwingParam` — ヨコ振り
> - `VerticalSwingUnitGroupParam.DamageParam.Inside/Outside` — 中心軸 (Inside) と扇外 (Outside) の遠近4段階ダメージ

### 2.1 ローラー数値表

| 内部名 | 武器 | 轢殺ダメ | コロガシ速度 | 塗り幅 | ヨコ振りF | タテ振りF | ヨコ消費 | タテ消費 | 中心最大ダメ | 中心最大距離 |
|--------|------|---------|------------|-------|----------|----------|---------|---------|------------|------------|
| WeaponRollerNormal | スプラローラー | 125 (内部1250) | 0.132 DU/F | 2.8 DU | 21F | 26F | 8.5% | 8.5% | 150 (1500) at 5.2 DU内 | — |
| WeaponRollerHeavy | ダイナモローラー | 125 | 0.108 DU/F | 3.4 DU | 45F | 55F | 21.0% | 21.0% | 180 (1800) at 8.2 DU | 縦14 DU届く |
| WeaponRollerCompact | カーボンローラー | 70 | 0.152 DU/F (最速) | 2.2 DU | 10F (最速) | 12F | 3.96% | 3.96% | 100 at 4.7 DU内 | DashFrame=20F限定 |
| WeaponRollerHunter | ヴァリアブルローラー | 125 | 0.132 DU/F | 2.6 DU | 19F | 42F | 8.0%/12.0% | 12.0% | 150 at 5.2 DU内 (前方狭) | 7+2扇形 |
| WeaponRollerWide | ワイドローラー | 70 | 0.142 DU/F | 3.4 DU (最広) | 18F | 20F | 9.0% | 9.0% | 70 at 5.2 DU (横扇) | 横広 |

### 2.2 ヨコ振りの飛沫構造 (Splatoon Roller "WideSwing")

ローラーのヨコ振りは多段のユニットで構成 — `WideSwingUnitGroupParam.Unit[]`:
- Unit[0]: 主弾10本 (Wide=18°ファン, Speed=1.05DU/F) + 中心軸の高ダメージ判定
- Unit[1]: 副弾2本 (狭ファン)
- Unit[2]: ヴァリアブル等のみ拡張ユニット

衝突半径 (Inside) と扇外判定 (Outside) は別:
- スプラローラー Inside: 最大1500(150)/4.2DU, 最小350(35)/10.7DU, 角度15°
- スプラローラー Outside: 最大1000(100)/2.0DU, 最小350/9.2DU, 角度20°

### 2.3 タテ振りの飛沫

`VerticalSwingUnitGroupParam.SpawnSplashNum` 個の飛沫を一直線に間隔 `SpawnSplashBetweenLength` で配置:
- スプラ: 5発, 間隔4.6DU, 初期距離1.2DU = 最大18.6DU届く
- ダイナモ: 6発, 間隔5.2DU = 最大26.2DU届く
- カーボン: 5発, 間隔4.2DU

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponRollerNormal.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponRollerHeavy.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponRollerCompact.game__GameParameterTable.json

---

## 3. チャージャー (Charger) — 全7種

> 共通スキーマ:
> - `ChargeFrameFullCharge` — フルチャージ所要F、`ChargeFrame_First/Second` — 1/2段階
> - `ValueFullCharge/MaxCharge/MinCharge` — 内部ダメージ。100以上で 確殺(=実HP100)
> - `DistanceFullCharge/MaxCharge/MinCharge` — 射程 (DU)
> - `InkConsumeFullCharge/MinCharge` — 消費
> - `KeepChargeFullFrame` — フルチャージ保持可能F (イカ潜伏に持ち込み可)

### 3.1 チャージャー数値表

| 内部名 | 武器 | 無チャージダメ | フルチャダメ | 射程 (DU) | フルチャF | 最短チャF | フルチャ消費 | チャージ保持F | 貫通 |
|--------|------|------------|------------|----------|----------|----------|------------|--------------|-----|
| WeaponChargerQuick | スクイックリン | 40 (400) | 140 (1400) | 16.765 | 45F | 8F | 10.5% | (なし) | × |
| WeaponChargerNormal | スプラチャージャー | 40 (400) | 160 (1600) | 24.037 | 75F | (即) | 18.0% | 不明 (要確認) | ○ |
| WeaponChargerNormalScope | スプラスコープ | 40 | 160 | (Normal+) | (Normal+) | — | — | — | ○ |
| WeaponChargerLong | リッター4K | 40 | 180 (1800) | 29.05 | 92F | — | 25.0% | — | ○ |
| WeaponChargerLongScope | 4Kスコープ | 40 | 180 | (Long+) | (Long+) | — | — | — | ○ |
| WeaponChargerLight | 14式竹筒銃・甲 | 30 (300) | 85 (850) | 19.564 | 20F + 1F固定 | 1F | 7.0% | (連射型) | △ |
| WeaponChargerKeeper | ソイチューバー | 40 | 180 (1800) | 19.804 (full) / 18.8 / 12.6 | 71F (full) / 50F (mid) | — | 15.0% | (Keeper=保持◎) | ○ |
| WeaponChargerPencil | R-PEN/5H | 40 | 68 (680) ×5発 | 26.037 | 72F | 8F | 35.0% | 各発0.0197 | × |

### 3.2 チャージ-威力-射程の補間

- 全チャージャー: `Min → Max → Full` の3段階でダメージ・射程を線形補間
- 例: スプラチャージャー → MinCharge=9.033DU/40 → MaxCharge=24.037DU/80 → FullCharge=24.037DU/160
- `JumpHeightFullCharge` (ジャンプ撃ち補正), `KeepChargeFullFrame` (フルチャ保持)
- スプラチャージャーは `KeepChargeFullFrame=75F` (1.25秒イカ保持可能) — ソイチューバーはさらに長い

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponChargerNormal.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponChargerLong.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponChargerLight.game__GameParameterTable.json (Bamboozler)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponChargerQuick.game__GameParameterTable.json (Squiffer)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponChargerKeeper.game__GameParameterTable.json (Goo Tuber)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponChargerPencil.game__GameParameterTable.json (R-PEN/5H)

---

## 4. スロッシャー (Slosher) — 全6種

> 共通スキーマ: 放物線弾道。`Unit[]` で複数の弾を1キャストに配置 (バケットスロッシャーは5発の連弾)。
> - `BlastParam` / `DistanceDamage` — 距離別ダメージ (近 1800 / 遠 300 が代表)
> - `BurstFrame` — 起爆遅延F (爆発系のみ)
> - `RepeatFrame` — 連投間隔F
> - `BaseDistance` (MainEffectiveRangeUp) — 効果射程基準値

### 4.1 スロッシャー数値表

| 内部名 | 武器 | 主弾ダメ | 連弾構成 | 射程 (BaseDist) | 連射F | インク消費 | 重力 / 空気抵抗 |
|--------|------|---------|---------|----------------|------|----------|----------------|
| WeaponSlosherStrong | バケットスロッシャー | 70 (700→500) | 4+5の縦並び弾 | 14.0 DU | 29F | 7.6% | 0.05 / 12% |
| WeaponSlosherDiffusion | ヒッセン | 350/620 max (3列扇) | 計9発 (4+3+2) | 11.5 DU | 23F | 6.0% | (一般) |
| WeaponSlosherDouble | スクリュースロッシャー | 直撃48 + 渦42 (8F遅れ) | 主+副8F遅延 | 14.0 DU | 45F | 9.0% | (一般) |
| WeaponSlosherLauncher | エクスプロッシャー | 30→52 (76 max) ×爆風2回 | 直線投擲、地面爆発 | 14.5 DU | 38F (+10F post) | 9.2% | (一般) |
| WeaponSlosherWashtub | オーバーフロッシャー | 55 (550) ×複数バウンド | 1弾, バウンド | 20.7 DU | (バウンド型) | — | 落下半径4.5DU |
| WeaponSlosherBathtub | モップリン (新) | 32 + 副弾複数 | 1主+3副(5F遅) | 14.0 DU | 32F (+15F post) | 8.0% | バウンド型 |

備考:
- 1キャストにつき複数のユニットを段階的に発射する点が特徴 (バケット5連、ヒッセン9発扇など)
- `SpawnSpeedBase` 別: バケット主弾 1.79 DU/F, 副弾 1.5-1.0 DU/F
- 爆発系 (エクスプロッシャー) は地面着弾時に2段階の半径で爆発

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSlosherStrong.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSlosherDiffusion.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSlosherDouble.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSlosherLauncher.game__GameParameterTable.json (Explosher)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSlosherWashtub.game__GameParameterTable.json (Bloblobber)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSlosherBathtub.game__GameParameterTable.json

---

## 5. スピナー (Splatling) — 全6種

> 共通スキーマ:
> - `ChargeFrame_First / ChargeFrame_Second` — 1段階 / フル チャージF
> - `MaxShootingFrame_First / Second` — 1/2段階 ぶんの 発射継続F
> - `RepeatFrame` — 弾と弾の間隔F (3F or 4F が典型)
> - `ValueMin/Max/FullChargeMax` — ダメージ3段
> - `InkConsume` — 1発の消費
> - `KeepChargeFullFrame` — フルチャ保持F (ノーチラスのみ大きい)

### 5.1 スピナー数値表

| 内部名 | 武器 | 1段チャF | 2段チャF | 1段発射F | 2段発射F | 連射間隔F | 1発ダメ | フルチャダメ | 1発消費 | 推定射程 (DU) |
|--------|------|---------|---------|---------|---------|---------|--------|------------|--------|-------------|
| WeaponSpinnerStandard | スプラスピナー | 48F | 72F | 80F | 160F | 4F | 30 (300) | 30 (300) | 2.25% | 中 (≈13DU) |
| WeaponSpinnerHyper | バレルスピナー | 120F | 150F | 130F | 260F | 4F | 32 (320) | 40 (400) | — | 長 (≈18DU) |
| WeaponSpinnerHyperShort | ミニスピナー | 60F | 120F | 120F | 240F | 4F | 26 (260) | 26 (260) | 20.0% | 中短 |
| WeaponSpinnerDownpour | ハイドラント | 1F | 100F | 1F (1発) | 200F | 3F | 28 (280) | 28 (280) | 25.0% | 最長 (≈22DU) |
| WeaponSpinnerQuick | ノーチラス | 18F | 27F | 42F | 84F | 4F | 32 (320) | 32 (320) | 15.0% | 中 |
| WeaponSpinnerSerein | クーゲルシュライバー/イグザミナー | 38F+150F | 76F | 60F | 120F | — (2モード) | 32 (320) | 40 (400) | — | 切替式 |

備考:
- ノーチラスは `KeepChargeFullFrame` 大 = チャージ後にイカ潜伏可能
- クーゲルは2モード切替 (近距離高威力 / 遠距離精密)
- ハイドラントは「1F空チャージ」が連射に直結

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpinnerStandard.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpinnerHyper.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpinnerDownpour.game__GameParameterTable.json

---

## 6. マニューバー (Dualies) — 全6種

> 共通スキーマ:
> - 通常射撃 + スライド (`SideStepXxx`)
> - `MoveDist` — スライド距離 (DU)、`MoveFrame` — スライド持続F (= 無敵F相当)
> - スライド後 `UnrelaxFrameMove` フレーム間は移動コマンド不可
> - スライド後の射撃は `Stand_DegSwerve` が変化 (精度UP)

### 6.1 マニューバー数値表

| 内部名 | 武器 | ダメージ | 連射F | 通常射ブレ° | スライド距離 | スライドF | スライド消費 | スライド後精度 | スライド回数 |
|--------|------|---------|------|------------|------------|----------|-----------|-------------|------------|
| WeaponManeuverShort | スパッタリー | 36/18 | 5F | (12+) | 3.0 DU | 8F | 5.0% | 2.0° | 2回 |
| WeaponManeuverNormal | スプラマニューバー | 30/15 | 5F | (8+) | 3.5 DU (推定) | 12F | 7.0% | 2.0° | 2回 |
| WeaponManeuverGallon | ケルビン525 | 36/18 → 36 (slide後+) | 7F | — | 1.2 DU (短) | 40F (重) | 8.0% | 1.5° (大幅UP) | 2回 |
| WeaponManeuverDual | デュアルスイーパー | 28/14 | 7F | — | 3.5 DU | 12F | 8.0% | — | 2回 |
| WeaponManeuverStepper | クアッドホッパー | 28/14 | 5F | — | 3.5 DU × 4 | 5F (短) | 3.0% | — | **4回** |
| WeaponManeuverLong | ガエンFF | 25.5/12.8 | 9-17F (短) | — | 5.5 DU (最長) | 16F | — | — | 2回 |

備考:
- ケルビン525はスライド後の射撃が確2火力 (52.5+) になる仕様 (`Stand_DegSwerve` 縮小+ダメージ別補正)
- クアッドホッパーは4スライド連発 + 1回毎の硬直が短い
- スライド中は無敵フレーム相当 (Inkipediaでは「移動回避F」と表記)

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponManeuverNormal.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponManeuverGallon.game__GameParameterTable.json (Glooga)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponManeuverStepper.game__GameParameterTable.json (Tetra)

---

## 7. シェルター (Brella) — 全4種

> 共通スキーマ:
> - 1ショットに複数ペレット (CanopySpread系)
> - `CanopyHP` — 傘耐久HP、`CanopyInitSpeed/CanopyGravity` — パージ初速・重力
> - `CanopyNakedFrame` — 展開状態維持F
> - `InkConsume` (撃つたび) と展開コスト

### 7.1 シェルター数値表

| 内部名 | 武器 | ペレット数 | 1発ダメ (内部) | 傘HP | パージ初速 | パージ重力 | 撃つ消費 | 展開消費 | 展開F |
|--------|------|----------|--------------|------|----------|----------|--------|--------|-----|
| WeaponShelterNormal | パラシェルター | 11 (1中+6+4) | 108-162 (内部1080-1620) | (不明、約2000?) | 7.92 DU/F | 0.75 | 5.0% | 30% | 330F |
| WeaponShelterCompact | スパイガジェット | 7 (1+2+4) | 90-120 | 2000 | (パージなし) | — | 4.0% | — | — |
| WeaponShelterWide | キャンピングシェルター | 13 (1+6+6) | 170 | 7000 (最大) | (パージなし→交換) | — | 11.0% | — | — |
| WeaponShelterFocus | 24式張替傘・甲 | 11 (1+6+4) | 100-150 | 1500 (リジェネ5HP/F) | 9.0 DU/F | 0.72 air/0.48 ground | 11.0% | — | 170F |

備考:
- 24式は自己回復付き
- パージは「傘を前方に飛ばす」攻撃 — パラシェルター/24式のみ持つ
- スパイガジェットはパージなしの代わりに低消費・高機動
- キャンピングシェルターは「展開→味方再使用可能」な巨大傘で耐久最大

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponShelterNormal.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponShelterCompact.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponShelterWide.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponShelterFocus.game__GameParameterTable.json

---

## 8. ブラスター (Blaster) — 全7種

> 共通スキーマ:
> - `DamageParam.ValueMax/Min` — 直撃 (Direct) 1250 = 125ダメ
> - `BlastParam.DistanceDamage.DamageNear/Far` + `Distance` — 爆風近 (例 700@1.0DU) / 爆風遠 (350@3.5DU)
> - `BurstFrame` — 時限爆発F (発射→爆発)
> - `GoStraightToBrakeStateFrame` — 直進F (これを超えると失速)
> - `RepeatFrame` — 連射間隔F
> - 爆風半径 = `KnockBackParam.Distance` (≒物理範囲)、塗り半径 = `PaintRadius`

### 8.1 ブラスター数値表

| 内部名 | 武器 | 直撃 | 爆風近 | 爆風遠 | 爆風半径 | 起爆F | 連射F | 消費 |
|--------|------|------|--------|--------|---------|------|------|------|
| WeaponBlasterShort | ノヴァブラスター | 125 (1250) | 70@1.8 | 50@4.0 | 4.0 DU | 11F | 40F | 7.5% |
| WeaponBlasterMiddle | ホットブラスター | 125 | 70@1.11 | 50@3.47 | 3.5 DU | 9F (=Straight→Brake) | 50F | 10.0% |
| WeaponBlasterLong | ロングブラスター | 125 | 70@1.01 | 50@3.37 | 3.45 DU | 15F | 60F | 11.0% |
| WeaponBlasterLight | ラピッドブラスター | 85 (850) | 35@(近) | 35@3.37 | 3.4 DU | (Brake=遅延) | 35F | 7.0% |
| WeaponBlasterLightLong | Rブラスターエリート | 85 | 35 | 35 | 3.35 DU | 11F | (>35F) | 9.12% |
| WeaponBlasterLightShort | クラッシュブラスター | 60 (600) | 30@1.17 | 30@4.17 | 4.2 DU | (短) | 20F (最速) | 4.0% |
| WeaponBlasterPrecision | S-BLAST92 | 125 | (2モード) | (2モード) | 1.0DU (通常) / 2.5DU (ジャンプ) | 7F (Straight) | (長め) | — |

備考:
- 一般的に「爆風50ダメ」のホット・ノヴァ・ロングは確2が爆風2発で取れる
- ラピッド系は直撃85ダメで「3発当てで確殺」型
- S-BLAST92はジャンプ撃ちで弾速・塗りが拡大する特殊仕様

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBlasterShort.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBlasterMiddle.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBlasterLong.game__GameParameterTable.json
- https://splatoonwiki.org/wiki/Blaster

---

## 9. フデ (Brush) — 全3種

> 共通スキーマ:
> - `BodyParam.PaintParam.SpeedMax` — フデダッシュ最高速度 (DU/F)
> - `SwingFrame` — 振り出しF (1F=最速)、`InkConsume` — 振り消費
> - 飛沫は `Unit[]` で複数本飛び、`SpawnWideDegree` で扇

### 9.1 フデ数値表

| 内部名 | 武器 | 振りダメ (Max) | 振りF | 振り消費 | ダッシュ速度 | 飛沫本数 | 飛沫扇度 |
|--------|------|--------------|------|--------|------------|--------|---------|
| WeaponBrushMini | パブロ | 30/15 (300) | 1F | 2.0% | 0.198 DU/F (最速) | 2+1 | 24°(主) |
| WeaponBrushNormal | ホクサイ | 40/20 (400) | 1F | 2.7% | 0.132 DU/F | 3+2 | 24°(主) |
| WeaponBrushHeavy | フィンセント | 60/25 (600→250) | 23F | 4.8% | (中速) | 1+2×多 | 0-12° |

備考:
- パブロはダッシュインク消費 0.00125/F (約400F = 6.6秒) なので塗り効率非常に高い
- フィンセントは曲射型 (上方向に発射) — `SpawnRotateXDegree` が大きい
- 振り出し時の塗り判定 = `PaintParam.WidthHalfMax`

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBrushMini.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBrushNormal.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBrushHeavy.game__GameParameterTable.json

---

## 10. ストリンガー (Stringer) — 全3種

> 共通スキーマ:
> - 通常射: 3矢同時 (`SpawnWideDegree` で扇)
> - 溜め射: 直撃+時限爆発
> - `ChargeFrameMin/Mid/Max` — 3段階チャージF
> - `BurstFrame` — 着弾後の起爆遅延 (= フルチャ時)

### 10.1 ストリンガー数値表

| 内部名 | 武器 | 矢ダメ Max | 矢ダメ Min | 爆発ダメ | フルチャF | 最短チャF | 射程 (DU) | フル消費 | 最低消費 |
|--------|------|----------|----------|---------|----------|----------|----------|---------|---------|
| WeaponStringerNormal | トライストリンガー | 35 (350) | 30 (300) | 30 (300) | 72F | 9F | 13.075 | 8.5% | 5.0% |
| WeaponStringerExplosion | LACT-450 | 35 | 24 (240) | 30 | 80F | 12F | 13.075 | 9.0% | 7.0% |
| WeaponStringerShort | フルイドV / Wellstring V | 45 (450) | 30 | 28 | 34F (短) | 6F | 13.075 | 6.5% | 3.5% |

備考:
- 全ストリンガーの最大射程は `MaxLen=13.075` で固定 (同射程内で性能差別化)
- 爆発フューズ: 45F (トライストリンガー) で時限爆発 (フルチャ時)
- 3矢の角度: 通常射時は約20°ファン

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponStringerNormal.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponStringerExplosion.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponStringerShort.game__GameParameterTable.json

---

## 11. ワイパー (Splatana / Saber) — 全3種

> 共通スキーマ:
> - ヨコ斬り (`HorizontalParam`) と タメ斬り (`VerticalParam`)
> - `ChargeFrameMin / ChargeFrameFull` — タメ斬りチャージF
> - 衝撃波弾は `BulletSaber*` 型で飛ぶ (直線・限界距離あり)

### 11.1 ワイパー数値表

| 内部名 | 武器 | ヨコ斬りダメ | タメ斬りダメ | タメ最短F | タメフルF | ヨコ消費 | タメ消費 | 衝撃波塗り (HalfWidth) |
|--------|------|------------|------------|----------|----------|---------|---------|----------------------|
| WeaponSaberLite | ジムワイパー | 30 (300) | 60? (直撃) | 5F | 14F (短) | 3.5% | 6.0% | 2.3 DU |
| WeaponSaberNormal | ドライブワイパー | 20 (200) | 140 (1400) | 5F | 20F | 5.2% | 11.7% | (Horizontal 1.6×0.25×1.4 / Vertical 0.5×1.4×0.7) |
| WeaponSaberHeavy | デンタルワイパー | 30 (300) → 40 (HitDamage) | 160 (1600) → 80 (HitDamage) | 5F | 20F | 3.5% | 13.0% | 2.6 DU |

備考:
- ヨコ斬りは「斬撃 (近) + 衝撃波 (中距離)」の2段判定
- タメ斬りは大ダメージ + 突進移動付き — フル溜めで確殺
- デンタルワイパーは衝撃波の塗り3.0DU (最広)

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSaberNormal.game__GameParameterTable.json (Stamper)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSaberLite.game__GameParameterTable.json (Wiper)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSaberHeavy.game__GameParameterTable.json (Decavitator)

---

## 12. サブウェポン (全14種)

| 内部名 | 武器 | 直撃 | 爆風近 | 爆風遠 | 爆風半径 | 起爆F | インク消費 | 塗り半径 |
|--------|------|------|--------|--------|----------|------|----------|---------|
| WeaponBombSplash | スプラッシュボム | — | 180 (@3.6DU) | 30 (@7.0DU) | 12.0 DU (KB) | 60F | 70% (推定 from InkRecoverStop:60) | 1.064 DU + 飛沫15 |
| WeaponBombSuction | キューバンボム | — | 180 (@4.6DU) | 30 (@8.0DU) | 12.0 DU | (壁吸着後60F) | 70% | 5.0 DU |
| WeaponBombQuick | クイックボム | 35 (350@2.8DU) | 25 (250@4.0DU) | — | 4.0 DU | 0F (即) | 45% | 1.4 DU (cross) + 飛沫10 |
| WeaponBombCurling | カーリングボム | 200 (接触@0.67s) | 180 / 30 (fullチャ) | — | 4.6→8.0 DU | 60F max charge | 65% | 進路塗り |
| WeaponBombRobot | ロボットボム | — | 180 (@2.85) | 30 (@6.5) | (追尾) | 自走→着地 | 55% | 1.75 cross + 0.7 around |
| WeaponBombFizzy | タンサンボム | — | 50×3段 | — | 段階拡大 | 0/40/80F | 60% | 3.0→3.8 DU |
| WeaponBombTorpedo | トーピード | — | 60 (600@2.6) → 35 (@6.0) | 12 (副弾) | (空中追尾) | 接近時起爆 | 65% | 3.5 DU |
| WeaponLineMarker | ラインマーカー | 40 (400) | — | — | (跳ね回り) | — | 40% | 跳ね 1.0率 |
| WeaponBeacon | ジャンプビーコン | — | — | — | — | — | 75% | 設置 (最大3個) |
| WeaponPointSensor | ポイントセンサー | — (マーキングのみ) | — | — | 6.0 DU 半径 | — | 45% | 480-960F マーキング |
| WeaponPoisonMist | ポイズンミスト | — (移動低下 + インク減) | — | — | 5.4 DU | — | 55% | 段階 60/30/15F インターバル |
| WeaponSprinkler | スプリンクラー (推測内部 `WeaponSprinkler`) | — | — | — | — | — | 60% | 3段階稼働、HP120 |
| WeaponShield | スプラッシュシールド | 60 (接触ダメ+ノックバック) | — | — | — | — | 60% | HP 800, 持続2.67秒前 不可 |
| WeaponMine | トラップ | 45 (直撃) + 35 (爆風) | — | — | (接近検知) | — | 60% | 2個設置 + マーキング |

備考:
- 「サブ性能アップ」ギアパワーで威力・距離が拡張される (`SubInkSaveLv`)
- インク消費は全サブで `InkRecoverStop` フレームの直後まで回復しない
- スプラ・キューバンの直撃180は確殺 (実HP100以上)

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBombSplash.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBombSuction.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBombQuick.game__GameParameterTable.json (Burst)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBombCurling.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBombRobot.game__GameParameterTable.json (Auto-Bomb)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBombFizzy.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponBombTorpedo.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponLineMarker.game__GameParameterTable.json (Angle Shooter)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponPointSensor.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponPoisonMist.game__GameParameterTable.json (Toxic Mist)
- https://splatoonwiki.org/wiki/Sprinkler (HP120, 3-phase)
- https://splatoonwiki.org/wiki/Splash_Wall (HP800)
- https://splatoonwiki.org/wiki/Ink_Mine (2個まで)

---

## 13. スペシャルウェポン (全19種)

> 共通スキーマ:
> - `SpecialDurationFrame.Low/Mid/High` — スペシャル性能アップで変動 (基本Mid)
> - 大半はWeaponSp*プレフィックス
> - 攻撃系は `BlastParam.DistanceDamage` の遠近2段
> - SP発動に必要pt = `WeaponInfoMain.json` の `Special Points` (180-220pt範囲)

### 13.1 攻撃系スペシャル

| 内部名 | 武器名 | 効果 | 主ダメージ | 補助ダメ | 持続F (Mid) | 半径/距離 |
|--------|--------|------|-----------|---------|----------|---------|
| WeaponSpUltraShot | ウルトラショット (Trizooka) | 螺旋3発 | 220 直撃 | 53/35 爆風 (@2.5/4.0) | 405F | 3.2 DU 塗り |
| WeaponSpMultiMissile | マルチミサイル (Tenta Missiles) | 複数ロック誘導 | 150 (@1.1) → 30 (@4.25) | — | (発射まで) | 10〜発射 |
| WeaponSpInkStorm | アメフラシ (Ink Storm) | 雨雲設置 | 継続 (raindrop) | — | 540F | 10.0 DU 雲半径 |
| WeaponSpNiceBall | ナイスダマ (Booyah Bomb) | チャージ→爆撃 | 380直撃 / 300爆風 (要確認) | アーマー 4700HP | 充電可変 | 12.6 DU 爆風 |
| WeaponSpMicroLaser | メガホンレーザー5.1ch | 6個追尾レーザー | 35×N段 (5F毎) | — | 180-240F | 80°ロック |
| WeaponSpUltraStamp | ウルトラハンコ | 振り+投擲 | 振り200 (1000@4.0) / 投げ300 (1000@8.0) | — | 570F | 4.0/8.0 DU |
| WeaponSpTripleTornado | トリプルトルネード | 3点トルネード | 75持続 | KB 8.0 | 360F | 7.7 DU x 高さ20 |
| WeaponSpSkewer | サメライド (Reefslider) | 突撃→爆発 | 220 (@近) / 70 (@遠) | — | 38+54F | 7.5-9.0 DU 塗り |
| WeaponSpGachihoko | テイオウイカ (Kraken Royale)? | ホコ撃ち (チャージャー型) | 180 (フル直撃) / 40 (遠) | — | — | 4.6 DU |
| WeaponSpSuperLanding | ウルトラチャクチ (Splashdown) | 飛上→爆撃 | 180 (1800@7.0) / 70 (@10) / 55 (@14) | — | (即着地) | 13.0 DU 塗り |

### 13.2 機動・変身系

| 内部名 | 武器名 | 効果 | 持続F | 主ダメ | 備考 |
|--------|--------|------|------|------|------|
| WeaponSpPogo | ショクワンダー (Zipcaster) | ワイヤー伸縮 | (Mid基準) | (撃ち各メイン) | 最大35DU |
| WeaponSpJetpack | ジェットパック (Inkjet) | 浮遊+爆撃 | 480F | 直撃120 / 爆風50 / 30 | 3.2DU 爆風 / 100DU Y上限 |

### 13.3 支援・防御系

| 内部名 | 武器名 | 効果 | HP/持続 | 半径 |
|--------|--------|------|--------|------|
| WeaponSpGreatBarrier | グレートバリア (Big Bubbler) | 大型バリア | HP 1536 (実HP) / 30720 (内部) / 持続不明 | 2.255-7.5 DU |
| WeaponSpShockSonar | ホップソナー (Wave Breaker) | 3波動 + マーキング | 3波 (90, 240, 390F発射) | 24.0 DU (Mid) |
| WeaponSpBlower | キューインキ (Ink Vac) | 吸収 → 放出 | 15 DU 吸収距離 | 3.8 DU 半径 (Mid) |
| WeaponSpChariot | カニタンク (Crab Tank) | 操作可能カニ | HP 5000 / 570F (Mid) | ガトリング+大砲 |
| WeaponSpEnergyStand | エナジースタンド (Tacticooler) | 味方バフ | 17秒 / 4本 | (味方接触で取る) |
| WeaponSpSuperHook | デコイチラシ (Super Chump) | 偽の的散布 | 着地後3.5秒で爆発 | — |
| WeaponSpCastle | スプラッタカラースクリーン (Splattercolor Screen) | 視覚妨害 + ダメ | 480/540/600F (Lo/Mid/Hi) | ヒトカラーAU=4F塗り |
| WeaponSpFirework | (旧スペシャル/未使用?) | — | 635F | 3.5 DU 塗り |
| WeaponSpChimney | (新スペシャル?) | — | (560F進行) | 15.0×4.5 壁構造 |

備考:
- 持続Fは Lo/Mid/Hi の3段階で「スペシャル性能アップ」ギアパワーレベル別
- 内部HPは表示HPの10倍 (例: Big Bubbler 1536 表示 = 15360 内部)
- ナイスダマアーマー: 4700HP (内部)

出典:
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpUltraShot.game__GameParameterTable.json (Trizooka)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpMultiMissile.game__GameParameterTable.json (Tenta Missiles)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpInkStorm.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpNiceBall.game__GameParameterTable.json (Booyah)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpMicroLaser.game__GameParameterTable.json (Killer Wail 5.1)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpUltraStamp.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpTripleTornado.game__GameParameterTable.json
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpSkewer.game__GameParameterTable.json (Reefslider)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpSuperLanding.game__GameParameterTable.json (Splashdown)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpJetpack.game__GameParameterTable.json (Inkjet)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpGreatBarrier.game__GameParameterTable.json (Big Bubbler)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpShockSonar.game__GameParameterTable.json (Wave Breaker)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpBlower.game__GameParameterTable.json (Ink Vac)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpChariot.game__GameParameterTable.json (Crab Tank)
- https://github.com/Leanny/splat3/blob/main/data/parameter/1110/weapon/WeaponSpEnergyStand.game__GameParameterTable.json (Tacticooler)
- https://splatoonwiki.org/wiki/Tacticooler (4本/17秒)
- https://splatoonwiki.org/wiki/Super_Chump (3.5秒爆発)

---

## 14. ヒットボックス・判定詳細

### 14.1 プレイヤーカプセル

| 状態 | 形状 | 半径 (R) | 高さ (H) | 出典 |
|------|-----|---------|---------|------|
| ヒト形態 (humanoid) | カプセル | 0.7 DU | 1.65 DU | Squidboards / Inkipedia |
| イカ形態 (squid) | 球体 | 0.675 DU | (球体) | v11.0.0以降 |
| イカ→ヒト遷移 | (補間) | 0.675 → 0.7 | — | — |

### 14.2 弾の対プレイヤー判定

各シューター弾は `CollisionParam` で:
- `InitRadiusForPlayer` — 発射時の対プレイヤー判定半径
- `EndRadiusForPlayer` — 遠距離での判定半径
- `ChangeFrameForField` — フィールド判定切替F

例: スプラシューター `InitRadiusForPlayer=0.285 / EndRadiusForPlayer=0.285` — 一定半径28.5cm

### 14.3 弾の対床判定

- `InitRadiusForField=0.2 / EndRadiusForField=0.2` — 床塗り判定半径20cm
- 落下塗り (WallDropCollisionPaintParam) — `PaintRadiusFall=0.65 / PaintRadiusGround=0.6 / PaintRadiusShock=1.56`
- 着弾飛沫 (`SplashPaintParam.WidthHalfNearest`) — 約2.0 DU

### 14.4 壁面塗装

`PaintParam.HeightUseDepthScaleMaxBreakFree=1.5 / HeightUseDepthScaleMinBreakFree=10.0` — 壁の高さ1.5〜10mで塗り深度が変化。

### 14.5 ダメージ判定の遠近

ダメージは `ValueMax → ValueMin` で `ReduceStartFrame → ReduceEndFrame` 間に線形補間。
- 例: スプラシューター 36@F0-F8 → 線形 → 18@F40
- 内部式: `damage = ValueMax - (ValueMax - ValueMin) × (current_frame - ReduceStart) / (ReduceEnd - ReduceStart)` (frame > ReduceStart の場合)

---

## 15. インクカラー・チームカラー

### 15.1 シリーズシグナチャ

| シリーズ | デフォルトα | デフォルトβ |
|---------|-----------|------------|
| Splatoon 1 | オレンジ | 青紫 |
| Splatoon 2 | 黄緑 | ピンク |
| Splatoon 3 | **黄色** | **青紫** |

### 15.2 通常マッチ ペアリスト (Inkipedia/Fandom より)

代表的なバトルカラー組:
- 黄色 vs 青紫 (default)
- 緑 vs 紫
- ライム vs 紫
- オレンジ vs 青
- オレンジ vs 紫
- ピンク vs 緑
- ターコイズ vs ピンク
- ターコイズ vs 赤 (S3新規)
- 黄 vs 青
- 黄 vs 紫
- マスターメロン (赤ピンク) vs ? — S3新規

### 15.3 カラーロック機能 (アクセシビリティ)

「カラーロック」をオンにすると、識別しやすい一定の組合せに固定:
- 黄色 vs ダーク紫
- 紫 vs 緑
- 青 vs ダーク緑
- 黄 vs マゼンタ
- ピンク vs ターコイズ

色覚特性に配慮した対応。Splatoon 3 ではメニュー → オプションから設定可能。

### 15.4 既知の Hex (代表値, 視認用近似)

| 色 | HEX (近似) | RGB |
|----|-----------|-----|
| シグナチャ黄色 (Yellow) | #FFFC00 / #DCE000 | 255,252,0 |
| シグナチャ青紫 (Purple) | #6844E6 | 104,68,230 |
| オレンジ | #F2630F | 242,99,15 |
| 青 | #2031D2 | 32,49,210 |
| 緑 | #00B45D | 0,180,93 |
| ピンク | #E30270 | 227,2,112 |
| ターコイズ | #07B2A9 | 7,178,169 |
| マスターメロン | #FF002B | 255,0,43 |

出典:
- https://splatoonwiki.org/wiki/Ink/Splatoon_3
- https://splatoon.fandom.com/wiki/Ink
- https://www.colourlovers.com/color/6844E6/Splatoon_3_purple
- https://www.deviantart.com/sheepman5003/art/Splatoon-1-3-Ink-Colors-947819410

---

## 16. Unity ScriptableObject 設計指針

### 16.1 推奨クラス階層

```
WeaponData (abstract ScriptableObject)
├─ ShooterData
│   ├─ float spawnSpeed  // DU/F
│   ├─ int goStraightFrames
│   ├─ float brakeStateMaxSpeed
│   ├─ float freeGravity = 0.016f
│   ├─ float brakeGravity = 0.07f
│   ├─ float damageMax  // /10 で表示値
│   ├─ float damageMin
│   ├─ int reduceStartFrame
│   ├─ int reduceEndFrame
│   ├─ float inkConsume
│   ├─ int repeatFrame
│   ├─ float standSwerveDeg
│   ├─ float jumpSwerveDeg
│   ├─ float moveSpeed   // 撃ち歩き DU/F
│   └─ PaintParams paintData
├─ RollerData
│   ├─ float bodyDamage
│   ├─ float dashSpeed, normalSpeed
│   ├─ int verticalSwingFrame, wideSwingFrame
│   ├─ List<SwingUnit> verticalUnits
│   └─ List<SwingUnit> wideUnits
├─ ChargerData
│   ├─ int chargeFrameFull
│   ├─ float damageFull, damageMax, damageMin
│   ├─ float distanceFull
│   ├─ float keepChargeFullFrame
│   ├─ bool penetrate
│   └─ float jumpHeightFullCharge
├─ SplatlingData / SlosherData / ManeuverData / ...
├─ SubWeaponData (abstract)
└─ SpecialWeaponData (abstract)
```

### 16.2 単位変換ルール

- **DU → Unity unit**: 1:1 推奨 (1 DU = 1 m)
- **フレーム → 秒**: Unityの `Time.fixedDeltaTime` を 1/60 にして同期 (`Project Settings > Time > Fixed Timestep = 0.01667`)
- **ダメージ内部値**: そのまま使用、表示時のみ `/10`

### 16.3 物理シミュレーション

```csharp
// 弾の3状態モデル
public class BulletPhysics
{
    enum State { Straight, Brake, Free }
    State _state = State.Straight;
    int _framesAlive = 0;
    Vector3 _velocity;
    ShooterData _data;

    public void FixedUpdate()
    {
        switch (_state)
        {
            case State.Straight:
                if (_framesAlive >= _data.goStraightFrames)
                {
                    _velocity = Vector3.ClampMagnitude(_velocity, _data.brakeStateMaxSpeed);
                    _state = State.Brake;
                }
                break;
            case State.Brake:
                _velocity *= (1f - 0.36f);              // 36%空気抵抗
                _velocity.y -= _data.brakeGravity;       // 重力0.07
                if (_velocity.y < -0.15f) _state = State.Free;
                break;
            case State.Free:
                _velocity *= (1f - 0.02f);              // 2%空気抵抗
                _velocity.y -= _data.freeGravity;        // 重力0.016
                break;
        }
        transform.position += _velocity;  // 1F=1 fixedDeltaTime
        _framesAlive++;
    }

    public float GetCurrentDamage()
    {
        if (_framesAlive <= _data.reduceStartFrame) return _data.damageMax;
        if (_framesAlive >= _data.reduceEndFrame) return _data.damageMin;
        float t = (float)(_framesAlive - _data.reduceStartFrame) / (_data.reduceEndFrame - _data.reduceStartFrame);
        return Mathf.Lerp(_data.damageMax, _data.damageMin, t);
    }
}
```

---

## 17. 主要参考資料・URL集

### 17.1 一次データソース (生パラメータ)

- **Leanny/splat3 (GitHub):** https://github.com/Leanny/splat3 — 任天堂内部GameParameterTable JSON群 (本DBの根幹)
- **Leanny Database (Web UI):** https://leanny.github.io/splat3/database.html
- **Leanny Parameter Database:** https://leanny.github.io/splat3/parameters.html
- **Damage Multipliers:** https://leanny.github.io/splat3/damagetable.html

### 17.2 解析・解説

- **Inkipedia 主要ページ:**
  - 武器個別: https://splatoonwiki.org/wiki/Splattershot, https://splatoonwiki.org/wiki/Splat_Roller, など
  - メイン武器データ集約: https://splatoonwiki.org/wiki/Main_weapon_data
  - スペシャル武器: https://splatoonwiki.org/wiki/Special_weapon_data
  - ダメージ仕様: https://splatoonwiki.org/wiki/Damage
  - XarrotDデータ解説: https://splatoonwiki.org/wiki/User:XarrotD/Data_Explanation

- **Squidboards (競技勢解析):** https://squidboards.com/
  - ヒットボックス: https://squidboards.com/threads/hitbox-sizes.2235/
  - ヘルス回復: https://squidboards.com/threads/health-regeneration-enemy-ink-damage-mechanics.2688/

- **sendou.ink (ビルドツール + 計算機):**
  - ビルド: https://sendou.ink/builds
  - 武器アナライザー: https://sendou.ink/analyzer
  - オブジェクトダメージ計算: https://sendou.ink/object-damage-calculator

- **Game8 (各武器のスタッツガイド):**
  - スプラシューター: https://game8.co/games/Splatoon-3/archives/387518
  - わかば: https://game8.co/games/Splatoon-3/archives/387513
  - (全武器個別ガイドあり)

- **計算機:** https://calc.splatoon.ink/

### 17.3 ハッキング・モディング

- **Awesome-Splatoon3-Hacking:** https://github.com/DesperC/Awesome-Splatoon3-Hacking
- **SplatHeX (Splatoon 2 datamine):** https://github.com/MirayXS/SplatHeX

### 17.4 動画解析

- ProChara (YouTube): 武器個別の競技目線解析
- ThatSrb2DUDE (YouTube): フレームデータ・物理解析
- VFX Mike: インク塗装シェーダ技術解析

---

## 付録A: 内部名 → 公式名対応表

### A.1 メイン武器

| 内部名 | 公式名 (JP) | 公式名 (EN) |
|--------|------------|------------|
| WeaponShooterFirst | わかばシューター | Splattershot Jr. |
| WeaponShooterNormal | スプラシューター | Splattershot |
| WeaponShooterFlash | スプラッシュシューター系 | Splash-o-matic / Sploosh-o-matic |
| WeaponShooterShort | .52ガロン | .52 Gal |
| WeaponShooterGravity | .96ガロン | .96 Gal |
| WeaponShooterPrecision | ジェットスイーパー | Jet Squelcher |
| WeaponShooterBlaze | プライムシューター | Splattershot Pro |
| WeaponShooterLong | エクスシューター/N-ZAP系 | (mapping debate) |
| WeaponShooterExplosive | ボトルガイザー | Squeezer (variable mode) |
| WeaponShooterQuick | (recently added) | Pencilshot? |
| WeaponBlasterShort | ノヴァブラスター | Nova Blaster |
| WeaponBlasterMiddle | ホットブラスター | Blaster |
| WeaponBlasterLong | ロングブラスター | Range Blaster |
| WeaponBlasterLight | ラピッドブラスター | Rapid Blaster |
| WeaponBlasterLightLong | Rブラスターエリート | Rapid Blaster Pro |
| WeaponBlasterLightShort | クラッシュブラスター | Clash Blaster |
| WeaponBlasterPrecision | S-BLAST92 | S-BLAST |
| WeaponRollerNormal | スプラローラー | Splat Roller |
| WeaponRollerHeavy | ダイナモローラー | Dynamo Roller |
| WeaponRollerCompact | カーボンローラー | Carbon Roller |
| WeaponRollerHunter | ヴァリアブルローラー | Flingza Roller |
| WeaponRollerWide | ワイドローラー | Big Swig Roller |
| WeaponChargerNormal | スプラチャージャー | Splat Charger |
| WeaponChargerNormalScope | スプラスコープ | Splatterscope |
| WeaponChargerLong | リッター4K | E-liter 4K |
| WeaponChargerLongScope | 4Kスコープ | E-liter 4K Scope |
| WeaponChargerLight | 14式竹筒銃・甲 | Bamboozler 14 |
| WeaponChargerQuick | スクイックリン | Classic Squiffer |
| WeaponChargerKeeper | ソイチューバー | Goo Tuber |
| WeaponChargerPencil | R-PEN/5H | Snipewriter 5H |
| WeaponSlosherStrong | バケットスロッシャー | Slosher |
| WeaponSlosherDiffusion | ヒッセン | Tri-Slosher |
| WeaponSlosherDouble | スクリュースロッシャー | Dread Wringer |
| WeaponSlosherLauncher | エクスプロッシャー | Explosher |
| WeaponSlosherWashtub | オーバーフロッシャー | Bloblobber |
| WeaponSlosherBathtub | モップリン | (新, JP: Mopperin / EN: TBD) |
| WeaponSpinnerStandard | スプラスピナー | Mini Splatling? |
| WeaponSpinnerHyper | バレルスピナー | Heavy Splatling |
| WeaponSpinnerHyperShort | (ハイドラ系派生?) | Heavy Edit Splatling |
| WeaponSpinnerDownpour | ハイドラント | Hydra Splatling |
| WeaponSpinnerQuick | ノーチラス | Nautilus 47/79 |
| WeaponSpinnerSerein | クーゲルシュライバー/イグザミナー | Ballpoint Splatling? |
| WeaponManeuverShort | スパッタリー | Dapple Dualies |
| WeaponManeuverNormal | スプラマニューバー | Splat Dualies |
| WeaponManeuverGallon | ケルビン525 | Glooga Dualies |
| WeaponManeuverDual | デュアルスイーパー | Dualie Squelchers |
| WeaponManeuverStepper | クアッドホッパー | Tetra Dualies |
| WeaponManeuverLong | ガエンFF | Douser Dualies FF |
| WeaponShelterNormal | パラシェルター | Splat Brella |
| WeaponShelterCompact | スパイガジェット | Undercover Brella |
| WeaponShelterWide | キャンピングシェルター | Tenta Brella |
| WeaponShelterFocus | 24式張替傘・甲 | Recycled Brella 24 Mk I |
| WeaponBrushMini | パブロ | Inkbrush |
| WeaponBrushNormal | ホクサイ | Octobrush |
| WeaponBrushHeavy | フィンセント | Painbrush |
| WeaponStringerNormal | トライストリンガー | Tri-Stringer |
| WeaponStringerExplosion | LACT-450 | REEF-LUX 450 |
| WeaponStringerShort | フルイドV | Wellstring V |
| WeaponSaberLite | ジムワイパー | Splatana Wiper |
| WeaponSaberNormal | ドライブワイパー | Splatana Stamper |
| WeaponSaberHeavy | デンタルワイパー | Mint Decavitator |

### A.2 スペシャル

| 内部名 | 公式名 (JP) | 公式名 (EN) |
|--------|------------|------------|
| WeaponSpUltraShot | ウルトラショット | Trizooka |
| WeaponSpMultiMissile | マルチミサイル | Tenta Missiles |
| WeaponSpInkStorm | アメフラシ | Ink Storm |
| WeaponSpNiceBall | ナイスダマ | Booyah Bomb |
| WeaponSpMicroLaser | メガホンレーザー5.1ch | Killer Wail 5.1 |
| WeaponSpUltraStamp | ウルトラハンコ | Ultra Stamp |
| WeaponSpTripleTornado | トリプルトルネード | Triple Inkstrike |
| WeaponSpSkewer | サメライド | Reefslider |
| WeaponSpGachihoko | テイオウイカ | Kraken Royale |
| WeaponSpSuperLanding | ウルトラチャクチ | Splashdown (replaced) |
| WeaponSpPogo | ショクワンダー | Zipcaster |
| WeaponSpJetpack | ジェットパック | Inkjet |
| WeaponSpGreatBarrier | グレートバリア | Big Bubbler |
| WeaponSpShockSonar | ホップソナー | Wave Breaker |
| WeaponSpBlower | キューインキ | Ink Vac |
| WeaponSpChariot | カニタンク | Crab Tank |
| WeaponSpEnergyStand | エナジースタンド | Tacticooler |
| WeaponSpSuperHook | デコイチラシ | Super Chump |
| WeaponSpCastle | スプラッタカラースクリーン | Splattercolor Screen |
| WeaponSpFirework | (旧?) | (Legacy/unused) |
| WeaponSpChimney | (新?) | (TBD) |
| WeaponSpIkuraShoot | サーモンラン用 | Coop-only |

### A.3 サブ

| 内部名 | 公式名 (JP) | 公式名 (EN) |
|--------|------------|------------|
| WeaponBombSplash | スプラッシュボム | Splat Bomb |
| WeaponBombSuction | キューバンボム | Suction Bomb |
| WeaponBombQuick | クイックボム | Burst Bomb |
| WeaponBombCurling | カーリングボム | Curling Bomb |
| WeaponBombRobot | ロボットボム | Autobomb |
| WeaponBombFizzy | タンサンボム | Fizzy Bomb |
| WeaponBombTorpedo | トーピード | Torpedo |
| WeaponLineMarker | ラインマーカー | Angle Shooter |
| WeaponBeacon | ジャンプビーコン | Squid Beakon |
| WeaponPointSensor | ポイントセンサー | Point Sensor |
| WeaponPoisonMist | ポイズンミスト | Toxic Mist |
| WeaponSprinkler | スプリンクラー | Sprinkler |
| (Splash Wall) | スプラッシュシールド | Splash Wall |
| (Ink Mine) | トラップ | Ink Mine |

---

## 付録B: 検証コマンド

### Unity Inspector で値を確認

```csharp
// ScriptableObject から実HPを取得する例
public class TestDamageDisplay : MonoBehaviour
{
    [SerializeField] ShooterData _weapon;

    void OnGUI()
    {
        // 内部値→表示値
        float displayMax = _weapon.damageMax / 10f;
        float displayMin = _weapon.damageMin / 10f;
        GUI.Label(new Rect(10, 10, 400, 20),
            $"{_weapon.name}: {displayMax} → {displayMin} ダメージ");
    }
}
```

### 確殺距離の式

```
hp = 100
killShots = ceil(hp / damage_at_frame)
killTime = killShots * repeatFrame frames
killDistance = sum(velocity[t] for t in [0..killTime])
```

例: スプラシューター 36ダメ → 3発確殺 → 6F×2 = 12F (= 0.2秒)、確殺距離 ≈ Spawn初速2.266 × Straight4F + Brake残期間 ≈ 9-10 DU

---

## 付録C: 未解決事項・追加調査推奨

1. **WeaponShooterQuick / Explosive** が GitHub Trees API では取得失敗 — 内部名が変更された可能性。`WeaponInfoMain.json` でクロスリファレンスして再特定が必要。
2. **WeaponSpinnerNormal/Heavy/Long/Short** も 404 — 上記同様 (Standard/Hyper/HyperShort/Downpour/Quick/Sereinが正しい名前と判明)
3. **トラップ・スプリンクラー・シールド** の `WeaponMine* / WeaponSprinkler* / WeaponShield*` がweapon/フォルダにない (data/parameter/1110/misc/?) — 別ディレクトリ調査が必要
4. **DamageRateInfoConfig** (data/parameter/1110/misc/spl__DamageRateInfoConfig.pp__CombinationDataTableData.json) — 全武器のオブジェクト別ダメ倍率テーブル。レインメーカー/ビーコン/壁などへの倍率
5. **WeaponInfoMain.json** (data/mush/1110/) — 武器メタ情報 (必要SPpt、関連サブ・スペシャル等) のソースだが本DB未統合

これら未取得値の調査は、 https://github.com/Leanny/splat3 の `data/mush/1110/WeaponInfoMain.json` および `data/parameter/1110/misc/` を直接JSONダンプして補完すること。

---

> このDBは Splatoon 3 v1.1.10 相当 (Drizzle Season 2024)時点。
> 任天堂は定期アップデートでバランス調整を行う — 大型パッチごとに `data/parameter/1xxx/` を再取得すること。
> 最新版検索: https://github.com/Leanny/splat3/tree/main/data/parameter/latest
