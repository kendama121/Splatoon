# INKWAVE 13画面 Unity UI Toolkit 移植 TODO

## 基盤(済)
- [x] InkwaveCommon.uss (styles.css 移植)
- [x] InkwaveScreenManager.cs (13画面 enum/Show/HideAll/TransitionTo)
- [x] InkwaveScreenBase.cs (UIDocument基底 抽象 BindUI)

## 共通拡張 USS
- [ ] InkwaveAnimations.uss (iw-anim-blink/pop/glow/tilt/float/arrow/shimmer/slide)
- [ ] InkwaveExtras.uss (iw-noise/iw-scroll/iw-card.cut/iw-stripe追加クラス)

## 13画面 UXML + Controller

### A. Pre-Game (5)
- [ ] Screen_Title.uxml + .cs (タイトル/PRESS START/News流れ/Sticker)
- [ ] Screen_Menu.uxml + .cs (4モード選択/プロフィール/ローテ/クイック)
- [ ] Screen_Character.uxml + .cs (装備スロット/プレビュー/タブ:ギア/スキン/パワー)
- [ ] Screen_Weapon.uxml + .cs (カテゴリ/ブキリスト/ヒーロー/ステ4種/サブ/SP)
- [ ] Screen_Lobby.uxml + .cs (2チーム/4人タイル/チャット/準備ボタン)

### B. In-Game (3)
- [ ] Screen_Loading.uxml + .cs (VS構図/進捗5段/Tips循環)
- [ ] Screen_HUD.uxml + .cs (上部TurfBar/照準/味方敵/キル/ミニマップ/インクタンク/SP/ブキ)
- [ ] Screen_Map.uxml + .cs (左:SJ先4人/中央:SVGマップ/右:目標/ピン/勝率)

### C. Post-Action (5)
- [ ] Screen_Respawn.uxml + .cs (キルカム/カウントダウン円/殺害者カード)
- [ ] Screen_Pause.uxml + .cs (PAUSED透かし/5メニュー/3カード)
- [ ] Screen_Results.uxml + .cs (ヒーローバナー/2チームテーブル/MVP)
- [ ] Screen_Training.uxml + .cs (左:ドリル5/中央:練習場/右:統計)
- [ ] Screen_Settings.uxml + .cs (5タブ/設定行/キーバインド)

## 配線
- [ ] InkwaveCanvas Scene (13個 GameObject + UIDocument + PanelSettings)
- [ ] InkwaveScreenManager に 13個参照
- [ ] 既存ゲーム(InkPaintManager/WeaponShooter/PlayerHealth) と HUD連携

## アート移植
- [ ] InkChar VisualElement(SVG → USS背景画像 or generateVisualContent)
- [ ] WeaponArt/ModeArt/StageIso 同上
- [ ] Splat (ランダム border-radius 生成)
- [ ] TurfBar (両端矢じり)
- [ ] Avatar (SVGポートレート)

## 検証
- [ ] 13画面表示・遷移
- [ ] HUD と既存ゲーム連携
- [ ] インタラクション全動作
- [ ] アニメ再生
