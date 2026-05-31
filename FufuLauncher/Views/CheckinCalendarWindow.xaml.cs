using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MihoyoBBS;

namespace FufuLauncher.Views
{
    public sealed partial class CheckinCalendarWindow : Window
    {
        public ObservableCollection<CalendarRewardItem> Rewards { get; } = new();

        public CheckinCalendarWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            SystemBackdrop = new MicaBackdrop();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(680, 720));

            _ = LoadCalendarDataAsync();
        }

        private async Task LoadCalendarDataAsync()
        {
            var path = Helpers.AppPaths.ConfigFile;
            if (!File.Exists(path)) return;

            try
            {
                var json = await File.ReadAllTextAsync(path);
                var config = JsonSerializer.Deserialize<Config>(json) ?? new Config();

                var genshin = new Genshin();
                await genshin.InitializeAsync(config);

                var calendarData = await genshin.GetCheckinCalendarAsync();
                if (calendarData != null && calendarData.Awards != null)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        TitleText.Text = $"{calendarData.Month}月 签到奖励日历";
                        Rewards.Clear();
                        foreach (var item in calendarData.Awards)
                        {
                            Rewards.Add(item);
                        }
                        CalendarGridView.ItemsSource = Rewards;
                    });
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}