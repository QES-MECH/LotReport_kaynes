﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LotReport.Views.ReusableControls
{
    /// <summary>
    /// Interaction logic for LabelTextBox.xaml
    /// </summary>
    public partial class LabelTextBox : UserControl
    {
        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(LabelTextBox), new PropertyMetadata("Label: "));

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(LabelTextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for Ratio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelWidthProperty =
            DependencyProperty.Register("LabelWidth", typeof(GridLength), typeof(LabelTextBox), new PropertyMetadata(new GridLength(0.7, GridUnitType.Star)));

        // Using a DependencyProperty as the backing store for ValueWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueWidthProperty =
            DependencyProperty.Register("ValueWidth", typeof(GridLength), typeof(LabelTextBox), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

        // Using a DependencyProperty as the backing store for LabelAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelAlignmentProperty =
            DependencyProperty.Register("LabelAlignment", typeof(HorizontalAlignment), typeof(LabelTextBox), new PropertyMetadata(HorizontalAlignment.Left));

        // Using a DependencyProperty as the backing store for TextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueAlignmentProperty =
            DependencyProperty.Register("ValueAlignment", typeof(TextAlignment), typeof(LabelTextBox), new PropertyMetadata(TextAlignment.Left));

        // Using a DependencyProperty as the backing store for ValueReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(LabelTextBox), new PropertyMetadata(false));

        public LabelTextBox()
        {
            this.InitializeComponent();
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public GridLength LabelWidth
        {
            get { return (GridLength)GetValue(LabelWidthProperty); }
            set { this.SetValue(LabelWidthProperty, value); }
        }

        public GridLength ValueWidth
        {
            get { return (GridLength)GetValue(ValueWidthProperty); }
            set { this.SetValue(ValueWidthProperty, value); }
        }

        public HorizontalAlignment LabelAlignment
        {
            get { return (HorizontalAlignment)GetValue(LabelAlignmentProperty); }
            set { this.SetValue(LabelAlignmentProperty, value); }
        }

        public TextAlignment ValueAlignment
        {
            get { return (TextAlignment)GetValue(ValueAlignmentProperty); }
            set { this.SetValue(ValueAlignmentProperty, value); }
        }

        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { this.SetValue(ReadOnlyProperty, value); }
        }
    }
}
