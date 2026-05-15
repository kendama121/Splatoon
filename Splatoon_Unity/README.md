# Splat Battle — Splatoon-like Turf War Clone (Unity 6 / URP)

任天堂 Splatoon を題材にした、**個人学習目的の TPS 陣取りゲーム**。Unity 6 LTS + URP で完全自作。MCP for Unity 経由で Claude が全実装した試験的プロジェクト。

> ⚠️ **法的注意**: 完全非公開・個人学習用。Splatoon は任天堂株式会社の登録商標。本プロジェクトはアセット・固有名・ロゴいずれも任天堂著作物を使用していないが、メカニクスを参考にした **fan-art / 学習用クローン** であり、商用利用・公開配布禁止。

---

## 目次

1. [プロジェクト概要](#プロジェクト概要)
2. [技術スタック](#技術スタック)
3. [アーキテクチャ](#アーキテクチャ)
4. [ディレクトリ構成](#ディレクトリ構成)
5. [インク塗装システム](#インク塗装システム)
6. [シェーダ詳細](#シェーダ詳細)
7. [UIToolkit](#uitoolkit)
8. [プロシージャルテクスチャ](#プロシージャルテクスチャ)
9. [プロシージャル音響](#プロシージャル音響)
10. [武器・サブ・スペシャル](#武器サブスペシャル)
11. [ステージ構成](#ステージ構成)
12. [ギミック](#ギミック)
13. [HUD・UI](#hudui)
14. [ゲームモード](#ゲームモード)
15. [操作コマンド](#操作コマンド)
16. [セットアップ](#セットアップ)
17. [開発履歴](#開発履歴)

---

## プロジェクト概要

- **タイトル**: Splat Battle (内部コード Splatoon_Unity)
- **ジャンル**: チームベース TPS 陣取り (Splatoon クローン)
- **プレイヤー数**: 1 (vs BOT、MVP)
- **対戦時間**: 3 分
- **目標**: 床面塗り面積で勝敗判定
- **エンジン**: Unity 6000.3.11f1
- **言語**: C# (.NET Standard 2.1)
- **対応OS**: Windows (Editor 動作確認)
- **入力**: マウス&キーボード (Unity New Input System)
- **開発期間**: 約 1.5 日 (Claude による完全自走実装)

### ハイライト

- **完全自作インク塗装システム** — VFX Mike 式 RenderTexture/UV2 方式の URP 移植
- **本家準拠数値** — Inkipedia + Leanny GameParameterTable.json 解析
- **3D ステージ** — 100×60m メインステージ + マンタマリア号 + ハンマーヘッドブリッジ + 港街 計 4 マップ
- **武器 11 種 / サブ 15 種 / スペシャル 19 種** — 全種 ScriptableObject 化
- **5 ゲームモード** — ナワバリ / エリア / ヤグラ / ホコ / アサリ Strategy パターン
- **UI Toolkit + uGUI 二重実装** — モダン UXML/USS HUD + StartMenu
- **プロシージャル全実装** — テクスチャ・音響・SE をスクリプト合成、外部画像/音源ゼロ

---

## 技術スタック

### Unity Packages

```json
{
  "com.unity.render-pipelines.universal": "17.3.0",
  "com.unity.inputsystem": "1.19.0",
  "com.unity.cinemachine": "3.x",
  "com.unity.probuilder": "6.0.9",
  "com.unity.ai.navigation": "2.0.11",
  "com.unity.timeline": "1.8.11",
  "com.unity.ugui": "2.0.0",
  "com.unity.test-framework": "1.6.0",
  "com.unity.visualscripting": "1.9.10"
}
```

### 採用技術と理由

| 技術 | 用途 | 理由 |
|------|------|------|
| **URP 17.3** | レンダパイプライン | カスタムシェーダ + RenderTexture 制御 + ScriptableRenderFeature 拡張性 |
| **InputSystem 1.19** | 入力 | キー/マウス/ゲームパッド統一抽象 |
| **Cinemachine 3** | カメラ | ThirdPersonFollow + Blend スタック |
| **UI Toolkit** | UI | UXML/USS による Web 風レイアウト、HotReload、StyleSheet |
| **uGUI** | UI 旧版 HUD | Canvas + Image fillAmount でリアルタイム描画 |
| **TextMeshPro** | 文字 | SDF レンダ + アウトライン + Glow Underlay |
| **Particle System** | VFX | マズルフラッシュ・着弾飛沫・花火 |
| **AsyncGPUReadback** | スコア集計 | 非同期 GPU→CPU 転送でストール回避 |
| **RenderTexture (ピンポンバッファ)** | インク塗装マップ | フレーム毎 Blit + Read-Write 安全化 |
| **AudioClip プロシージャル生成** | SE/BGM | 外部音源不要、コード合成のみ |
| **ScriptableObject** | データアセット | 武器/スペシャル/サブ/物理パラメータ |

---

## アーキテクチャ

**Clean Architecture 4 層構成**(CLAUDE.md ルール準拠)。Domain 層は POCO、依存は内向きのみ。

```
┌─────────────────────────────────────────────────────────┐
│ Presentation 層 (MonoBehaviour、View/Controller)         │
│  PlayerController / WeaponShooter / InkBullet /          │
│  PlayerHealth / SpecialAction / SubWeaponAction /        │
│  TeamMember / AdvancedBot / HUDManager / MapView /       │
│  StartMenuToolkit / SplatoonHUDToolkit / KillFeed /      │
│  ScreenDamageOverlay / CameraShaker / ProceduralAudio    │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Application 層 (Use Cases / Services)                    │
│  InkPaintService / ScoreCalculator / SwimSpeedModifier / │
│  SuperJumpAction / TurfWarMatchManager /                 │
│  MatchModeBase + TurfWar/SplatZones/                     │
│  TowerControl/Rainmaker/ClamBlitz                        │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│ Domain 層 (POCO、純 C#)                                  │
│  PlayerPhysicsConfig / TeamId / WeaponCategory /         │
│  WeaponData / SpecialCategory / SpecialData /            │
│  SubWeaponCategory / SubWeaponData                       │
└─────────────────────────────────────────────────────────┘
                          ↑
┌─────────────────────────────────────────────────────────┐
│ Infrastructure 層 (外部リソース・GPU 操作)               │
│  InkPaintManager (RenderTexture + CommandBuffer) /       │
│  PaintableSurface (CPUミラー + AsyncGPUReadback) /       │
│  ProceduralTextureBuilder (Texture2D 合成)               │
└─────────────────────────────────────────────────────────┘
```

### 設計原則 (CLAUDE.md 準拠)

- `null` チェック禁止(意図的省略)
- `private` 修飾子省略
- `///` XML サマリーを全 public メンバーに**日本語で**付与
- インラインコメントも**日本語**で意図を記述
- **命名規約**:
  - クラス/メソッド/プロパティ/public フィールド = `PascalCase`
  - private フィールド = `_camelCase`
  - ローカル変数 = `camelCase`
  - bool = `is` 始まり

---

## ディレクトリ構成

```
Splatoon_Unity/
├── Assets/
│   ├── Splatoon/
│   │   ├── Domain/          # POCO + ScriptableObject 定義
│   │   ├── Application/     # ユースケース・ゲームモード
│   │   ├── Infrastructure/  # InkPaintManager・PaintableSurface・ProceduralTextureBuilder
│   │   ├── Presentation/    # MonoBehaviour 全部
│   │   ├── Shaders/         # TexturePainter / ExtendIslands / PaintableSurface
│   │   ├── UIToolkit/       # UXML / USS / PanelSettings
│   │   ├── Data/            # ScriptableObject .asset 52 個 + Material + Texture
│   │   ├── Prefabs/         # InkBullet
│   │   ├── Scenes/          # MVP_TurfWar.unity
│   │   └── Screenshots/     # 開発スクショ
│   ├── Settings/            # URP Renderer + RP Asset (Mobile/PC)
│   └── InputSystem_Actions.inputactions
├── Packages/
│   └── manifest.json
├── ProjectSettings/
└── docs/
    ├── Splatoon_Knowledge_Base.md   # 世界観・モード・武器網羅資料
    ├── Weapon_Database.md           # Leanny 由来武器詳細データ
    └── Implementation_Plan.md       # Unity URP 実装計画書
```

---

## インク塗装システム

### コンセプト

VFX Mike 式 (2017 解析) を URP に移植。**メッシュを UV2 空間に展開してテクスチャに直接ペイント**する方式。

### パイプライン

```
[1] PaintableSurface.Awake
  ├── MaskRT (2048² ARGB32, 塗装本体)
  ├── MaskSupport (2048² ARGB32, 一時バッファ)
  ├── CpuMirror (128² Texture2D, CPU 側色サンプル用)
  └── _downsampleRT (128² ARGB32, AsyncGPUReadback 経由更新)

[2] InkPaintManager.Paint(surface, worldPos, radius, team)
  ├── 弾着弾時に呼出
  ├── CommandBuffer に Blit を積む
  │   ├── Pass 0: TexturePainter.shader で MaskSupport に塗装
  │   └── Pass 1: ExtendIslands.shader で MaskRT に dilate コピー
  └── Graphics.ExecuteCommandBuffer で実行

[3] TexturePainter.shader (URP)
  ├── 頂点シェーダ: UV2 を SV_Position に変換 (メッシュを UV 空間に展開)
  ├── _ProjectionParams.x で D3D/GL Y-flip 対応
  ├── フラグメント: ブラシ中心とのワールド距離 → smoothstep
  ├── 角度方向の sin 合成ノイズで花びら状ギザギザ境界
  └── _BrushSeed ランダムでスプラットごとに形状変化

[4] ExtendIslands.shader (URP)
  └── 8-tap dilation: UV 境界部の hairline 消去

[5] PaintableSurface.shader (URP/Lit 互換)
  ├── ベース色 + _SplatTex (MaskRT) を UV2 で重ね合わせ
  ├── インク部分は Smoothness ↑ (湿った光沢)
  ├── インク部分は Emission で発光 (ネオン感)
  └── SurfaceData → UniversalFragmentPBR で PBR 出力

[6] PaintableSurface.Update
  ├── 0.25 秒毎に MaskRT → _downsampleRT に縮小 Blit
  └── AsyncGPUReadback.Request → CpuMirror に反映

[7] SwimSpeedModifier.GetSpeedMultiplier(footPos, myTeam, isSquid)
  ├── 足元から下方向 Raycast
  ├── ヒット surface.CpuMirror.GetPixelBilinear(uv) で色取得
  ├── RGB チャネル比でチーム判定
  │   ├── 自軍 → ヒト 1.10 倍 / イカ 1.50 倍
  │   ├── 敵軍 → 0.35 倍 + 継続ダメージ
  │   └── 中立 → 1.00 倍
```

### 数値仕様

- 床マスク解像度: 2048² (PaintableSurface 個別、Stage_Ground 用)
- 壁・ハイランド: 512² (パフォーマンス優先)
- 塗装ブラシ半径: 武器ごとに 0.3〜1.8m
- 多重スプラット弾: 1 発撃つと主弾 1 + 飛沫 5 発バースト (本家質感)
- CPU ミラー更新: 0.25 秒毎 AsyncGPUReadback

---

## シェーダ詳細

### `Splatoon/TexturePainter.shader`

ブラシペイント用。Mesh を UV2 空間にオーソグラフィック展開 → ブラシ中心とのワールド距離でマスク生成。

```hlsl
Varyings Vert(Attributes v) {
    Varyings o;
    float2 uv = v.uv1.xy;
    o.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
    o.positionCS.y *= _ProjectionParams.x; // D3D/GL Y-flip
    o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
    o.uv1 = uv;
    return o;
}

half4 Frag(Varyings i) : SV_Target {
    float3 rel = i.worldPos - _BrushWorldPos;
    float dist = length(rel.xz);
    float2 dir = normalize(rel.xz);
    float angle = atan2(dir.y, dir.x);
    // 多周波数 sin 合成で花びら状歪み
    float radialNoise = sin(angle * 6 + _BrushSeed * 1.3) * 0.18
                      + sin(angle * 13 + _BrushSeed * 2.7) * 0.10
                      + sin(angle * 27 - _BrushSeed * 4.1) * 0.05;
    float effectiveR = _BrushRadius * (1.0 + radialNoise);
    float mask = 1.0 - smoothstep(effectiveR * _BrushHardness, effectiveR, dist);
    return half4(_BrushColor.rgb, mask);
}
```

### `Splatoon/ExtendIslands.shader`

8-tap dilation。塗装マスクの UV シーム部分の黒線除去。

### `Splatoon/PaintableSurface.shader`

URP Lit 互換のサーフェスシェーダ。

- 第 0 UV: 通常テクスチャ(BaseMap、グリッドタイル等)
- 第 2 UV: 塗装マスク(SplatTex)
- ベース色 × グリッドテクスチャ × 塗装色を重ね合わせ
- インク部分は smoothness を上げ濡れた光沢
- インク部分は emission で発光(ブルーム連携)
- ワールド座標から `step(frac, 0.45)` でグリッド線を生成
- 微細ノイズで床凹凸感
- `LightMode = UniversalForward` + `ShadowCaster` 2 パス

---

## UIToolkit

### ファイル構成

```
Assets/Splatoon/UIToolkit/
├── SplatoonHUD.uxml           # 試合中 HUD レイアウト
├── SplatoonHUD.uss            # HUD スタイル
├── StartMenu.uxml             # タイトル画面レイアウト
├── StartMenu.uss              # タイトル画面スタイル
├── HUDPanelSettings.asset     # PanelSettings (sortingOrder 200)
└── StartMenuPanelSettings.asset (sortingOrder 300)
```

### SplatoonHUD.uxml の VisualElement 階層

```xml
<Root class="root" picking-mode="Ignore">
    <TopBar>
        <Label name="Timer" />                 <!-- 3:00、残30秒で赤化 -->
        <ScoreBarBG>
            <AlphaBar />                       <!-- 中央→左 Scale Lerp -->
            <BravoBar />                       <!-- 中央→右 Scale Lerp -->
            <AlphaPct /> <BravoPct />
            <LeadText />                       <!-- 差5%以上で表示 -->
        </ScoreBarBG>
    </TopBar>
    <InkTankFrame>
        <InkTankFill />                        <!-- Height Lerp -->
        <InkPct /> <InkLabel />
    </InkTankFrame>
    <SpecialFrame>                             <!-- 円形 -->
        <SpecialFill />                        <!-- Scale Lerp 円形充電 -->
        <SpecialLabel />
    </SpecialFrame>
    <Label name="WeaponName" />
    <Label name="CenterNotice" />              <!-- 試合終了「YOU WIN!」 -->
</Root>
```

### USS スタイル特徴

- **HTML/CSS 風記法**: `background-color: rgba(...)`, `border-radius: 18px`, `text-shadow: 0 4px 8px ...`
- **角丸ネオン縁** ボタン: `border-color: rgba(255,200,80,...)` + `border-width: 4px`
- **インタラクティブ擬似クラス**: `.menu-btn:hover { scale: 1.04; }` / `.menu-btn:active { scale: 0.98; }`
- **絶対位置 + translate** で中央寄せ: `left: 50%; translate: -50% 0;`
- **linear-gradient** で背景: `background-image: linear-gradient(180deg, ...)`

### StartMenu.uxml — タイトル画面

```
SPLAT BATTLE タイトル(180px、bold-italic、ピンクシャドウ)
INK YOUR TURF! サブタイトル(44px、ネオンピンク)
背景: 紫グラデ + 5 色インクスパット円
ボタン 4 つ:
  - START BATTLE (オレンジ、95px 高さ)
  - HOW TO PLAY (デフォルト)
  - SETTINGS (デフォルト)
  - QUIT (赤系)
```

### Controller

- `SplatoonHUDToolkit.cs` — `_root.Q<Label>("Timer")` で要素取得、Update で Lerp 反映
- `StartMenuToolkit.cs` — `_btnStart.clicked += StartGame;` でイベント、Cursor 制御、HUD/uGUI 連携

---

## プロシージャルテクスチャ

外部画像ゼロ。`ProceduralTextureBuilder.cs` で実行時生成。

### 生成パターン

```csharp
public static Texture2D MakeGridTile(int size, int cellPixels, float noiseStrength);
// グリッド線 + Perlin ノイズで微細凹凸感、床用

public static Texture2D MakeStripes(int size, Color a, Color b, int stripeWidth);
// 縞模様、屋根用

public static Texture2D MakeWoodGrain(int size, Color light, Color dark);
// 同心円 + ノイズで年輪、Crate 用

public static Texture2D MakeHexPattern(int size, Color baseCol, Color hexCol, float cellSize);
// 六角ハニカム、SF 高所用

public static Texture2D MakeMetalPlate(int size, Color baseCol);
// リベット付き金属プレート、壁・クレーン用

public static Texture2D MakeCityWindows(int size, Color buildingCol, Color windowCol);
// 都市夜景、遠景ビル用
```

### 適用先

- メイン床: Grid
- Crate × 4: Wood
- ハイランド上面: Hex
- 港街屋根: Stripe
- クレーン・パイプ: Metal
- 都市シルエット 24 棟: CityWindows + Emission

---

## プロシージャル音響

`ProceduralAudio.cs` で AudioClip を実行時合成。外部音源ゼロ。

### 生成 SE

- **PlayShoot()** — ノイズスパイク + 急速減衰 (0.08 秒)
- **PlayImpact()** — 200→100Hz 低音スイープ + ノイズ (0.18 秒)
- **PlayJump()** — 300→900Hz 上昇スイープ (0.25 秒)
- **PlayWin()** — C-E-G-C 上昇和音 + harmonic (1.2 秒、勝利ファンファーレ)

### BGM ループ

```csharp
// 4 秒ループ、8 拍構成
// Bass(矩形波 110Hz/165Hz/220Hz) + Lead(サイン+倍音) + Kick(60Hz急減衰)
// AudioClip.Create + SetData で float[] 直接書込
// AudioSource.loop=true で連続再生
```

---

## 武器・サブ・スペシャル

### 武器メイン 11 種 (`WeaponData` SO)

シューター/ローラー/チャージャー/スロッシャー/スピナー/マニューバー/シェルター/ブラスター/フデ/ストリンガー/ワイパー。

**パラメータ** (Leanny GameParameterTable.json 由来):
- DamageMax / DamageMin / ReduceStartFrame / ReduceEndFrame
- MaxRangeMeters / MuzzleVelocity (DU/F)
- PaintRadius / InkConsumePerShot / FireIntervalFrames

### 弾道 3 ステートモデル (本家準拠)

```
[Straight] N フレーム定速直進 (重力なし)
    ↓
[Brake] 空気抵抗 36% + 重力 0.07 DU/F²
    ↓ (Y速度 < -0.15 DU/F で遷移)
[Free] 空気抵抗 2% + 重力 0.016 DU/F²
```

### サブウェポン 15 種 (`SubWeaponData` SO)

スプラッシュボム/カーリング/クイック/キューバン/スプリンクラー/スプラッシュシールド/オートボム/ロボットボム/タンサン/トーピード/トラップ/ポイントセンサー/ジャンビ/ラインマーカー/ポイズン。

**SubWeaponAction.Throw()** で放物線投擲 → 起爆 → InkPaintService.PaintAt + AOEダメージ。

### スペシャル 19 種 (`SpecialData` SO)

UltraShot/MultiMissile/InkStorm/BooyahBomb/KillerWail/UltraStamp/TripleTornado/Reefslider/Kraken/UltraStomp/InkVac/Inkjet/BigBubbler/Wavebreaker/InkVac2/CrabTank/TacticoolerStand/SuperChump/TripleInkstrike。

**SpecialAction.TryActivate()** がカテゴリ別に分岐:
- UltraShot: 9 発螺旋弾 + 大型 VFX
- MultiMissile: 15 発放物線ミサイル + 着弾爆発
- InkStorm: 3 地点同時インク雨 (8 秒継続)
- BooyahBomb: 巨大 3 重爆発 + 周辺ダメージ + 8 連鎖 VFX
- KillerWail: 12 発扇形 + 5 波動 Ring
- UltraStamp: 20 マス突進塗装
- TripleTornado: 3 地点竜巻 + 螺旋 VFX
- Reefslider: 12 マス長距離突撃 + 爆発
- CrabTank: 30 発バースト + カノン砲 2 発

### 充電仕様

塗装着弾時 `chargeAmount = PaintRadius² × 0.012` をオーナーの SpecialAction.AddCharge に加算。ゲージ満タンで Q 発動可能。

---

## ステージ構成

### メインステージ (`Stage_Ground`)

- **サイズ**: 100m × 60m
- **PaintableSurface 解像度**: 2048²
- **多層構造**:
  - 中央タワー 3 層 (下:木目 / 中:ピンク六角ハニカム / 上:黄プラットフォーム)
  - 8 柱で支持
  - 上層に緑ネオンリング + 黄 Glow
- **螺旋階段**: タワー周囲を 8 段で登る
- **4 方向ハイランド**: 東西南北、それぞれ異色ネオン縁(オレンジ/青/ピンク/緑)
- **スロープ**: 中央から 4 方向への斜面
- **ジャンプパッド**: 6 箇所(色違いパルス発光)
- **移動床**: 4 箇所(上下往復)
- **回転柱**: 4 箇所(障害物)
- **ネオン柱**: 12 本(壁際、12 色)
- **壁ネオン縁**: 4 辺(マゼンタ/シアン/オレンジ/青)
- **パイプ装飾**: 12 本(壁伝い縦長、Metal テクスチャ)
- **看板**: 4 枚(発光、空中、6 倍 Emission)
- **スポーン地点**: ±45m 両端、4m 円盤
- **遠景**: 都市シルエット 24 棟(発光窓、北南両側)

### サブステージ

- **マンタマリア号** (z=30): 船型(船底 + デッキ + 船首船尾 + 船橋 + 3 マスト + 3 帆 + ピンク/シアンネオン舷)
- **ハンマーヘッドブリッジ** (z=-30): 鉄骨橋(14×4m + 両壁 + 4 柱 + 巨大梁 + シアン/ピンクネオン)
- **港街** (z=70): 桟橋 + 倉庫 2 棟 + クレーン 2 基 + コンテナ 4 個 + 提灯 12 個 + 海 + ピンク/青舷ネオン

### ライティング

- Directional Light: 1.8 強度、暖色(0.85, 0.7)、35°/-50° 角度
- AmbientMode: Trilight
- SkyColor: (0.55, 0.50, 0.85) 紫紺
- Skybox: Procedural、AtmosphereThickness 0.5、Exposure 1.5(夕暮れ風)
- Fog: ExponentialSquared、紫色(0.35, 0.30, 0.55)、density 0.012

---

## ギミック

`StageGimmick.cs` に統合定義:

- **JumpPad**: OnTriggerEnter で `CharacterController.SimpleMove(Vector3.up * Power)` ジャンプ加速
- **MovingPlatform**: `Mathf.PingPong` で `Distance` 往復、`Speed` 速度
- **RotatingObject**: `transform.Rotate(0, Speed*dt, 0)`
- **KillZone**: OnTriggerEnter で PlayerHealth.TakeDamage(999)
- **PulseGlow**: Material.SetColor("_EmissionColor", baseCol * Mathf.Lerp(min,max,sin(time)))

---

## HUD・UI

### uGUI HUD (旧版、互換維持)

`HUDCanvas` 配下に Canvas + Image + TMPro。`HUDManager.cs` で参照配線。

### UIToolkit HUD (新版、推奨)

`UIToolkitHUD` GameObject に UIDocument + PanelSettings + SplatoonHUDToolkit.cs。
sortingOrder 200 で uGUI HUD より上に重ねる。

### 構成要素

- 上中央: Timer (90px、太字、黒丸背景、残30秒で赤化)
- 上中央下: ScoreBar (両側 Lerp、LeadText)
- 左下: InkTank (角丸、INK% + Height Lerp)
- 右下: SpecialGauge (円形、Scale Lerp、SPECIAL ラベル)
- 右上: WeaponName
- 中央上: CenterNotice (試合終了「YOU WIN!」、SplashNotification)
- 下部: TeammateStatus (4 スロット)
- 全画面: ScreenDamageOverlay (被弾赤フラッシュ)
- 全画面: MapPanel (Tab 長押し、塗装 RT + マーカー)
- 全画面: ModeSelectorPanel (ESC、Mode/Weapon 切替)
- 右側: KillFeed (チーム色付ログ)

### StartMenu (UIToolkit)

タイトル「SPLAT BATTLE」 + 4 ボタン (START/HOWTO/SETTINGS/QUIT)。

---

## ゲームモード

`MatchModeBase` 派生 5 種 (State / Strategy):

- **TurfWarMode** (ナワバリ): 床塗装面積最大チーム勝利、3 分継続
- **SplatZonesMode** (ガチエリア): エリア占有率 50% でカウント減、ノックアウト 100
- **TowerControlMode** (ガチヤグラ): 中央タワー乗車で敵ゴール方向自動移動
- **RainmakerMode** (ガチホコ): ホコ保持で敵ゴール接近距離でカウント
- **ClamBlitzMode** (ガチアサリ): MVP 簡略(塗装率代用)

`TurfWarMatchManager` が 3 分タイマー + ScoreCalculator + EndMatch 判定を担当。

`ScoreCalculator`:
- Stage_Ground.MaskRT を 64×64 にダウンサンプル
- AsyncGPUReadback で非同期 CPU 読戻し
- 各 RGBA チャネル輝度平均 = チーム塗装率

---

## 操作コマンド

| キー | 動作 |
|------|------|
| **WASD** | 移動 |
| **マウス** | カメラ・エイム |
| **左クリック** | メインウェポン発射 (連射) |
| **右クリック** | サブウェポン投擲 |
| **Shift (長押し)** | イカ潜伏(スイム形態、自軍インクで 1.5 倍速) |
| **Space** | ジャンプ |
| **Q** | スペシャル発動(ゲージ満タン時) |
| **Tab (長押し)** | マップ画面 + クリック地点へスーパージャンプ |
| **ESC** | ポーズメニュー(モード/武器切替) |
| **R** | カメラリセット |
| **Ctrl** | スコープ(チャージャー時) |
| **E** | カモン(This Way) |
| **F** | ナイス(Booyah) |

---

## セットアップ

### 必要環境

- Unity Hub
- Unity Editor 6000.3.11f1
- Windows 10/11 (テスト済)
- GPU: GTX 1060 相当以上推奨(URP RenderTexture 2048²)

### 起動手順

```bash
# 1. Unity Hub でプロジェクトを開く
#    パス: E:\GitHub\Splatoon\Splatoon_Unity

# 2. 自動で Package インポート (約 3 分)

# 3. シーン: Assets/Splatoon/Scenes/MVP_TurfWar.unity を開く

# 4. Edit > Project Settings > Player > Resolution and Presentation
#    "Run In Background" を ON (必須、コルーチン進行のため)

# 5. ▶ Play ボタン押下

# 6. UIToolkit StartMenu が表示 → START BATTLE クリック

# 7. 試合開始(WASD で移動・左クリック発射)
```

### トラブルシューティング

- **インクが塗れない** → InkPaintManager の Shader 参照が null。`Assets/Splatoon/Shaders/*.shader` が Splatoon 名前空間でロード可能か確認
- **キャラが床に埋まる** → PlayerController.Awake で HumanModel/SquidModel 自動取得が走る。手動で localPosition.y = HumanColliderHeight / 2 確認
- **BOT が動かない** → Edit > Project Settings > Player > **Run In Background = ON**
- **エラー大量** → 過去 ParticleSystem duration エラー解決済 (`ps.Stop() → 設定 → ps.Play()` パターン)

---

## 開発履歴

### 採用した参考資料

- [VFX Mike: Splatoon in Unity](http://vfxmike.blogspot.com/2017/04/splatoon-in-unity.html) — インク塗装の核心解読
- [Mix and Jam: Splatoon-Ink (MIT)](https://github.com/mixandjam/Splatoon-Ink) — 起点リポジトリ
- [Bronson Zgeb: URP Metaball Tutorial](https://bronsonzgeb.com/index.php/2021/03/15/mix-and-jam-splatoons-ink-system/)
- [Inkipedia](https://splatoonwiki.org/) — 世界観・モード・武器仕様
- [Leanny Splatoon 3 Database](https://leanny.github.io/splat3/database.html) — 武器パラメータ生データ
- [Andrew Cassidy: SDF Antialiasing](https://drewcassidy.me/2020/06/26/sdf-antialiasing/) — DDX/DDY エッジ処理

### 実装統計

- **C# スクリプト**: 44 本
- **シェーダ**: 3 本 (URP HLSL)
- **UXML/USS**: 2 セット
- **ScriptableObject**: 52 個 (武器 11 + サブ 15 + スペシャル 19 + 物理 1 + テクスチャ 6)
- **Material**: 5+ (PaintableSurface + Stage + Player + Bullet 等)
- **シーン**: 1 (MVP_TurfWar.unity)
- **総実装時間**: 約 36 時間 (Claude 自走)
- **行数**: 約 6500 行 (C# + Shader + UXML/USS 合計)

### 既知の制限

1. **VRoid キャラ未差替** — 現状プリミティブ組立(頭+胴+腕+脚+触手+目+靴+銃)
2. **ネット対戦未実装** — Netcode for GameObjects 未統合(MVP は vs BOT)
3. **本家アセット未使用** — フォント/モデル/音源/UI 全て自作 or プロシージャル生成
4. **PC のみ** — Switch/Steam ビルド未対応
5. **マップ画面の MaskRT 表示** — Cube デフォルト UV2 が 6 面分割のためマップが面ごとに分かれて表示

### 法的注記再掲

本プロジェクトは Splatoon (任天堂株式会社) を題材にした **個人学習用 fan-art / クローン**。任天堂の著作物・商標・固有名詞・アセットは一切使用していないが、メカニクス・世界観を参考にしている。**商用利用・公開配布・YouTube 動画化等は禁止**。Unity プロジェクト改変・C# コード参考は学習用途のみ。

---

## ライセンス

個人プロジェクト、非公開。コードは Claude が生成した派生物として、参考にしたオープンソース (Mix and Jam = MIT) に準じた扱い。商用化・再配布禁止。

---

**Generated by Claude via MCP for Unity — 2026-05-15**
