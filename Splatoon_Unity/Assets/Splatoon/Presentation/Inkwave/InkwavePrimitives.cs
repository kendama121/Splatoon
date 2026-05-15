using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave
{
    // ============================================================
    // INKWAVE Primitives — VisualElement 派生(UXML対応)
    // ============================================================

    /// <summary>
    /// ロゴマーク。INK/WAVE 表記の小さい行内ロゴ。
    /// </summary>
    public class IwLogo : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwLogo, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        /// <summary>コンストラクタ。ロゴマーク+テキストを構築する。</summary>
        public IwLogo()
        {
            AddToClassList("iw-logo");
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            var mark = new VisualElement();
            mark.AddToClassList("iw-logo-mark");
            mark.style.marginRight = 8;
            Add(mark);

            var label = new Label("INK/WAVE");
            label.style.fontSize = 14;
            label.style.letterSpacing = 3;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(label);
        }
    }

    /// <summary>
    /// スプラット(インクのしぶき)。ランダムなborder-radiusで有機形状。
    /// </summary>
    public class IwSplat : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwSplat, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlIntAttributeDescription _splatSize = new() { name = "splat-size", defaultValue = 120 };
            UxmlColorAttributeDescription _splatColor = new() { name = "splat-color", defaultValue = new Color(1f, 0.106f, 0.42f) };
            UxmlFloatAttributeDescription _rotate = new() { name = "rotate", defaultValue = 0f };
            UxmlFloatAttributeDescription _splatOpacity = new() { name = "splat-opacity", defaultValue = 1f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var splat = (IwSplat)ve;
                splat.SetShape(_splatSize.GetValueFromBag(bag, cc),
                               _splatColor.GetValueFromBag(bag, cc),
                               _rotate.GetValueFromBag(bag, cc),
                               _splatOpacity.GetValueFromBag(bag, cc));
            }
        }

        /// <summary>スプラット形状を設定する。</summary>
        public void SetShape(int splatSize, Color splatColor, float rotate, float splatOpacity)
        {
            style.width = splatSize;
            style.height = splatSize;
            style.backgroundColor = splatColor;
            style.opacity = splatOpacity;
            style.rotate = new StyleRotate(new Rotate(rotate));

            // ランダムなborder-radius (有機形状)
            var rand = new System.Random(splatSize * 7 + (int)(rotate * 31));
            float tl = 30 + rand.Next(50);
            float tr = 30 + rand.Next(50);
            float br = 30 + rand.Next(50);
            float bl = 30 + rand.Next(50);
            style.borderTopLeftRadius = Length.Percent(tl);
            style.borderTopRightRadius = Length.Percent(tr);
            style.borderBottomRightRadius = Length.Percent(br);
            style.borderBottomLeftRadius = Length.Percent(bl);
        }

        public IwSplat()
        {
            SetShape(120, new Color(1f, 0.106f, 0.42f), 0f, 1f);
        }
    }

    /// <summary>
    /// アバターアイコン。チーム色のフレーム+簡易ポートレート。
    /// </summary>
    public class IwAvatar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwAvatar, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _team = new() { name = "team", defaultValue = "a" };
            UxmlIntAttributeDescription _avatarSize = new() { name = "avatar-size", defaultValue = 56 };
            UxmlStringAttributeDescription _avatarName = new() { name = "avatar-name", defaultValue = "カイ-07" };
            UxmlBoolAttributeDescription _showLabel = new() { name = "show-label", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var av = (IwAvatar)ve;
                av.Configure(_team.GetValueFromBag(bag, cc),
                             _avatarSize.GetValueFromBag(bag, cc),
                             _avatarName.GetValueFromBag(bag, cc),
                             _showLabel.GetValueFromBag(bag, cc));
            }
        }

        VisualElement _portrait;
        Label _nameLabel;

        /// <summary>アバター設定。</summary>
        public void Configure(string team, int avatarSize, string avatarName, bool showLabel)
        {
            Clear();
            style.flexDirection = FlexDirection.Column;
            style.alignItems = Align.Center;

            _portrait = new VisualElement();
            _portrait.style.width = avatarSize;
            _portrait.style.height = avatarSize;
            _portrait.style.borderTopLeftRadius = 4;
            _portrait.style.borderTopRightRadius = 4;
            _portrait.style.borderBottomLeftRadius = 4;
            _portrait.style.borderBottomRightRadius = 4;
            Color teamColor = team == "b" ? new Color(0f, 0.898f, 1f) : new Color(1f, 0.106f, 0.42f);
            Color teamDim = team == "b" ? new Color(0.031f, 0.396f, 0.463f) : new Color(0.541f, 0.039f, 0.227f);
            _portrait.style.backgroundColor = teamDim;
            _portrait.style.borderTopColor = teamColor;
            _portrait.style.borderBottomColor = teamColor;
            _portrait.style.borderLeftColor = teamColor;
            _portrait.style.borderRightColor = teamColor;
            _portrait.style.borderTopWidth = 1;
            _portrait.style.borderBottomWidth = 1;
            _portrait.style.borderLeftWidth = 1;
            _portrait.style.borderRightWidth = 1;

            // 顔の簡易表現(Painter2D)
            _portrait.generateVisualContent += ctx => DrawFace(ctx, _portrait.contentRect, teamColor);
            Add(_portrait);

            if (showLabel)
            {
                _nameLabel = new Label(avatarName);
                _nameLabel.style.fontSize = 10;
                _nameLabel.style.color = new Color(0.541f, 0.541f, 0.572f);
                _nameLabel.style.marginTop = 6;
                Add(_nameLabel);
            }
        }

        /// <summary>顔Path描画(イカ風シルエット)。</summary>
        void DrawFace(MeshGenerationContext ctx, Rect rect, Color teamColor)
        {
            var p = ctx.painter2D;
            // 顔楕円
            p.fillColor = new Color(0.051f, 0.051f, 0.063f);
            p.BeginPath();
            float cx = rect.width * 0.5f;
            float cy = rect.height * 0.4f;
            p.Arc(new Vector2(cx, cy), rect.width * 0.18f, 0f, 360f);
            p.Fill();
            // 体
            p.BeginPath();
            p.MoveTo(new Vector2(rect.width * 0.22f, rect.height));
            p.LineTo(new Vector2(rect.width * 0.22f, rect.height * 0.7f));
            p.QuadraticCurveTo(new Vector2(cx, rect.height * 0.55f), new Vector2(rect.width * 0.78f, rect.height * 0.7f));
            p.LineTo(new Vector2(rect.width * 0.78f, rect.height));
            p.ClosePath();
            p.Fill();
            // 触手(両側)
            p.strokeColor = teamColor;
            p.lineWidth = 3f;
            p.BeginPath();
            p.MoveTo(new Vector2(rect.width * 0.32f, rect.height * 0.28f));
            p.QuadraticCurveTo(new Vector2(rect.width * 0.24f, rect.height * 0.4f), new Vector2(rect.width * 0.28f, rect.height * 0.58f));
            p.Stroke();
            p.BeginPath();
            p.MoveTo(new Vector2(rect.width * 0.68f, rect.height * 0.28f));
            p.QuadraticCurveTo(new Vector2(rect.width * 0.76f, rect.height * 0.4f), new Vector2(rect.width * 0.72f, rect.height * 0.58f));
            p.Stroke();
            // 目(横長矩形)
            p.fillColor = teamColor;
            p.BeginPath();
            p.MoveTo(new Vector2(rect.width * 0.40f, rect.height * 0.36f));
            p.LineTo(new Vector2(rect.width * 0.46f, rect.height * 0.36f));
            p.LineTo(new Vector2(rect.width * 0.46f, rect.height * 0.39f));
            p.LineTo(new Vector2(rect.width * 0.40f, rect.height * 0.39f));
            p.ClosePath();
            p.Fill();
            p.BeginPath();
            p.MoveTo(new Vector2(rect.width * 0.54f, rect.height * 0.36f));
            p.LineTo(new Vector2(rect.width * 0.60f, rect.height * 0.36f));
            p.LineTo(new Vector2(rect.width * 0.60f, rect.height * 0.39f));
            p.LineTo(new Vector2(rect.width * 0.54f, rect.height * 0.39f));
            p.ClosePath();
            p.Fill();
        }

        public IwAvatar()
        {
            Configure("a", 56, "カイ-07", true);
        }
    }

    /// <summary>
    /// プログレスバー。背景+フィル+割合表示。
    /// </summary>
    public class IwBar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwBar, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription _pct = new() { name = "pct", defaultValue = 50f };
            UxmlColorAttributeDescription _fillColor = new() { name = "fill-color", defaultValue = new Color(1f, 0.106f, 0.42f) };
            UxmlIntAttributeDescription _barHeight = new() { name = "bar-height", defaultValue = 6 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var b = (IwBar)ve;
                b.SetBar(_pct.GetValueFromBag(bag, cc),
                         _fillColor.GetValueFromBag(bag, cc),
                         _barHeight.GetValueFromBag(bag, cc));
            }
        }

        VisualElement _fill;

        /// <summary>バー設定。</summary>
        public void SetBar(float pct, Color fillColor, int barHeight)
        {
            Clear();
            style.height = barHeight;
            style.backgroundColor = new Color(0.165f, 0.165f, 0.2f);
            style.overflow = Overflow.Hidden;
            style.position = Position.Relative;

            _fill = new VisualElement();
            _fill.style.position = Position.Absolute;
            _fill.style.left = 0;
            _fill.style.top = 0;
            _fill.style.bottom = 0;
            _fill.style.width = Length.Percent(Mathf.Clamp(pct, 0f, 100f));
            _fill.style.backgroundColor = fillColor;
            Add(_fill);
        }

        /// <summary>割合更新。</summary>
        public void SetPct(float pct)
        {
            if (_fill != null) _fill.style.width = Length.Percent(Mathf.Clamp(pct, 0f, 100f));
        }

        public IwBar()
        {
            SetBar(50f, new Color(1f, 0.106f, 0.42f), 6);
        }
    }

    /// <summary>
    /// 塗りバー(両陣営塗り率)。中央仕切り付き、両端矢じり形状。
    /// </summary>
    public class IwTurfBar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwTurfBar, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription _a = new() { name = "a-pct", defaultValue = 50f };
            UxmlFloatAttributeDescription _b = new() { name = "b-pct", defaultValue = 50f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var tb = (IwTurfBar)ve;
                tb.SetTurf(_a.GetValueFromBag(bag, cc), _b.GetValueFromBag(bag, cc));
            }
        }

        float _aPct, _bPct;

        /// <summary>塗り率設定。</summary>
        public void SetTurf(float aPct, float bPct)
        {
            _aPct = aPct;
            _bPct = bPct;
            MarkDirtyRepaint();
        }

        public IwTurfBar()
        {
            style.height = 28;
            style.width = 540;
            generateVisualContent += OnPaint;
            SetTurf(50f, 50f);
        }

        /// <summary>Painter2Dでバー描画。</summary>
        void OnPaint(MeshGenerationContext ctx)
        {
            var p = ctx.painter2D;
            var r = contentRect;
            float arrow = 14f;

            // 背景(六角形)
            p.fillColor = new Color(0.114f, 0.114f, 0.141f);
            p.strokeColor = new Color(0.204f, 0.204f, 0.247f);
            p.lineWidth = 1f;
            p.BeginPath();
            p.MoveTo(new Vector2(arrow, 0));
            p.LineTo(new Vector2(r.width - arrow, 0));
            p.LineTo(new Vector2(r.width, r.height * 0.5f));
            p.LineTo(new Vector2(r.width - arrow, r.height));
            p.LineTo(new Vector2(arrow, r.height));
            p.LineTo(new Vector2(0, r.height * 0.5f));
            p.ClosePath();
            p.Fill();
            p.Stroke();

            // チームA塗り(左)
            float aWidth = (r.width - 8f) * (_aPct / 100f);
            p.fillColor = new Color(1f, 0.106f, 0.42f);
            p.BeginPath();
            p.MoveTo(new Vector2(10f + 4f, 4f));
            p.LineTo(new Vector2(4f + aWidth, 4f));
            p.LineTo(new Vector2(4f + aWidth, r.height - 4f));
            p.LineTo(new Vector2(10f + 4f, r.height - 4f));
            p.LineTo(new Vector2(4f, r.height * 0.5f));
            p.ClosePath();
            p.Fill();

            // チームB塗り(右)
            float bWidth = (r.width - 8f) * (_bPct / 100f);
            p.fillColor = new Color(0f, 0.898f, 1f);
            p.BeginPath();
            p.MoveTo(new Vector2(r.width - 4f - bWidth, 4f));
            p.LineTo(new Vector2(r.width - 14f, 4f));
            p.LineTo(new Vector2(r.width - 4f, r.height * 0.5f));
            p.LineTo(new Vector2(r.width - 14f, r.height - 4f));
            p.LineTo(new Vector2(r.width - 4f - bWidth, r.height - 4f));
            p.ClosePath();
            p.Fill();

            // 中央仕切り
            p.fillColor = new Color(0.957f, 0.957f, 0.941f);
            p.BeginPath();
            p.MoveTo(new Vector2(r.width * 0.5f - 1f, -6f));
            p.LineTo(new Vector2(r.width * 0.5f + 1f, -6f));
            p.LineTo(new Vector2(r.width * 0.5f + 1f, r.height + 6f));
            p.LineTo(new Vector2(r.width * 0.5f - 1f, r.height + 6f));
            p.ClosePath();
            p.Fill();
        }
    }

    /// <summary>
    /// ステッカー(斜めシール)。角度+背景色+テキスト+影。
    /// </summary>
    public class IwSticker : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwSticker, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _text = new() { name = "text", defaultValue = "FRESH" };
            UxmlColorAttributeDescription _bgColor = new() { name = "bg-color", defaultValue = new Color(0.957f, 0.957f, 0.941f) };
            UxmlColorAttributeDescription _textColor = new() { name = "text-color", defaultValue = new Color(0.051f, 0.051f, 0.063f) };
            UxmlFloatAttributeDescription _rotate = new() { name = "rotate", defaultValue = -4f };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var s = (IwSticker)ve;
                s.SetSticker(_text.GetValueFromBag(bag, cc),
                             _bgColor.GetValueFromBag(bag, cc),
                             _textColor.GetValueFromBag(bag, cc),
                             _rotate.GetValueFromBag(bag, cc));
            }
        }

        Label _label;

        /// <summary>シール設定。</summary>
        public void SetSticker(string text, Color bgColor, Color textColor, float rotate)
        {
            Clear();
            AddToClassList("iw-sticker");
            style.backgroundColor = bgColor;
            style.color = textColor;
            style.rotate = new StyleRotate(new Rotate(rotate));
            style.paddingLeft = 14;
            style.paddingRight = 14;
            style.paddingTop = 8;
            style.paddingBottom = 8;
            style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
            style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
            style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
            style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
            style.borderTopWidth = 3;
            style.borderBottomWidth = 3;
            style.borderLeftWidth = 3;
            style.borderRightWidth = 3;
            style.borderTopLeftRadius = 4;
            style.borderTopRightRadius = 4;
            style.borderBottomLeftRadius = 4;
            style.borderBottomRightRadius = 4;

            _label = new Label(text);
            _label.style.fontSize = 14;
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;
            Add(_label);
        }

        public IwSticker()
        {
            SetSticker("FRESH", new Color(0.957f, 0.957f, 0.941f), new Color(0.051f, 0.051f, 0.063f), -4f);
        }
    }

    /// <summary>
    /// キーバインドキャップ。
    /// </summary>
    public class IwKey : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwKey, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _text = new() { name = "key-text", defaultValue = "E" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var k = (IwKey)ve;
                k.SetText(_text.GetValueFromBag(bag, cc));
            }
        }

        Label _label;

        /// <summary>キー文字設定。</summary>
        public void SetText(string text)
        {
            Clear();
            AddToClassList("iw-kbd");
            _label = new Label(text);
            _label.style.fontSize = 11;
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;
            _label.style.color = new Color(0.957f, 0.957f, 0.941f);
            Add(_label);
        }

        public IwKey()
        {
            SetText("E");
        }
    }

    /// <summary>
    /// チップ(英大文字ラベル)。
    /// </summary>
    public class IwChip : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwChip, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _text = new() { name = "chip-text", defaultValue = "LIVE" };
            UxmlBoolAttributeDescription _live = new() { name = "live", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var c = (IwChip)ve;
                c.SetChip(_text.GetValueFromBag(bag, cc), _live.GetValueFromBag(bag, cc));
            }
        }

        /// <summary>チップ設定。</summary>
        public void SetChip(string text, bool live)
        {
            Clear();
            AddToClassList("iw-chip");
            if (live) AddToClassList("iw-chip-live");

            var dot = new VisualElement();
            dot.AddToClassList("iw-chip-dot");
            Add(dot);

            var label = new Label(text);
            Add(label);
        }

        public IwChip()
        {
            SetChip("LIVE", false);
        }
    }

    /// <summary>
    /// インクキャラ(立ち絵)。Painter2DでフルボディSVG忠実再現(art.jsx 240x440 viewBox)。
    /// </summary>
    public class IwInkChar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwInkChar, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _pose = new() { name = "pose", defaultValue = "idle" };
            UxmlStringAttributeDescription _team = new() { name = "team", defaultValue = "a" };
            UxmlStringAttributeDescription _weapon = new() { name = "weapon", defaultValue = "shooter" };
            UxmlIntAttributeDescription _charSize = new() { name = "char-size", defaultValue = 240 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var c = (IwInkChar)ve;
                c.Configure(_pose.GetValueFromBag(bag, cc),
                            _team.GetValueFromBag(bag, cc),
                            _weapon.GetValueFromBag(bag, cc),
                            _charSize.GetValueFromBag(bag, cc));
            }
        }

        string _pose = "idle", _team = "a", _weapon = "shooter";

        /// <summary>キャラ設定。size= 横幅、縦= size*440/240。</summary>
        public void Configure(string pose, string team, string weapon, int charSize)
        {
            _pose = pose; _team = team; _weapon = weapon;
            style.width = charSize;
            style.height = charSize * 440f / 240f;
            MarkDirtyRepaint();
        }

        public IwInkChar()
        {
            style.width = 240;
            style.height = 240 * 440f / 240f;
            generateVisualContent += OnPaint;
        }

        // SVG座標(0-240, 0-440) → element px へ変換
        Vector2 P(Rect r, float vx, float vy) =>
            new Vector2(r.width * (vx + 10f) / 240f, r.height * (vy + 10f) / 440f);
        float SW(Rect r, float w) => w * r.width / 240f;

        /// <summary>art.jsx InkChar の SVG paths 完全再現。</summary>
        void OnPaint(MeshGenerationContext ctx)
        {
            var p = ctx.painter2D;
            var r = contentRect;
            if (r.width < 1f) return;

            // 色定義(art.jsx 同等)
            Color tc = _team == "b" ? new Color(0f, 0.898f, 1f) : new Color(1f, 0.106f, 0.42f);
            Color tcDim = _team == "b" ? new Color(0.031f, 0.396f, 0.463f) : new Color(0.541f, 0.039f, 0.227f);
            Color skin = new Color(0.91f, 0.788f, 0.627f);
            Color hood = new Color(0.114f, 0.114f, 0.141f);
            Color pants = new Color(0.165f, 0.165f, 0.2f);
            Color visor = tc;
            Color bg0 = new Color(0.051f, 0.051f, 0.063f);
            Color white = new Color(0.957f, 0.957f, 0.941f);

            bool aim = _pose == "aim";
            bool victory = _pose == "victory";

            // 影 ellipse(110, 408, 64, 10) — 楕円描画(Path で近似)
            p.fillColor = new Color(0, 0, 0, 0.5f);
            DrawEllipse(p, r, 110, 408, 64, 10);

            // 触手髪 — メイン2本(両側) + サブ2本 + 上1本
            // M70,86 C40,120 38,170 56,210 (stroke 22, gradient tc→tcDim)
            DrawStrokeBezier(p, r, 70, 86, 40, 120, 38, 170, 56, 210, 22, tcDim);
            // M150,86 C180,120 182,170 164,210
            DrawStrokeBezier(p, r, 150, 86, 180, 120, 182, 170, 164, 210, 22, tcDim);
            // M90,60 C70,90 80,150 96,180 (sub stroke 14)
            DrawStrokeBezier(p, r, 90, 60, 70, 90, 80, 150, 96, 180, 14, tcDim);
            // M130,60 C150,90 140,150 124,180
            DrawStrokeBezier(p, r, 130, 60, 150, 90, 140, 150, 124, 180, 14, tcDim);
            // M110,42 C100,28 120,16 130,8 (top stroke 10)
            DrawStrokeBezier(p, r, 110, 42, 100, 28, 120, 16, 130, 8, 10, tc);

            // 頭楕円(110, 105) rx=56 ry=62 fill=skin stroke=bg0 sw=3
            p.fillColor = skin;
            DrawEllipse(p, r, 110, 105, 56, 62);
            DrawEllipseStroke(p, r, 110, 105, 56, 62, 3, bg0);

            // visor band (台形)
            p.fillColor = visor;
            p.BeginPath();
            p.MoveTo(P(r, 58, 98)); p.QuadraticCurveTo(P(r, 110, 86), P(r, 162, 98));
            p.LineTo(P(r, 162, 118)); p.QuadraticCurveTo(P(r, 110, 108), P(r, 58, 118));
            p.ClosePath(); p.Fill();
            // visor stroke
            p.strokeColor = bg0; p.lineWidth = SW(r, 3);
            p.BeginPath();
            p.MoveTo(P(r, 58, 98)); p.QuadraticCurveTo(P(r, 110, 86), P(r, 162, 98));
            p.LineTo(P(r, 162, 118)); p.QuadraticCurveTo(P(r, 110, 108), P(r, 58, 118));
            p.ClosePath(); p.Stroke();

            // visor reflection
            p.fillColor = new Color(white.r, white.g, white.b, 0.5f);
            p.BeginPath();
            p.MoveTo(P(r, 70, 102)); p.LineTo(P(r, 102, 98));
            p.LineTo(P(r, 96, 114)); p.LineTo(P(r, 72, 116));
            p.ClosePath(); p.Fill();

            // 口
            p.strokeColor = bg0; p.lineWidth = SW(r, 3);
            if (victory)
            {
                p.BeginPath();
                p.MoveTo(P(r, 94, 140)); p.QuadraticCurveTo(P(r, 110, 155), P(r, 126, 140));
                p.Stroke();
            }
            else if (aim)
            {
                p.BeginPath();
                p.MoveTo(P(r, 98, 142)); p.LineTo(P(r, 122, 142));
                p.Stroke();
            }
            else
            {
                p.BeginPath();
                p.MoveTo(P(r, 98, 142)); p.QuadraticCurveTo(P(r, 110, 148), P(r, 122, 142));
                p.Stroke();
            }

            // neck rect(98,158 w24 h14)
            p.fillColor = skin;
            DrawRect(p, r, 98, 158, 24, 14);
            DrawRectStroke(p, r, 98, 158, 24, 14, 2, bg0);

            // hoodie path
            p.fillColor = hood;
            p.BeginPath();
            p.MoveTo(P(r, 50, 170));
            p.QuadraticCurveTo(P(r, 60, 166), P(r, 110, 168));
            p.QuadraticCurveTo(P(r, 160, 166), P(r, 170, 170));
            p.LineTo(P(r, 182, 290)); p.LineTo(P(r, 38, 290));
            p.ClosePath(); p.Fill();
            p.strokeColor = bg0; p.lineWidth = SW(r, 3);
            p.BeginPath();
            p.MoveTo(P(r, 50, 170));
            p.QuadraticCurveTo(P(r, 60, 166), P(r, 110, 168));
            p.QuadraticCurveTo(P(r, 160, 166), P(r, 170, 170));
            p.LineTo(P(r, 182, 290)); p.LineTo(P(r, 38, 290));
            p.ClosePath(); p.Stroke();

            // hood collar
            p.fillColor = tcDim;
            p.BeginPath();
            p.MoveTo(P(r, 70, 170));
            p.QuadraticCurveTo(P(r, 110, 184), P(r, 150, 170));
            p.QuadraticCurveTo(P(r, 140, 186), P(r, 110, 188));
            p.QuadraticCurveTo(P(r, 80, 186), P(r, 70, 170));
            p.ClosePath(); p.Fill();

            // strings + balls
            p.strokeColor = tc; p.lineWidth = SW(r, 3);
            p.BeginPath(); p.MoveTo(P(r, 96, 186)); p.LineTo(P(r, 92, 220)); p.Stroke();
            p.BeginPath(); p.MoveTo(P(r, 124, 186)); p.LineTo(P(r, 128, 220)); p.Stroke();
            p.fillColor = tc;
            DrawCircle(p, r, 92, 222, 3);
            DrawCircle(p, r, 128, 222, 3);

            // chest insignia (110,232) rot15 24x24 内12x12穴
            p.fillColor = tc;
            DrawRectCentered(p, r, 110, 232, 24, 24);
            p.fillColor = bg0;
            DrawRectCentered(p, r, 110, 232, 12, 12);

            // pocket (60,252 w100 h30) opacity 0.7
            p.fillColor = new Color(tcDim.r, tcDim.g, tcDim.b, 0.7f);
            DrawRect(p, r, 60, 252, 100, 30);
            DrawRectStroke(p, r, 60, 252, 100, 30, 2, bg0);

            // arms (pose別)
            p.fillColor = hood;
            if (aim)
            {
                // 両腕前方 path
                p.BeginPath();
                p.MoveTo(P(r, 50, 180)); p.LineTo(P(r, 36, 220));
                p.LineTo(P(r, 96, 250)); p.LineTo(P(r, 110, 220));
                p.ClosePath(); p.Fill();
                p.BeginPath();
                p.MoveTo(P(r, 170, 180)); p.LineTo(P(r, 184, 220));
                p.LineTo(P(r, 124, 250)); p.LineTo(P(r, 110, 220));
                p.ClosePath(); p.Fill();
                // hands
                p.fillColor = skin;
                DrawCircle(p, r, 96, 252, 10);
                DrawCircle(p, r, 124, 252, 10);
            }
            else if (victory)
            {
                p.BeginPath();
                p.MoveTo(P(r, 50, 180)); p.LineTo(P(r, 34, 260));
                p.LineTo(P(r, 60, 290)); p.LineTo(P(r, 70, 210));
                p.ClosePath(); p.Fill();
                p.BeginPath();
                p.MoveTo(P(r, 170, 180)); p.LineTo(P(r, 210, 100));
                p.LineTo(P(r, 194, 86)); p.LineTo(P(r, 150, 180));
                p.ClosePath(); p.Fill();
                p.fillColor = skin;
                DrawCircle(p, r, 200, 92, 12);
                DrawCircle(p, r, 48, 292, 12);
            }
            else // idle
            {
                p.BeginPath();
                p.MoveTo(P(r, 50, 180)); p.LineTo(P(r, 34, 290));
                p.LineTo(P(r, 60, 300)); p.LineTo(P(r, 70, 210));
                p.ClosePath(); p.Fill();
                p.BeginPath();
                p.MoveTo(P(r, 170, 180)); p.LineTo(P(r, 186, 290));
                p.LineTo(P(r, 160, 300)); p.LineTo(P(r, 150, 210));
                p.ClosePath(); p.Fill();
                p.fillColor = skin;
                DrawCircle(p, r, 48, 298, 12);
                DrawCircle(p, r, 172, 298, 12);
            }

            // shorts (両足)
            p.fillColor = pants;
            p.BeginPath();
            p.MoveTo(P(r, 44, 290)); p.LineTo(P(r, 60, 372));
            p.LineTo(P(r, 100, 372)); p.LineTo(P(r, 106, 294));
            p.ClosePath(); p.Fill();
            p.BeginPath();
            p.MoveTo(P(r, 176, 290)); p.LineTo(P(r, 160, 372));
            p.LineTo(P(r, 120, 372)); p.LineTo(P(r, 114, 294));
            p.ClosePath(); p.Fill();

            // knee tape
            p.fillColor = tc;
            DrawRect(p, r, 70, 354, 22, 6);
            DrawRect(p, r, 128, 354, 22, 6);

            // legs (skin)
            p.fillColor = skin;
            DrawRect(p, r, 62, 372, 36, 20);
            DrawRect(p, r, 122, 372, 36, 20);

            // sneakers
            p.fillColor = white;
            p.BeginPath();
            p.MoveTo(P(r, 56, 392)); p.LineTo(P(r, 100, 392));
            p.LineTo(P(r, 104, 408)); p.LineTo(P(r, 52, 408));
            p.ClosePath(); p.Fill();
            p.BeginPath();
            p.MoveTo(P(r, 116, 392)); p.LineTo(P(r, 160, 392));
            p.LineTo(P(r, 168, 408)); p.LineTo(P(r, 116, 408));
            p.ClosePath(); p.Fill();
            // sole (tc)
            p.fillColor = tc;
            DrawRect(p, r, 52, 402, 52, 8);
            DrawRect(p, r, 116, 402, 52, 8);

            // 武器(状況別)
            float wx = aim ? 70 : 170;
            float wy = aim ? 232 : 230;
            DrawWeapon(p, r, _weapon, wx, wy, aim ? 1f : 0.85f, aim ? 0 : 20, tc);
        }

        // 武器インライン描画(art.jsx WeaponInline 同等)
        void DrawWeapon(UnityEngine.UIElements.Painter2D p, Rect r, string kind, float baseX, float baseY, float scale, float rotate, Color color)
        {
            Color bg0 = new Color(0.051f, 0.051f, 0.063f);
            Color metal = new Color(0.227f, 0.227f, 0.271f);
            Color metalLight = new Color(0.353f, 0.353f, 0.396f);
            float cosR = Mathf.Cos(rotate * Mathf.Deg2Rad);
            float sinR = Mathf.Sin(rotate * Mathf.Deg2Rad);
            Vector2 T(float x, float y)
            {
                float sx = x * scale, sy = y * scale;
                return P(r, baseX + sx * cosR - sy * sinR, baseY + sx * sinR + sy * cosR);
            }

            if (kind == "shooter")
            {
                p.fillColor = metal;
                p.BeginPath();
                p.MoveTo(T(-8, -6)); p.LineTo(T(112, -6)); p.LineTo(T(112, 14)); p.LineTo(T(-8, 14));
                p.ClosePath(); p.Fill();
                p.fillColor = metalLight;
                p.BeginPath();
                p.MoveTo(T(60, -22)); p.LineTo(T(90, -22)); p.LineTo(T(90, -2)); p.LineTo(T(60, -2));
                p.ClosePath(); p.Fill();
                p.fillColor = color;
                p.BeginPath();
                p.MoveTo(T(0, -2)); p.LineTo(T(56, -2)); p.LineTo(T(56, 4)); p.LineTo(T(0, 4));
                p.ClosePath(); p.Fill();
                p.fillColor = metal;
                p.BeginPath();
                p.MoveTo(T(76, 14)); p.LineTo(T(90, 14)); p.LineTo(T(90, 46)); p.LineTo(T(76, 46));
                p.ClosePath(); p.Fill();
                p.fillColor = color;
                Vector2 muzzle = T(-4, 4); float r2 = SW(r, 6 * scale);
                p.BeginPath(); p.Arc(muzzle, r2, 0, 360); p.Fill();
            }
            else if (kind == "roller")
            {
                p.fillColor = metal;
                p.BeginPath();
                p.MoveTo(T(-10, -2)); p.LineTo(T(70, -2)); p.LineTo(T(70, 6)); p.LineTo(T(-10, 6));
                p.ClosePath(); p.Fill();
                p.fillColor = color;
                p.BeginPath();
                p.MoveTo(T(70, -22)); p.LineTo(T(120, -22)); p.LineTo(T(120, 26)); p.LineTo(T(70, 26));
                p.ClosePath(); p.Fill();
            }
            else if (kind == "charger")
            {
                p.fillColor = metal;
                p.BeginPath();
                p.MoveTo(T(-8, -4)); p.LineTo(T(152, -4)); p.LineTo(T(152, 10)); p.LineTo(T(-8, 10));
                p.ClosePath(); p.Fill();
                p.fillColor = metalLight;
                Vector2 c = T(100, -10);
                p.BeginPath(); p.Arc(c, SW(r, 14 * scale), 0, 360); p.Fill();
                p.fillColor = color;
                p.BeginPath(); p.Arc(c, SW(r, 6 * scale), 0, 360); p.Fill();
                p.fillColor = metal;
                p.BeginPath();
                p.MoveTo(T(60, 14)); p.LineTo(T(74, 14)); p.LineTo(T(74, 46)); p.LineTo(T(60, 46));
                p.ClosePath(); p.Fill();
            }
            else if (kind == "dualies")
            {
                p.fillColor = metal;
                p.BeginPath();
                p.MoveTo(T(-6, -4)); p.LineTo(T(54, -4)); p.LineTo(T(54, 10)); p.LineTo(T(-6, 10));
                p.ClosePath(); p.Fill();
                p.BeginPath();
                p.MoveTo(T(-6, 14)); p.LineTo(T(8, 14)); p.LineTo(T(8, 34)); p.LineTo(T(-6, 34));
                p.ClosePath(); p.Fill();
                p.BeginPath();
                p.MoveTo(T(40, 14)); p.LineTo(T(54, 14)); p.LineTo(T(54, 34)); p.LineTo(T(40, 34));
                p.ClosePath(); p.Fill();
                p.fillColor = color;
                p.BeginPath();
                p.MoveTo(T(0, 0)); p.LineTo(T(36, 0)); p.LineTo(T(36, 4)); p.LineTo(T(0, 4));
                p.ClosePath(); p.Fill();
            }
            else if (kind == "slosher")
            {
                p.fillColor = metal;
                p.BeginPath();
                p.MoveTo(T(-4, -20)); p.LineTo(T(62, -20));
                p.LineTo(T(58, 30)); p.LineTo(T(0, 30));
                p.ClosePath(); p.Fill();
            }
            else if (kind == "splatana")
            {
                p.fillColor = new Color(0.753f, 0.753f, 0.816f);
                p.BeginPath();
                p.MoveTo(T(0, -4)); p.LineTo(T(140, -4)); p.LineTo(T(140, 8)); p.LineTo(T(0, 8));
                p.ClosePath(); p.Fill();
                p.fillColor = color;
                p.BeginPath();
                p.MoveTo(T(-4, -8)); p.LineTo(T(16, -8)); p.LineTo(T(16, 12)); p.LineTo(T(-4, 12));
                p.ClosePath(); p.Fill();
            }
        }

        // ヘルパ: 楕円塗りつぶし
        void DrawEllipse(UnityEngine.UIElements.Painter2D p, Rect r, float cx, float cy, float rx, float ry)
        {
            Vector2 c = P(r, cx, cy);
            float ex = SW(r, rx);
            float ey = ry * r.height / 440f;
            // 楕円を多角形近似(36分割)
            p.BeginPath();
            for (int i = 0; i <= 36; i++)
            {
                float ang = i * Mathf.PI * 2 / 36f;
                Vector2 pt = new Vector2(c.x + Mathf.Cos(ang) * ex, c.y + Mathf.Sin(ang) * ey);
                if (i == 0) p.MoveTo(pt); else p.LineTo(pt);
            }
            p.ClosePath();
            p.Fill();
        }
        void DrawEllipseStroke(UnityEngine.UIElements.Painter2D p, Rect r, float cx, float cy, float rx, float ry, float sw, Color col)
        {
            Vector2 c = P(r, cx, cy);
            float ex = SW(r, rx);
            float ey = ry * r.height / 440f;
            p.strokeColor = col; p.lineWidth = SW(r, sw);
            p.BeginPath();
            for (int i = 0; i <= 36; i++)
            {
                float ang = i * Mathf.PI * 2 / 36f;
                Vector2 pt = new Vector2(c.x + Mathf.Cos(ang) * ex, c.y + Mathf.Sin(ang) * ey);
                if (i == 0) p.MoveTo(pt); else p.LineTo(pt);
            }
            p.ClosePath();
            p.Stroke();
        }
        void DrawStrokeBezier(UnityEngine.UIElements.Painter2D p, Rect r, float x0, float y0, float c1x, float c1y, float c2x, float c2y, float x1, float y1, float sw, Color col)
        {
            p.strokeColor = col; p.lineWidth = SW(r, sw); p.lineCap = LineCap.Round;
            p.BeginPath();
            p.MoveTo(P(r, x0, y0));
            p.BezierCurveTo(P(r, c1x, c1y), P(r, c2x, c2y), P(r, x1, y1));
            p.Stroke();
        }
        void DrawRect(UnityEngine.UIElements.Painter2D p, Rect r, float x, float y, float w, float h)
        {
            p.BeginPath();
            p.MoveTo(P(r, x, y)); p.LineTo(P(r, x + w, y));
            p.LineTo(P(r, x + w, y + h)); p.LineTo(P(r, x, y + h));
            p.ClosePath(); p.Fill();
        }
        void DrawRectStroke(UnityEngine.UIElements.Painter2D p, Rect r, float x, float y, float w, float h, float sw, Color col)
        {
            p.strokeColor = col; p.lineWidth = SW(r, sw);
            p.BeginPath();
            p.MoveTo(P(r, x, y)); p.LineTo(P(r, x + w, y));
            p.LineTo(P(r, x + w, y + h)); p.LineTo(P(r, x, y + h));
            p.ClosePath(); p.Stroke();
        }
        void DrawRectCentered(UnityEngine.UIElements.Painter2D p, Rect r, float cx, float cy, float w, float h)
        {
            DrawRect(p, r, cx - w / 2, cy - h / 2, w, h);
        }
        void DrawCircle(UnityEngine.UIElements.Painter2D p, Rect r, float cx, float cy, float radius)
        {
            Vector2 c = P(r, cx, cy);
            p.BeginPath();
            p.Arc(c, SW(r, radius), 0, 360);
            p.Fill();
        }
    }

    /// <summary>
    /// 画像スロット(プレースホルダ)。
    /// </summary>
    public class IwImgSlot : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<IwImgSlot, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _label = new() { name = "img-label", defaultValue = "画像" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var s = (IwImgSlot)ve;
                s.SetLabel(_label.GetValueFromBag(bag, cc));
            }
        }

        /// <summary>ラベル設定。</summary>
        public void SetLabel(string label)
        {
            Clear();
            style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
            style.borderTopColor = new Color(0.204f, 0.204f, 0.247f);
            style.borderBottomColor = new Color(0.204f, 0.204f, 0.247f);
            style.borderLeftColor = new Color(0.204f, 0.204f, 0.247f);
            style.borderRightColor = new Color(0.204f, 0.204f, 0.247f);
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            var lbl = new Label($"[ {label} ]");
            lbl.style.color = new Color(0.541f, 0.541f, 0.572f);
            lbl.style.fontSize = 10;
            lbl.style.letterSpacing = 2;
            Add(lbl);
        }

        public IwImgSlot()
        {
            SetLabel("画像");
        }
    }
}
