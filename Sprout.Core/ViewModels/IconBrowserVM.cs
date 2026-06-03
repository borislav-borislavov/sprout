using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.FontAwesome;
using System.Collections.ObjectModel;
using System.Windows;

namespace Sprout.Core.ViewModels
{
    public partial class IconBrowserVM : ObservableObject
    {
        private const int Columns = 6;

        private List<IconItemVM> _allIcons = [];
        private CancellationTokenSource _filterCts = new();

        [ObservableProperty]
        private ObservableCollection<List<IconItemVM>> _iconRows = [];

        [ObservableProperty]
        private IconItemVM? _selectedIcon;

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _xamlSnippet = string.Empty;

        [RelayCommand]
        private void SelectIcon(IconItemVM? icon)
        {
            if (icon is null) return;
            if (SelectedIcon is not null) SelectedIcon.IsSelected = false;
            icon.IsSelected = true;
            SelectedIcon = icon;
            XamlSnippet = icon.XamlSnippet;
        }

        [RelayCommand]
        private void CopySnippet()
        {
            if (!string.IsNullOrEmpty(XamlSnippet))
                Clipboard.SetText(XamlSnippet);
        }

        [ObservableProperty]
        private int _totalCount;

        public List<string> Categories { get; } = ["All", "Solid", "Regular", "Brands"];

        private string _selectedCategory = "All";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                _ = ApplyFilterAsync(_searchText);
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                _ = ApplyFilterAsync(value);
            }
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsLoading = true;

            var icons = await Task.Run(() =>
                FontAwesomeIcons.All
                    .OrderBy(i => i.Name)
                    .Select(i => new IconItemVM(i))
                    .ToList());

            _allIcons = icons;
            ApplyRows(icons);
            IsLoading = false;
        }

        private async Task ApplyFilterAsync(string query)
        {
            _filterCts.Cancel();
            _filterCts = new CancellationTokenSource();
            var token = _filterCts.Token;

            try
            {
                await Task.Delay(200, token);

                var trimmed = query.Trim();
                var category = _selectedCategory;

                var result = await Task.Run(() =>
                {
                    var filtered = _allIcons.AsEnumerable();

                    if (category != "All" && Enum.TryParse<FontAwesomeStyle>(category, out var style))
                        filtered = filtered.Where(i => i.Style == style);

                    if (!string.IsNullOrEmpty(trimmed))
                        filtered = filtered.Where(i => i.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase));

                    return filtered.ToList();
                }, token);

                ApplyRows(result);
            }
            catch (OperationCanceledException)
            {
                // A newer keystroke cancelled this one — expected, not a crash.
            }
        }

        private void ApplyRows(List<IconItemVM> icons)
        {
            TotalCount = icons.Count;
            var rows = new ObservableCollection<List<IconItemVM>>();
            for (int i = 0; i < icons.Count; i += Columns)
                rows.Add(icons.GetRange(i, Math.Min(Columns, icons.Count - i)));
            IconRows = rows;
        }
    }

    public class IconItemVM
    {
        public string Name { get; }
        public string Glyph { get; }
        public string UnicodeHex { get; }
        public FontAwesomeStyle Style { get; }
        public string DisplayLabel { get; }
        public string XamlSnippet { get; }
        public bool IsSelected { get; set; }

        public IconItemVM(FontAwesomeIcon icon)
        {
            Name = icon.Name;
            Glyph = icon.Glyph;
            UnicodeHex = icon.Unicode.ToString("x");
            Style = icon.Style;
            DisplayLabel = $"{icon.Name} ({icon.Style})";

            var fontKey = icon.Style switch
            {
                FontAwesomeStyle.Regular => "FaRegular",
                FontAwesomeStyle.Brands  => "FaBrands",
                _                        => "FaSolid"
            };
            var weight = icon.Style == FontAwesomeStyle.Solid ? " FontWeight=\"Black\"" : string.Empty;
            XamlSnippet = $"Text=\"&#x{UnicodeHex};\" FontFamily=\"{{StaticResource {fontKey}}}\"{weight}";
        }
    }
}
