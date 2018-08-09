using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BirthdayReminder.Views.Settings
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {
        public const string PARAM_CHECK_TIME_HOUR = "checkTimeHour";
        public const string PARAM_CHECK_TIME_MINUTE = "checkTimeMinute";

        private TextView timeDisplay;
        private Button timeSelectButton;
        private (int hour, int minute) checkTime;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SettingsView);

            timeDisplay = FindViewById<TextView>(Resource.Id.selected_time);
            timeSelectButton = FindViewById<Button>(Resource.Id.select_time_button);
            timeSelectButton.Click += TimeSelectButton_Click;

            checkTime = (
                Intent.Extras.GetInt(PARAM_CHECK_TIME_HOUR),
                Intent.Extras.GetInt(PARAM_CHECK_TIME_MINUTE)
                );

            ShowCheckTime();
        }

        private void TimeSelectButton_Click(object sender, EventArgs e)
        {
            ShowTimePicker();
        }

        private void ShowTimePicker()
        {
            TimePickerFragment frag = TimePickerFragment.NewInstance(
               delegate (DateTime time)
               {
                   checkTime = (time.Hour, time.Minute);
                   ShowCheckTime();

                   Intent intent = new Intent();
                   intent.PutExtra(PARAM_CHECK_TIME_HOUR, checkTime.hour);
                   intent.PutExtra(PARAM_CHECK_TIME_MINUTE, checkTime.minute);
                   SetResult(Result.Ok, intent);
                   //Finish();
               }, checkTime);

            
            frag.Show(FragmentManager, "SettingsTimePicker");
        }

        private void ShowCheckTime()
        {
            timeDisplay.Text = $"Erinnerungszeit: {checkTime.hour:00}:{checkTime.minute:00}";
        }
    }
}