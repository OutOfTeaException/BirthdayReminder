using System;
using Android.App;
using Android.OS;
using Android.Text.Format;
using Android.Widget;

namespace BirthdayReminder.Views.Settings
{
    public class TimePickerFragment : DialogFragment, TimePickerDialog.IOnTimeSetListener
    {
        private Action<DateTime> timeSelectedHandler = delegate { };
        private (int hour, int minute) time;

        public static TimePickerFragment NewInstance(Action<DateTime> onTimeSelected, (int hour, int minute) time)
        {
            TimePickerFragment fragment = new TimePickerFragment();
            fragment.timeSelectedHandler = onTimeSelected;
            fragment.time = time;

            return fragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            bool is24HourFormat = DateFormat.Is24HourFormat(Activity);
            TimePickerDialog dialog = new TimePickerDialog(Activity, this, time.hour, time.minute, is24HourFormat);

            return dialog;
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            DateTime currentTime = DateTime.Now;
            DateTime selectedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hourOfDay, minute, 0);

            timeSelectedHandler(selectedTime);
        }
    }
}