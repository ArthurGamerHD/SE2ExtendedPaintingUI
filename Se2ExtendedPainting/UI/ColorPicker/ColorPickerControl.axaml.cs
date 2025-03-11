// This code is from Space Engineers 2 and is a slightest modified copy of
// Keen.Game2.Client.UI.Library.Controls.LabelledControls.Colors
// Since the original is set to "internal" use only

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Keen.Game2.Client.UI.Library.Controls.LabelledControls;
using Keen.VRage.Library.Mathematics;
using Keen.VRage.UI.Shared.Extensions;

namespace Se2ExtendedPainting.UI.ColorPicker;

public class ColorPickerControl : LabelledControlBase
{
#pragma warning disable KN023 // Avoid keeping state in static member/property - the only exception is the Singleton pattern
    public static readonly StyledProperty<SolidColorBrush> ColorPreviewProperty =
        AvaloniaProperty.Register<ColorPickerControl, SolidColorBrush>(nameof(ColorPreview));

    public static readonly DirectProperty<ColorPickerControl, string> HexColorProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerControl, string>(nameof(HexColor), control => control.HexColor,
            (control, value) => control.HexColor = value);

    public static readonly DirectProperty<ColorPickerControl, ColorHSV> ColorProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerControl, ColorHSV>(nameof(Color), control => control.Color,
            (control, value) => control.Color = value);
#pragma warning restore KN023 // Avoid keeping state in static member/property - the only exception is the Singleton pattern

    private Border? _wheelColor;
    private Grid? _wheel;
    private Ellipse? _wheelSelector;


    // this should never be null
    private Slider _sliderH = null!;
    private Slider _sliderS = null!;
    private Slider _sliderV = null!;
    // if it is, well, bad for you

    public SolidColorBrush ColorPreview
    {
        get => GetValue(ColorPreviewProperty);
        set => SetValue(ColorPreviewProperty, value);
    }

    private ColorHSV _color;

    private bool _isManualInput = false;

    private int internalChange = 0;

    private bool hasTemplate = false;

    public string HexColor
    {
        get => $"#{Color.ToSRGB().ToHtml()}";
        set
        {
            if (value.Length < 6 || value.Length > 7)
                return;

            if (!value.StartsWith("#"))
                value = "#" + value;

            if (!Avalonia.Media.Color.TryParse(value, out var color))
                return;

            _isManualInput = true;
            Color = color.ToHSV();
            UpdateSliders();
            _isManualInput = false;
        }
    }

    public ColorHSV Color
    {
        get => _color;
        set
        {
            value.A = 1;

            var oldHex = HexColor;
            var oldColor = Color;

            _color = value;
            RaisePropertyChanged(ColorProperty, oldColor, _color);

            if (!_isManualInput)
                RaisePropertyChanged(HexColorProperty, oldHex, HexColor);

            UpdatePreview();

            if (internalChange == 0 && hasTemplate) UpdateSliders();
        }
    }

    private void UpdatePreview()
    {
        if (_wheelColor != null)
            _wheelColor.Opacity = Color.V;

        ColorPreview = new SolidColorBrush(_color.ToAvalonia());

        if (_wheel == null || _wheelSelector == null)
            return;

        var bounds = _wheel.Bounds.Size / 2;

        double angleRadians = MathHelper.ToRadians(Color.H * 360 + 90);

        var distance = Color.S * ((bounds.Width + bounds.Height) / 2);

        var x = distance * Math.Cos(angleRadians);
        var y = distance * Math.Sin(angleRadians);

        _wheelSelector.RenderTransform = new TranslateTransform(x, y);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_wheel != null)
        {
            _wheel.PointerPressed -= OnWheelOnPointerPressed;
            _wheel.PointerMoved -= OnWheelOnPointerMoved;
            _wheel.PropertyChanged -= OnWheelUpdateLayoutUpdated;
        }

        _wheel = e.NameScope.Find<Grid>("PART_Wheel");

