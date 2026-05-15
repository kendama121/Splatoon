# Splatoon Unity完全再現 実装計画書

> 目的: マウス&キーボード操作でスプラトゥーン試合を**見た目・挙動完全再現**。
> 調査基盤: docs/Splatoon_Knowledge_Base.md + docs/Weapon_Database.md + 本書
> 環境: Unity 6 (推奨) + URP + InputSystem 1.x + Cinemachine + DOTween

---

## 目次

1. [全体方針・アーキテクチャ](#1-全体方針アーキテクチャ)
2. [技術スタック決定事項](#2-技術スタック決定事項)
3. [マウス・キーボード操作マッピング](#3-マウスキーボード操作マッピング)
4. [プレイヤー物理パラメータ初期値](#4-プレイヤー物理パラメータ初期値)
5. [インク塗装システム実装(コア技術)](#5-インク塗装システム実装コア技術)
6. [ビジュアル再現方針](#6-ビジュアル再現方針)
7. [VFX・パーティクル実装](#7-vfxパーティクル実装)
8. [UI/HUD構成](#8-uihud構成)
9. [オーディオシステム](#9-オーディオシステム)
10. [ステージ設計](#10-ステージ設計)
11. [実装フェーズ計画](#11-実装フェーズ計画)
12. [著作権・法務注記](#12-著作権法務注記)
13. [未解決事項・追加調査](#13-未解決事項追加調査)
14. [参考リソース統合](#14-参考リソース統合)

---

## 1. 全体方針・アーキテクチャ

### 1.1 Clean Architecture 4層構成(CLAUDE.md準拠)

```
┌──────────────────────────────────────────────┐
│  Presentation層 (MonoBehaviour、薄く)         │
│  - PlayerView / WeaponView / UIView          │
│  - InputSystem ハンドラ                       │
│  - Cinemachine カメラ制御                     │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│  Application層 (Use Cases)                    │
│  - ShootInkUseCase                           │
│  - TransformToSquidUseCase                   │
│  - SuperJumpUseCase                          │
│  - MatchProgressUseCase (State Pattern)      │
│  - ScoreCalculationUseCase                   │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│  Domain層 (POCO、ピュアC#)                   │
│  - Player / Inkling / Weapon / Gear / Match  │
│  - 物理値・ダメージ・スコア計算ロジック       │
│  - ModeRule (Strategy: TurfWar/SplatZones/..)│
└──────────────────────────────────────────────┘
                    ↑↓
┌──────────────────────────────────────────────┐
│  Infrastructure層                             │
│  - IInkPaintService (RenderTexture制御)      │
│  - IInkQueryService (移動判定)               │
│  - IVfxService (VFX Graph呼出)               │
│  - IAudioService (AudioMixer制御)            │
│  - INetworkService (将来Netcode統合)         │
└──────────────────────────────────────────────┘
```

### 1.2 設計原則(CLAUDE.md準拠)

- **nullチェック禁止**(意図的に省略)
- **`private` アクセス修飾子省略**
- **`///` XMLサマリーを全 public メンバーへ日本語付与**
- **インラインコメントも日本語、処理意図を丁寧に記述**
- **命名**: クラス/メソッド/プロパティ = PascalCase / publicフィールド = PascalCase / 非public = _camelCase / ローカル = camelCase / bool = is始まり

---

## 2. 技術スタック決定事項

| カテゴリ | 採用 | 理由 |
|---------|------|------|
| Render Pipeline | **URP** | 現プロジェクト設定済(Mobile_Renderer + PC_Renderer両用) |
| Unity Version | **Unity 6 LTS** (推奨) | RenderGraph対応、VFX Graph内蔵 |
| Input | **InputSystem 1.x** | 現プロジェクト設定済(InputSystem_Actions.inputactions) |
| カメラ | **Cinemachine 3.x** | ThirdPersonFollow + Blend |
| アニメ | **DOTween** | UIアニメ・スプラット演出 |
| インク塗装 | **VFX Mike式 4パス + ピンポンバッファ** | 業界デファクト、Splatoonity参照 |
| メタボール | **Bronson Zgeb式 sphere tracing** | URP Shader Graph + Custom Function |
| トゥーン | **NiloCat URPToon または Delt06 urp-toon-shader** | URP対応、無料 |
| パーティクル | **VFX Graph (GPU)** + 補助 ParticleSystem | 大量粒子対応 |
| ステージ作成 | **ProBuilder** + ProGrids | プロトタイプ高速化 |
| フォント | **TextMeshPro SDF + Project Paintball** | フリー類似フォント |
| 物理 | Unity標準Rigidbody + CharacterController | 軽量 |
| ネット | **Netcode for GameObjects** (将来) | Phase 7で着手 |

---

## 3. マウス・キーボード操作マッピング

### 3.1 操作対応表

| 入力 | アクション | 本家対応 |
|------|----------|---------|
| `W/A/S/D` | 移動(Vector2) | 左スティック |
| マウス移動 | カメラ・エイム | 右スティック+ジャイロ統合 |
| 左クリック | メインウェポン発射(押下/長押し) | ZR |
| 右クリック(長押し) | サブウェポン投擲(離して投げ) | R |
| `Shift`(長押し) | イカ潜伏 | ZL |
| `Space` | ジャンプ | B |
| `Q` | スペシャル発動 | A |
| `Tab`(長押し) | マップ表示 | X |
| `R` | カメラリセット | Y |
| `E` | カモン(This Way) | 十字↑ |
| `F` | ナイス(Booyah) | 十字↓ |
| `Shift`+方向逆 | イカロール | スイム中B+逆方向 |
| `Shift`(壁登り中)離し | イカノボリ | ZL離し |
| `Ctrl` | スコープON/OFF | チャージャー50%以上 |
| マウスホイール / `1`-`4` | 武器スロット(将来) | - |

### 3.2 マウス感度設計

- 横: 0.1〜0.5度/ドット
- 縦: 横の70%
- Raw Input(Windowsアクセラレーション無効)
- 設定UI: 内部マッピングで本家「-5〜+5」表記

### 3.3 Unity InputSystem 構成

```
InputSystem_Actions.inputactions に既に存在
追加 Action Map: "Splatoon" を作成
  - Move (Value Vector2): WASD Composite
  - Look (Value Vector2): Mouse Delta
  - Fire (Button): Mouse Left
  - Sub (Button Hold): Mouse Right
  - Squid (Button Hold): Shift
  - Jump (Button): Space
  - Special (Button): Q
  - Map (Button Hold): Tab
  - ResetCamera (Button): R
  - Booyah (Button): F
  - ComeOn (Button): E
  - Scope (Button): Ctrl
  - SquidRoll (Button + Modifier): Shift+反対方向
```

---

## 4. プレイヤー物理パラメータ初期値

### 4.1 単位系

- **1 DU(距離単位) = 0.0525 m** (本家換算、最も妥当)
- **目標 FPS = 60** (本家ロック値)

### 4.2 移動速度(C#変数として配置)

```csharp
// docs/Weapon_Database.md とも整合
const float DU_TO_METER = 0.0525f;
const float FPS = 60f;

float runSpeedBase = 0.96f * FPS * DU_TO_METER;    // = 3.024 m/s
float swimSpeedBase = 1.92f * FPS * DU_TO_METER;   // = 6.048 m/s
float swimSpeedMax = 2.40f * FPS * DU_TO_METER;    // = 7.560 m/s (Swim Speed Up極限)
float enemyInkSpeedMul = 0.25f;                    // 敵インク減速倍率(推定)
float gravity = 9.81f;                             // Unity標準でOK
float jumpVelocity = 5.0f;                         // 実測調整必要
```

### 4.3 HP / ダメージ / インクタンク

```csharp
float maxHP = 100f;
float regenHumanoid = 12.5f;            // HP/s(ヒト形態自然回復)
float regenSwim = 100f;                 // HP/s(イカ自軍インク内)
float enemyInkDamagePerSecond = 30f;    // HP/s
float enemyInkDamageCap = 40f;          // 累積上限(S2基準)

float inkTankMax = 100f;
float inkRegenHumanoid = 2.5f;          // %/s(要実測)
float inkRegenSwim = 15f;               // %/s(要実測)
```

### 4.4 リスポーン

```csharp
float splatAnimDuration = 0.5f;
float splatCamDuration = 6.0f;          // キラーカム
float respawnDescendDuration = 2.0f;
float totalRespawnDefault = 8.5f;       // ノーアビリティ時
```

### 4.5 イカロール / イカノボリ / スーパージャンプ

```csharp
int squidRollIFrames = 6;               // 推定無敵F
float squidRollArmor = 100f;            // 単発ダメージ吸収
float superJumpChargeSquid = 0.8f;      // 秒
float superJumpChargeHuman = 1.15f;     // +0.35秒
float superJumpLandingLag = 0.5f;
```

### 4.6 ScriptableObject化

`Assets/Splatoon/Data/PlayerPhysicsConfig.asset` として全数値を一元管理。
武器ごとの`WeaponData.asset`は[docs/Weapon_Database.md](Weapon_Database.md)の数値表を参照してScriptableObject化。

---

## 5. インク塗装システム実装(コア技術)

### 5.1 採用アーキテクチャ: VFX Mike式 4パス

**Step 1: World Position bake**(ロード時1回)
- 各ペイント対象メッシュの**UV3(TEXCOORD2)**にワールド位置を**ARGBFloat RT**へベイク
- UnityのUnwrapping.GenerateSecondaryUVSetでUV2(ライトマップ)生成、UV3を別途生成
- 重複なし展開

**Step 2: Splat Blit**(発生時)
- ピンポンバッファ(2枚交互スワップ)で上塗り処理
- 最大10スプラット/フレーム、超過は次フレーム
- チームごとRGBチャネル分離(R=チーム0、G=チーム1、B=チーム2、A=チーム3)
- SDFスプラットアトラスから形状サンプル

**Step 3: Score downsample**(1秒ごと)
- スプラットマップ → 256×256 → 4×4 ミップマップ縮小
- step(SDF, 0.5)で塗り判定
- AsyncGPUReadbackで非同期CPU読み戻し
- 各チャネル輝度平均でチームスコア

**Step 4: Surface render**(毎フレーム)
- 表面シェーダで SDF + DDX/DDY AA + 法線生成
- インクの盛り上がり/光沢/ネオン

### 5.2 RenderTexture仕様

| RT名 | サイズ | フォーマット | 役割 |
|------|-------|-------------|------|
| splatTex | 2048² (PC) / 1024² (mobile) | ARGB32 | 塗装マップ(ピンポンA) |
| splatTexAlt | 同上 | ARGB32 | 塗装マップ(ピンポンB) |
| worldPosTex | 2048² | ARGBFloat | ワールド位置ベイク |
| worldTangentTex | 2048² | ARGB32 | 接線ベイク(法線生成用) |
| worldBinormalTex | 2048² | ARGB32 | 従法線ベイク |
| scoreTex | 256² | ARGBHalf(MipMap=true) | スコア縮小 |
| rt4 | 4×4 | ARGB32 | 最終縮小・readback元 |

メモリ: PC 96MB / Mobile 24MB

### 5.3 URP移行で注意

- **Surface Shader廃止** → URP Lit Custom Pass 必須書き換え
- `ConfigureTarget(RenderTargetIdentifier)`廃止 → `ConfigureTarget(RTHandle, RTHandle)`
- `ScriptableRenderPass.Blit()`廃止 → `Blitter.BlitCameraTexture()`
- 全マテプロパティを `CBUFFER_START(UnityPerMaterial)` 内へ
- グローバルテクスチャは CBUFFER 外で `TEXTURE2D(_SplatTex); SAMPLER(sampler_SplatTex);`

### 5.4 起点リポジトリ

**Mix and Jam Splatoon-Ink (MIT)** をフォーク → Unity 6 URP移植
- https://github.com/mixandjam/Splatoon-Ink
- 既存BiRP実装をURP化(上記5.3の対応)

参照: **SquirrelyJones/Splatoonity** のシェーダ(GPL注意、コピーは不可、参考のみ)
- https://github.com/SquirrelyJones/Splatoonity

### 5.5 移動判定

```csharp
bool QueryInkAt(Vector3 worldPos, out int teamIndex) {
    if (!Physics.Raycast(worldPos + Vector3.up * 0.5f, Vector3.down,
                         out var hit, 2f, paintableMask))
        { teamIndex = -1; return false; }
    // CPU側ミラー(AsyncGPUReadbackで定期同期、256×256低解像度)
    var col = _cpuSplatMirror.GetPixelBilinear(hit.textureCoord2.x, hit.textureCoord2.y);
    float[] vals = { col.r, col.g, col.b, col.a };
    int max = 0;
    for (int i = 1; i < 4; i++) if (vals[i] > vals[max]) max = i;
    if (vals[max] > 0.5f) { teamIndex = max; return true; }
    teamIndex = -1;
    return false;
}
```

---

## 6. ビジュアル再現方針

### 6.1 アートディレクション(本家完全再現)

| 要素 | 採用方針 |
|-----|---------|
| キャラシェーディング | トゥーン(離散なしスムース) + 強リムライト + 薄アウトライン |
| インク色 | HDR 1.5倍ブースト → Bloom で輝かせる |
| ネオン感 | Emissive材 + URP Volume Bloom |
| 彩度 | Color Adjustments で +15〜+30 |
| アンチエイリアス | なし(本家通り) or 軽いFXAA |
| ポストエフェクト | Bloom + 軽MotionBlur + 薄Vignette + 微Chromatic Aberration |

### 6.2 URP Volume 推奨初期値

| エフェクト | 値 |
|----------|----|
| Bloom Threshold | 0.9〜1.1 |
| Bloom Intensity | 0.3〜0.5 |
| Bloom Scatter | 0.6〜0.7 |
| Motion Blur Intensity | 0.2以下 |
| Vignette Intensity | 0.15〜0.25 |
| Chromatic Aberration | 0.05〜0.1 |
| Color Saturation | +15〜+30 |
| Color Contrast | +5〜+10 |
| Tonemapping | ACES or Neutral |
| White Balance | +5(暖色寄り) |

### 6.3 トゥーンシェーダー

採用候補(全URP対応、無料):
- [NiloCat URP Toon Example](https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample)
- [Delt06 urp-toon-shader](https://github.com/Delt06/urp-toon-shader)
- [ChiliMilk URP_Toon](https://github.com/ChiliMilk/URP_Toon)

アウトライン: Inverted Hull方式(front culling + extruded normals)を Renderer Feature で

### 6.4 キャラモデル(オリジナル作成必須)

著作権により本家アセット使用不可。MMD/Sketchfabは学習のみ。**完全オリジナル**:
- 軟体生物 + 人型 のシルエット(イカ⇔ヒト変身可能)
- 触手(髪)4本以上の物理ボーン
- ストリートファッション風衣装

別モデル切替方式(ヒト/イカ)+中間2-4フレームをBlend Shape

---

## 7. VFX・パーティクル実装

### 7.1 発射パーティクル(武器カテゴリ別)

| カテゴリ | 構成 |
|---------|------|
| シューター | 主弾(楕円ジェル+引き伸ばしビルボード)+ 小飛沫3〜5個 + Trail |
| ローラー | 振り下ろし時に扇状多数弾 |
| チャージャー | 太い直線レーザー的弾 + Trail メイン |
| ブラスター | 時限破裂エフェクト → 放射状飛沫 |
| スロッシャー | バケツ投げモーション + 大インク塊放物線 |
| スピナー | シューター同型を高密度連射 |
| マニューバー | 二丁拳銃 + スライド後の精度UPモーション |

### 7.2 弾道物理(本家準拠 3ステートモデル)

詳細値は [docs/Weapon_Database.md](Weapon_Database.md) の各武器セクション。

```csharp
// 1発の弾の物理ステート
enum BulletState { Straight, Brake, Free }

// 状態遷移: Straight (N frames) → Brake (Y速度 < -0.15まで) → Free
// Straight: 重力なし、定速直進
// Brake: 空気抵抗36% + 重力0.07 DU/F² (≒13.23 m/s²)
// Free: 空気抵抗2% + 重力0.016 DU/F² (≒3.02 m/s²)
```

### 7.3 メタボール風融合

**Bronson Zgeb式 sphere tracing**(URP Shader Graph + Custom Function HLSL)
- MaterialPropertyBlock で position+radius を配列送信
- URPのPBR lighting関数を Custom Function 経由で再利用
- インク塊が空中で融合して見える表現

参考: https://bronsonzgeb.com/index.php/2021/02/27/particle-metaballs-in-unity-using-urp-and-shader-graph-part-1/

### 7.4 着弾エフェクト(本家タイミング、60fps)

| フェーズ | フレーム数 | 内容 |
|---------|----------|------|
| 着弾爆発拡大 | 4〜6F (66〜100ms) | 急速膨張 |
| リング波紋 | 8〜12F | 円形に広がる |
| 飛沫減衰 | 15〜30F | 周囲粒子寿命 |
| スプラット定着 | 即時 | RenderTextureへ焼付 |

### 7.5 キャラアニメ

**イカ⇔ヒト変身**(プロアニメーター分析より):
- 3段階(ヒト → 中間 → イカ)、合計8〜12F (133〜200ms)
- 中間に Smear Frame(動きブラー)2〜4F
- Blend Shape で squishy 変形

**主要モーション**:
- イカ: 待機(ピクピク)/スイム(尾鰭水平)/壁登り/潜伏(静止)
- ヒト: アイドル(weight shift)/歩行/走行/射撃(武器別)/被弾/デス(インク散布15-30F)/勝利ポーズ
- スーパージャンプ: チャージ(イカ姿勢、無防備)/頂点停止/着地インク爆発
- リスポーン: スポーンドローン(エスプレッソマシン風)から放出

---

## 8. UI/HUD構成

### 8.1 HUD配置

| 位置 | 要素 | Unity実装 |
|------|------|----------|
| 上部中央 | 塗り進捗バー(両チーム%) | Image fillAmount 中央→左右 |
| 右上 | 残り時間(3:00→0:00) | TextMeshPro、残り30秒で赤化 |
| 左下 | インクタンク(円形/縦型) | Image fillAmount Vertical |
| インクタンク上 | スペシャルゲージ | Image fillAmount Radial 360 |
| ゲージ下 | サブ/メイン/スペシャル/ギアアイコン | Sprite+TextMeshPro |
| 下部 | チーム4人ステータス | HorizontalLayoutGroup |
| Tab長押し | マップ全画面 | 別Canvas切替 |

### 8.2 Canvas設定

- Screen Space - Overlay
- 1920×1080 + Scale With Screen Size
- Match Width Or Height = 0.5

### 8.3 フォント

**Project Paintball**(フリー、Squidboards制作)をTextMeshPro SDF Font Asset化
- Sampling Point Size: 50-70
- Padding: 5
- Outline Width: 0.2 + Soft Edge + Glow

公式類似: Dreamland (Jim Parkinson), Showcard Gothic

### 8.4 アニメ

- DOTween: `RectTransform.DOPunchScale` でカード出現時パンチ
- スペシャル満タン時パルス・回転
- スプラット時の対戦相手アイコン中央大表示

---

## 9. オーディオシステム

### 9.1 AudioMixer階層

```
Master
├── BGM
│   ├── Battle BGM
│   ├── Lobby BGM
│   └── Result BGM
├── SE
│   ├── Weapons (武器発射・着弾)
│   ├── Movement (スイム・変身・ジャンプ)
│   ├── UI
│   └── Environment
└── Voice
    ├── Self (Booyah/ComeOn)
    └── Other Players
```

### 9.2 3Dオーディオ設定

- Spatial Blend: 武器音=0.8〜1.0(3D)、UI=0.0(2D)
- Volume Rolloff: Logarithmic
- Min Distance: 5m / Max Distance: 40〜50m
- Doppler Level: 0.3(銃声に弱く)

### 9.3 Splatoon独自の演出

- スイム中: Low Pass Filter (Cutoff 800Hz) でBGMこもらせ
- スーパージャンプ中: Volume Snapshot で全体音量一時減衰

### 9.4 イカ語生成方法(再現)

- 「英語訛りを混ぜたランダム音節」を声優が発声
- ピッチシフト+リバーブで「水中感」演出
- 既存オリジナル音源 + Audacityで再現可能

---

## 10. ステージ設計

### 10.1 スケール感(本家準拠)

- キャラ身長: 0.8〜1.0m
- 建物高さ(中央/高所): 最大10m
- ジャンプ高度: 約2m(基本)、スーパージャンプ10〜30m
- 塗装可能面積: 中央値約2200p ≒ 110〜220m² (20m×10m程度)

### 10.2 ステージ作成

**ProBuilder** ワークフロー:
1. Tools > ProBuilder > New Shape で Cube 配置(5×2×5m基本)
2. ProGrids で 0.5m 単位スナップ
3. Face/Edge/Vertex 編集 → Extrude → Bevel
4. PolyShape で複雑地形

### 10.3 UV設計(3チャネル使い分け)

| UV | 用途 |
|----|------|
| UV0 (TEXCOORD0) | 通常テクスチャ(Albedo) |
| UV2 (TEXCOORD1, mesh.uv2) | ライトマップ自動生成 |
| **UV3 (TEXCOORD2, mesh.uv3)** | **インク塗装専用(重複なし展開、Splatoon方式)** |

UV2/UV3衝突回避: Unwrapping.GenerateSecondaryUVSet はmesh.uv2 を上書きするため、塗装はuv3を使う。

### 10.4 対称性ルール

- 99%のステージは鏡像対称
- リスポーン位置は両端高台
- 中央エリアは塗装可能面積の20〜30%(衝突ホットスポット)

### 10.5 表面分類

| 表面 | 塗装 | スコア | スイム |
|-----|-----|--------|--------|
| 床 | OK | 加算 | OK |
| 壁(垂直) | OK | 加算なし | OK(壁登り) |
| ガラス・シート | NG | - | - |
| Grate(透過格子) | NG | - | NG |
| 水場 | - | - | 即死 |

### 10.6 ライティング

- Lightmapper: Progressive GPU
- 屋外: Mixed Directional + Baked Indirect、Direct Samples 32 / Indirect 512
- 屋内: ネオン強調(Emissive) + Baked Point Lights + 反射プローブ(Resolution 256)
- 反射プローブ Refresh Mode: On Demand(塗り変更時のみ)

---

## 11. 実装フェーズ計画

### Phase 1: プロジェクト基盤(優先度: 最高)
- [ ] Unity 6 LTS にアップグレード(必要なら)
- [ ] 必要パッケージ追加(Cinemachine, ProBuilder, DOTween)
- [ ] フォルダ構造作成: `Assets/Splatoon/{Domain,Application,Infrastructure,Presentation}`
- [ ] CLAUDE.md準拠の命名規約コーディングルール文書化
- [ ] 第一テストステージ(ProBuilder製、20×10m単純箱型)

### Phase 2: プレイヤー基本動作
- [ ] CharacterController + マウスキーボード操作
- [ ] InputSystem アクション全設定
- [ ] Cinemachine ThirdPersonFollow カメラ
- [ ] ヒト⇔イカ変身(2モデル切替 + ブレンド)
- [ ] 移動速度(ヒト/イカ/敵インク内/壁登り)
- [ ] ジャンプ + Squid Roll + Squid Surge
- [ ] スーパージャンプ(チャージ→飛行→着地)

### Phase 3: インクシステム(最重要)
- [ ] Mix and Jam リポをフォーク → Unity 6 URP移植
- [ ] VFX Mike式 4パス実装(World pos bake → Splat blit → Score downsample → Surface render)
- [ ] SDFスプラットアトラス作成(円/ブロブ/飛沫/ローラー長線/フット)
- [ ] ピンポンバッファ + チーム色RGB分離
- [ ] AsyncGPUReadback でスコア集計
- [ ] 移動判定(自軍/敵軍/中立3状態)
- [ ] 表面シェーダ(SDF + DDX/DDY AA + 法線生成 + リムライト + Emissive)

### Phase 4: 武器システム
- [ ] WeaponData ScriptableObject(11カテゴリ × 各数体)
- [ ] 弾道3ステートモデル(Straight → Brake → Free)
- [ ] シューター実装(主弾 + 飛沫 + Trail + メタボール融合)
- [ ] ローラー実装(振り/コロガシ/飛沫扇配置)
- [ ] チャージャー実装(チャージ + スコープ + 直線弾)
- [ ] サブウェポン全14種
- [ ] スペシャル全19種(優先順: ウルトラショット/ナイスダマ/カニタンク等)
- [ ] インクタンク + スペシャルゲージ
- [ ] ダメージ・HP・リスポーン

### Phase 5: VFX・ビジュアル仕上げ
- [ ] トゥーンシェーダ(NiloCat URPToon 等)導入
- [ ] アウトライン(Inverted Hull)
- [ ] URP Volume(Bloom/MotionBlur/ColorAdjustments)
- [ ] 武器発射エフェクト(マズルフラッシュ + 飛沫)
- [ ] 着弾エフェクト(拡大→波紋→定着)
- [ ] スプラット時の散り方(インク弾け)
- [ ] スーパージャンプ演出(光柱+着地爆発)
- [ ] スペシャル発動演出(各19種)

### Phase 6: UI/オーディオ/ゲームロジック
- [ ] HUD全要素(タイマー/塗りバー/インクタンク/スペシャル/チームステータス)
- [ ] マップ画面(Tab長押し)
- [ ] TextMeshPro + Project Paintball
- [ ] ナワバリバトル ロジック(3分タイマー + スコア集計 + 勝敗判定)
- [ ] 試合終了演出(ジャッジ判定 + 個人スコア)
- [ ] AudioMixer + 全SE/BGM配置
- [ ] 3D音響設定
- [ ] イカ語ボイス再現(録音 + ピッチ加工)

### Phase 7: 追加モード・ネット対戦(将来)
- [ ] ガチエリア/ヤグラ/ホコ/アサリ(State/Strategy)
- [ ] サーモンラン(PvE)
- [ ] Netcode for GameObjects 統合
- [ ] マッチメイキング・ロビー
- [ ] ギアシステム + ギアパワー

---

## 12. 著作権・法務注記

### NG事項
- 本家アセット(モデル/テクスチャ/音/UI/フォント)使用は禁止
- 固有名(Inkling、Octoling、Splatoon、ジャッジくん、Splattershot等)使用不可
- キャラクター類似(造形・配色・服装)も意匠侵害リスク

### OK事項(クローン制作の安全領域)
- ゲームメカニクス(インクで陣取り)はアイディアであり著作権対象外
- VFX Mike/Mix and Jam等の技術記事の手法採用
- Project Paintball等のフリーフォント
- 完全オリジナルキャラ・音楽・UIで構築

### MMD/Sketchfab
- 学習・研究目的のみ
- 公開リリースには絶対に含めない

---

## 13. 未解決事項・追加調査

### 数値関連(実機計測推奨)
1. プレイヤー通常ジャンプの正確な初速・高さ
2. 敵インク移動速度倍率の正確な係数
3. イカロール無敵フレームの厳密値
4. スーパージャンプ各フェーズの正確なフレーム数
5. インクタンク自然回復の DU/frame
6. カメラ正確な距離・FOV

### データソース未取得
1. **leanny.github.io/splat3/database.html** をブラウザ手動アクセス → CSV/JSON取得 → ScriptableObject化
2. `WeaponInfoMain.json`(必要SPpt + メイン↔サブ↔スペシャルセット組合)
3. `DamageRateInfoConfig`(レインメーカー等オブジェクト別倍率)
4. トラップ/スプリンクラー/シールドが weapon/ 階層外(別ディレクトリ調査必要)

---

## 14. 参考リソース統合

### 必読技術文献
1. [VFX Mike: Splatoon in Unity](http://vfxmike.blogspot.com/2017/04/splatoon-in-unity.html) — 業界デファクトの解析
2. [Bronson Zgeb: Mix and Jam Splatoon Ink](https://bronsonzgeb.com/index.php/2021/03/15/mix-and-jam-splatoons-ink-system/)
3. [Bronson Zgeb: Particle Metaballs Part 1-3](https://bronsonzgeb.com/index.php/2021/02/27/particle-metaballs-in-unity-using-urp-and-shader-graph-part-1/)
4. [Cyanilux: URP Shader Code](https://www.cyanilux.com/tutorials/urp-shader-code/)
5. [Cyanilux: Custom Renderer Features](https://www.cyanilux.com/tutorials/custom-renderer-features/)
6. [Andrew Cassidy: SDF Antialiasing](https://drewcassidy.me/2020/06/26/sdf-antialiasing/)

### 起点リポジトリ
- [mixandjam/Splatoon-Ink (MIT)](https://github.com/mixandjam/Splatoon-Ink) — フォーク起点
- [SquirrelyJones/Splatoonity (GPL注意)](https://github.com/SquirrelyJones/Splatoonity) — 参考のみ

### URP公式ドキュメント
- [URP Custom Shader Upgrade Guide](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/urp-shaders/birp-urp-custom-shader-upgrade-guide.html)
- [URP Fullscreen Blit](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-features/how-to-fullscreen-blit.html)
- [Unity 6 RenderGraph Migration](https://discussions.unity.com/t/migrating-scriptablerenderpass-to-rendergraph-in-unity-6-urp/1648456)

### データ源
- [Inkipedia](https://splatoonwiki.org/)
- [Leanny Splatoon 3 Database](https://leanny.github.io/splat3/database.html)
- [Leanny Splatoon 2 Parameter Calculator](https://leanny.github.io/splat2new/parameter_calcs.html)
- [Squidboards Weapon Damage Megathread](https://squidboards.com/threads/splatoon-weapon-damage-data-megathread.2306/)
- [Game UI Database - Splatoon 3](https://www.gameuidatabase.com/gameData.php?id=1512)

### Unity技術
- [Mix and Jam YouTube](https://www.youtube.com/c/MixandJam)
- [Catlike Coding Rendering](https://catlikecoding.com/unity/tutorials/rendering/part-3/)

---

> **次のアクション**: ユーザーへ Phase 1 着手の確認 → 同意あればプロジェクト基盤構築から着手。
