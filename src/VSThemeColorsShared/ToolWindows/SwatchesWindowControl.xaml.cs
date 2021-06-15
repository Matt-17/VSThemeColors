﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;

namespace VSThemeColors
{
    /// <summary>
    /// Interaction logic for SwatchesWindowControl.
    /// </summary>
    public partial class SwatchesWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwatchesWindowControl"/> class.
        /// </summary>
        public SwatchesWindowControl()
        {
            InitializeComponent();
            Type environmentColors = typeof(EnvironmentColors);
            var tileSize = CalculateTileSize();

            foreach (PropertyInfo property in environmentColors.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (property.Name.EndsWith("BrushKey"))
                {
                    var tile = new Grid
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Tag = property.Name,
                        ToolTip = property.Name
                    };

                    tile.SetResourceReference(BackgroundProperty, property.GetValue(null));
                    tile.MouseDown += TileOnMouseDown;
                    Root.Children.Add(tile);
                }
            }

            SizeChanged += OnSizeChanged;
        }

        private double CalculateTileSize()
        {
            var width = Root.ActualWidth % 100;

            //If there's less than half a block free, expand the tiles to fit
            if (width < 50)
            {
                width = Root.ActualWidth / Math.Floor(Root.ActualWidth / 100);
            }
            //There's more than half a block free, shrink the tiles to pull another onto the row
            else
            {
                width = Root.ActualWidth / Math.Ceiling(Root.ActualWidth / 100);
            }

            return width;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var tileSize = CalculateTileSize();

            foreach (Grid child in Root.Children.OfType<Grid>())
            {
                child.Width = tileSize;
                child.Height = tileSize;
            }
        }

        private void TileOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var tile = (Grid)sender;
            var parent = (Panel)tile.Parent;
            parent.Children.Remove(tile);
            parent.Children.Insert(0, tile);

            var propertyName = (string)tile.ToolTip;
            BrushName.Text = $"{typeof(EnvironmentColors).Name}.{propertyName}";
            XamlNamespace.Text = $"xmlns:platformUI=\"clr-namespace:{typeof(EnvironmentColors).Namespace};assembly={typeof(EnvironmentColors).Assembly.GetName().Name}\"";
            XamlUsage.Text = $"{{DynamicResource {{x:Static platformUI:EnvironmentColors.{propertyName}}}}}";
            CSharpUsage.Text = $"{{TARGET_OBJECT}}.SetResourceReference({{DEPENDENCY_PROPERTY}}, {typeof(EnvironmentColors).FullName}.{propertyName});";
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (Grid child in Root.Children.OfType<Grid>())
            {
                var show = ((string)child.Tag).IndexOf(((TextBox)sender).Text, StringComparison.OrdinalIgnoreCase) > -1;
                child.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            var parent = (ContextMenu)item.Parent;

            if (parent.PlacementTarget is TextBox target && !string.IsNullOrEmpty(target.Text))
            {
                Clipboard.SetText(target.Text);
            }
        }

        private void CsharpCopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CSharpUsage.Text);
        }

        private void XamlUsageCopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(XamlUsage.Text);
        }

        private void XamlNamespaceCopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(XamlNamespace.Text);
        }
    }
}