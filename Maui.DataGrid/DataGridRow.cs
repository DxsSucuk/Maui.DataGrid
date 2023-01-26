﻿using Maui.DataGrid.Utils;
using Microsoft.Maui.Controls;

namespace Maui.DataGrid;

internal sealed class DataGridRow : Grid
{
    #region Fields

    private Color _bgColor;
    private Color _textColor;
    private bool _hasSelected;

    #endregion

    #region properties

    public DataGrid DataGrid
    {
        get => (DataGrid)GetValue(DataGridProperty);
        set => SetValue(DataGridProperty, value);
    }

    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public object RowContext
    {
        get => GetValue(RowContextProperty);
        set => SetValue(RowContextProperty, value);
    }

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty DataGridProperty =
        BindableProperty.Create(nameof(DataGrid), typeof(DataGrid), typeof(DataGridRow), null,
            propertyChanged: (b, _, _) => ((DataGridRow)b).CreateView());

    public static readonly BindableProperty IndexProperty =
        BindableProperty.Create(nameof(Index), typeof(int), typeof(DataGridRow), 0,
            propertyChanged: (b, _, _) => ((DataGridRow)b).UpdateBackgroundColor());

    public static readonly BindableProperty RowContextProperty =
        BindableProperty.Create(nameof(RowContext), typeof(object), typeof(DataGridRow),
            propertyChanged: (b, _, _) => ((DataGridRow)b).UpdateBackgroundColor());

    #endregion

    #region Methods

    private void CreateView()
    {
        UpdateBackgroundColor();
        BackgroundColor = DataGrid.BorderColor;
        ColumnSpacing = DataGrid.BorderThickness.HorizontalThickness / 2;
        Padding = new Thickness(DataGrid.BorderThickness.HorizontalThickness / 2,
            DataGrid.BorderThickness.VerticalThickness / 2);

        foreach (var col in DataGrid.Columns)
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = col.Width });
            View cell;

            if (col.CellTemplate != null)
            {
                cell = new ContentView { Content = col.CellTemplate.CreateContent() as View };
                if (col.PropertyName != null)
                {
                    cell.SetBinding(BindingContextProperty,
                        new Binding(col.PropertyName, source: RowContext));
                }
            }
            else
            {
                cell = new Label
                {
                    Padding = 0,
                    TextColor = _textColor,
                    BackgroundColor = _bgColor,
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalTextAlignment = col.VerticalContentAlignment.ToTextAlignment(),
                    HorizontalTextAlignment = col.HorizontalContentAlignment.ToTextAlignment(),
                    LineBreakMode = LineBreakMode.WordWrap
                };
                cell.SetBinding(Label.TextProperty,
                    new Binding(col.PropertyName, BindingMode.Default, stringFormat: col.StringFormat));
                cell.SetBinding(Label.FontSizeProperty,
                    new Binding(DataGrid.FontSizeProperty.PropertyName, BindingMode.Default, source: DataGrid));
                cell.SetBinding(Label.FontFamilyProperty,
                    new Binding(DataGrid.FontFamilyProperty.PropertyName, BindingMode.Default, source: DataGrid));
            }

            Children.Add(cell);
            SetColumn((BindableObject)cell, DataGrid.Columns.IndexOf(col));
        }
    }

    private void UpdateBackgroundColor()
    {
        _hasSelected = DataGrid?.SelectedItem == RowContext;
        var actualIndex = DataGrid?.InternalItems?.IndexOf(BindingContext) ?? -1;
        if (actualIndex > -1)
        {
            _bgColor =
                DataGrid.SelectionEnabled && DataGrid.SelectedItem != null && DataGrid.SelectedItem == RowContext
                    ? DataGrid.ActiveRowColor
                    : DataGrid.RowsBackgroundColorPalette.GetColor(actualIndex, BindingContext);
            _textColor = DataGrid.RowsTextColorPalette.GetColor(actualIndex, BindingContext);

            ChangeColor(_bgColor);
        }
    }

    private void ChangeColor(Color color)
    {
        foreach (var v in Children)
        {
            if (v is View view)
            {
                view.BackgroundColor = color;

                if (view is Label label)
                {
                    label.TextColor = _textColor;
                }
            }
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateBackgroundColor();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (Parent != null)
        {
            DataGrid.ItemSelected += DataGrid_ItemSelected;
        }
        else
        {
            DataGrid.ItemSelected -= DataGrid_ItemSelected;
        }
    }

    private void DataGrid_ItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (DataGrid.SelectionEnabled && (e.CurrentSelection[^1] == RowContext || _hasSelected))
        {
            UpdateBackgroundColor();
        }
    }

    #endregion
}