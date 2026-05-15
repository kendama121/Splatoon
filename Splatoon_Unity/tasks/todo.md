# Splatoon Unity完全再現 タスクリスト

> 参照: [docs/Implementation_Plan.md](../docs/Implementation_Plan.md)
> CLAUDE.md準拠で計画→確認→実装の流れ

---

## Phase 1: プロジェクト基盤

- [ ] Unity Editorバージョン確認(Unity 6 LTS 推奨)
- [ ] 必要パッケージ追加
  - [ ] Cinemachine 3.x
  - [ ] ProBuilder
  - [ ] ProGrids
  - [ ] DOTween (Asset Store)
  - [ ] TextMeshPro (デフォルト)
- [ ] フォルダ構造作成
  - [ ] `Assets/Splatoon/Domain/`
  - [ ] `Assets/Splatoon/Application/`
  - [ ] `Assets/Splatoon/Infrastructure/`
  - [ ] `Assets/Splatoon/Presentation/`
  - [ ] `Assets/Splatoon/Data/` (ScriptableObject)
  - [ ] `Assets/Splatoon/Shaders/`
  - [ ] `Assets/Splatoon/VFX/`
  - [ ] `Assets/Splatoon/UI/`
  - [ ] `Assets/Splatoon/Audio/`
- [ ] テストステージ作成(ProBuilder、20×10m平面+簡単な壁)
- [ ] PlayerPhysicsConfig.asset (ScriptableObject) 物理値定義

## Phase 2: プレイヤー基本動作

- [ ] InputSystem「Splatoon」Action Map 追加
- [ ] PlayerController.cs (CharacterController + マウスキーボード)
- [ ] Cinemachine ThirdPersonFollow カメラ設定
- [ ] ヒト/イカ2モデル切替システム
- [ ] イカ⇔ヒト変身アニメ(中間Blend 8-12F)
- [ ] 移動速度切替(ヒト3.024m/s / イカ6.048m/s)
- [ ] ジャンプ(Space)
- [ ] イカロール(Shift+逆方向、無敵F付与)
- [ ] イカノボリ(壁登り中Shift離し)
- [ ] スーパージャンプ(マップ画面→チャージ→飛行→着地)
- [ ] HP・インクタンク・スペシャルゲージの内部状態
- [ ] スプラット→リスポーン サイクル(8.5秒)

## Phase 3: インクシステム(最重要)

- [ ] Mix and Jam リポクローン → Unity 6 URP移植テスト
- [ ] World Position bake Pass (Pass 1)
- [ ] Splat Blit Pass + ピンポンバッファ (Pass 2)
- [ ] SDFスプラットアトラス作成(円/ブロブ/飛沫/ローラー/フット)
- [ ] チームRGB分離(R/G/B/A = チーム0/1/2/3)
- [ ] Score Downsample Pass 256→4×4 (Pass 3)
- [ ] AsyncGPUReadback スコア集計
- [ ] Surface Render Shader (Pass 4) — SDF + DDX/DDY AA + 法線生成
- [ ] World Tangent / Binormal bakeパス追加(法線品質向上)
- [ ] Bleed Pass(UVシーム dilation)
- [ ] CPUインクミラー(256² Texture2D) + 移動判定API
- [ ] 自軍インクで速度UP/敵軍でダメージ実装

## Phase 4: 武器システム

- [ ] WeaponData ScriptableObject 設計
- [ ] 弾道3ステートモデル(Straight → Brake → Free)実装
- [ ] スプラシューター実装(基準武器)
- [ ] わかばシューター、N-ZAP、.52ガロン、.96ガロン
- [ ] ローラー(振り + コロガシ)
- [ ] チャージャー(チャージ + スコープ + Ctrl対応)
- [ ] スロッシャー(放物線)
- [ ] スピナー(チャージ式連射)
- [ ] マニューバー(スライド + ガード)
- [ ] シェルター(散弾 + 傘 + パージ)
- [ ] ブラスター(爆発弾)
- [ ] フデ(振り + 塗り進み)
- [ ] ストリンガー(3弦同時)
- [ ] ワイパー(ヨコ斬り + タメ斬り)
- [ ] サブウェポン全14種
- [ ] スペシャル優先4種(ウルトラショット/ナイスダマ/カニタンク/メガホン)

## Phase 5: VFX・ビジュアル仕上げ

- [ ] トゥーンシェーダ導入(NiloCat URPToon)
- [ ] アウトライン(Inverted Hull)
- [ ] URP Volume プリセット(Bloom/MotionBlur/ColorAdjustments)
- [ ] 武器発射エフェクト(マズルフラッシュ + 飛沫 + Trail)
- [ ] 着弾エフェクト(拡大→波紋→定着)
- [ ] メタボール風融合(Bronson Zgeb式 sphere tracing)
- [ ] スプラット時の散り方(インク弾け15-30F)
- [ ] スーパージャンプ演出
- [ ] スペシャル発動演出(各4種)

## Phase 6: UI/オーディオ/ゲームロジック

- [ ] HUD Canvas構築
  - [ ] 上部塗り進捗バー
  - [ ] 右上タイマー
  - [ ] 左下インクタンク
  - [ ] スペシャルゲージ
  - [ ] 武器アイコン
  - [ ] 下部チームステータス
- [ ] マップ画面(Tab長押し、塗り状況可視化)
- [ ] TextMeshPro + Project Paintball フォント
- [ ] DOTween UIアニメ
- [ ] ナワバリバトル ロジック(3分タイマー、スコア集計、勝敗判定)
- [ ] 試合開始/終了演出
- [ ] AudioMixer階層構築
- [ ] 全SE/BGM配置(オリジナル音源)
- [ ] 3D音響設定(Spatial Blend、距離減衰)

## Phase 7: 拡張(将来)

- [ ] ガチエリア/ヤグラ/ホコ/アサリ(State/Strategy)
- [ ] サーモンラン(PvE)
- [ ] Netcode for GameObjects 統合
- [ ] マッチメイキング・ロビー
- [ ] ギアシステム + ギアパワー

---

## 進捗ログ

### 2026-05-14
- 調査フェーズ完了。docs/Splatoon_Knowledge_Base.md、Weapon_Database.md、Implementation_Plan.md 整備済
- 次: Phase 1 着手のユーザー承認待ち

---

## レビューセクション

(各Phase完了時に変更内容と教訓を記録)