        if (_wheel != null)
        {
            _wheel.PointerPressed += OnWheelOnPointerPressed;
            _wheel.PointerMoved += OnWheelOnPointerMoved;
            _wheel.PropertyChanged += OnWheelUpdateLayoutUpdated;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_sliderH != null)
            _sliderH.ValueChanged -= OnSliderHOnValueChanged;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_sliderS != null)
            _sliderS.ValueChanged -= OnSliderSOnValueChanged;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_sliderV != null)
            _sliderV.ValueChanged -= OnSliderVOnValueChanged;

        _sliderH = e.NameScope.Find<Slider>("PART_SliderH")!;
        _sliderS = e.NameScope.Find<Slider>("PART_SliderS")!;
        _sliderV = e.NameScope.Find<Slider>("PART_SliderV")!;

        _sliderH.ValueChanged += OnSliderHOnValueChanged;
        _sliderS.ValueChanged += OnSliderSOnValueChanged;
        _sliderV.ValueChanged += OnSliderVOnValueChanged;

        UpdateSliders();

        _wheelColor = e.NameScope.Find<Border>("PART_WheelColor");
        _wheelSelector = e.NameScope.Find<Ellipse>("PART_WheelSelector");

        UpdatePreview();

        hasTemplate = true;
    }

    private void OnWheelUpdateLayoutUpdated(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
            UpdatePreview();
    }

    private void OnSliderVOnValueChanged(object? _, RangeBaseValueChangedEventArgs args)
    {
        internalChange++;
        if (Math.Abs(Color.V - args.NewValue / 100) > 0.01)
            Color = new ColorHSV((float)(_sliderH.Value / 360d), (float)(_sliderS.Value / 100d),
                (float)(args.NewValue / 100d), 1);
        internalChange--;
    }

    private void OnSliderSOnValueChanged(object? _, RangeBaseValueChangedEventArgs args)
    {
        internalChange++;
        if (Math.Abs(Color.S - args.NewValue / 100) > 0.01)
            Color = new ColorHSV((float)_sliderH.Value / 360, (float)(args.NewValue / 100),
                (float)(_sliderV.Value / 100), 1);
        internalChange--;
    }

    private void OnSliderHOnValueChanged(object? _, RangeBaseValueChangedEventArgs args)
    {
        internalChange++;
        if (Math.Abs(Color.H - args.NewValue / 360) > 0.003)
            Color = new ColorHSV((float)args.NewValue / 360, (float)(_sliderS.Value / 100),
                (float)(_sliderV.Value / 100), 1);
        internalChange--;
    }

    private void UpdateSliders()
    {
        _sliderH.Value = (int)(_color.H * 360);
        _sliderS.Value = (int)(_color.S * 100);
        _sliderV.Value = (int)(_color.V * 100);
    }

    private void OnWheelOnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        SetColorFromWheel(args);
    }

    private void OnWheelOnPointerMoved(object? sender, PointerEventArgs args)
    {
        if (_wheel == null)
            return;

        if (args.GetCurrentPoint(_wheel).Properties.IsLeftButtonPressed)
            SetColorFromWheel(args);
    }

    private void SetColorFromWheel(PointerEventArgs args)
    {
        if (_wheel == null || _wheelSelector == null)
            return;

        var pos = args.GetPosition(_wheel);

        var bounds = _wheel.Bounds.Size / 2;

        var relativePosition = pos - new Point(bounds.Width, bounds.Height);

        var distance = Math.Sqrt(relativePosition.X * relativePosition.X + relativePosition.Y * relativePosition.Y);

        var angleRadians = Math.Atan2(relativePosition.Y, relativePosition.X) - Math.PI / 2;
        var angleDegrees = angleRadians * (180.0 / Math.PI);

        switch (angleDegrees)
        {
            case < 0:
                angleDegrees += 360;
                break;

            case >= 360:
                angleDegrees -= 360;
                break;

            default:
                break;
        }

        if (_sliderS != null)
            _sliderS.Value = (int)(Math.Clamp((float)(distance / ((bounds.Height + bounds.Width) / 2)), 0, 1) * 100);

        if (_sliderH != null) _sliderH.Value = (int)angleDegrees;
    }
}