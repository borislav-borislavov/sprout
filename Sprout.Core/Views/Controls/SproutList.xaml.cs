using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.SproutControlVMs;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
#nullable disable

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutList.xaml
    /// </summary>
    public partial class SproutList : UserControl, ISproutControl<SproutListConfig, SproutListUIState>
    {
        public SproutListConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.List;
        public SproutListUIState VM { get; internal set; }

        public SproutList()
        {
            InitializeComponent();
            RefreshItems();
        }

        // ── Header ───────────────────────────────────────────────────────────────
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(SproutList),
                new PropertyMetadata(string.Empty));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        // ── ShowSearch ─────────────────────────────────────────────────────────
        public static readonly DependencyProperty ShowSearchProperty =
            DependencyProperty.Register(nameof(ShowSearch), typeof(bool), typeof(SproutList),
                new PropertyMetadata(true));

        public bool ShowSearch
        {
            get => (bool)GetValue(ShowSearchProperty);
            set => SetValue(ShowSearchProperty, value);
        }

        // ── ShowFooter ─────────────────────────────────────────────────────────
        public static readonly DependencyProperty ShowFooterProperty =
            DependencyProperty.Register(nameof(ShowFooter), typeof(bool), typeof(SproutList),
                new PropertyMetadata(true));

        public bool ShowFooter
        {
            get => (bool)GetValue(ShowFooterProperty);
            set => SetValue(ShowFooterProperty, value);
        }

        // ── EmptyText ──────────────────────────────────────────────────────────
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(SproutList),
                new PropertyMetadata("No items to display"));

        public string EmptyText
        {
            get => (string)GetValue(EmptyTextProperty);
            set => SetValue(EmptyTextProperty, value);
        }

        // ── SearchText ─────────────────────────────────────────────────────────
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SproutList),
                new PropertyMetadata(string.Empty, OnItemsAffectingPropertyChanged));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        // ── SourceData (bound to the data provider) ──────────────────────────────
        public static readonly DependencyProperty SourceDataProperty =
            DependencyProperty.Register(nameof(SourceData), typeof(object), typeof(SproutList),
                new PropertyMetadata(null, OnItemsAffectingPropertyChanged));

        public object SourceData
        {
            get => GetValue(SourceDataProperty);
            set => SetValue(SourceDataProperty, value);
        }

        // ── IsEmpty (read-only state for the empty placeholder) ──────────────────
        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.Register(nameof(IsEmpty), typeof(bool), typeof(SproutList),
                new PropertyMetadata(true));

        public bool IsEmpty
        {
            get => (bool)GetValue(IsEmptyProperty);
            set => SetValue(IsEmptyProperty, value);
        }

        // ── FooterText (item count summary) ──────────────────────────────────────
        public static readonly DependencyProperty FooterTextProperty =
            DependencyProperty.Register(nameof(FooterText), typeof(string), typeof(SproutList),
                new PropertyMetadata(string.Empty));

        public string FooterText
        {
            get => (string)GetValue(FooterTextProperty);
            set => SetValue(FooterTextProperty, value);
        }

        private static void OnItemsAffectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SproutList list)
                list.RefreshItems();
        }

        /// <summary>
        /// Rebuilds the displayed items from <see cref="SourceData"/> applying the current
        /// <see cref="SearchText"/> filter, then updates the footer count and empty state.
        /// </summary>
        private void RefreshItems()
        {
            if (itemsControl == null)
            {
                return;
            }

            var allItems = GetSourceItems().ToList();
            var total = allItems.Count;

            var search = SearchText?.Trim();
            var searchActive = !string.IsNullOrEmpty(search);

            var visibleItems = searchActive
                ? allItems.Where(item => MatchesSearch(item, search)).ToList()
                : allItems;

            itemsControl.ItemsSource = visibleItems;

            var shown = visibleItems.Count;
            IsEmpty = shown == 0;

            if (searchActive && shown != total)
            {
                FooterText = $"{shown} of {total} items";
            }
            else
            {
                FooterText = total == 1 ? "1 item" : $"{total} items";
            }
        }

        private IEnumerable<object> GetSourceItems()
        {
            switch (SourceData)
            {
                case DataView dataView:
                    return dataView.Cast<object>();
                case DataTable dataTable:
                    return dataTable.DefaultView.Cast<object>();
                case string:
                    return [];
                case IEnumerable enumerable:
                    return enumerable.Cast<object>();
                default:
                    return [];
            }
        }

        private static bool MatchesSearch(object item, string search)
        {
            if (item is DataRowView rowView)
            {
                foreach (DataColumn column in rowView.Row.Table.Columns)
                {
                    if (column.ColumnName.StartsWith("_"))
                    {
                        continue;
                    }

                    var value = rowView[column.ColumnName];
                    if (value != null && value != System.DBNull.Value &&
                        value.ToString().Contains(search, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

            return item?.ToString()?.Contains(search, System.StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}
