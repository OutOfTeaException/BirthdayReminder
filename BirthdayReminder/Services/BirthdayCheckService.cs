using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Runtime;
using NodaTime;
using NodaTime.Extensions;

namespace BirthdayReminder.Services
{
    public class BirthdayCheckService : ContextWrapper
    {
        private NotificationService notificationService;
        private BirthdayService birthdayService;
        private ConfigurationService ConfigurationService;

        public BirthdayCheckService(Context context) : base(context)
        {
            notificationService = new NotificationService(context);
            birthdayService = new BirthdayService(context);
            ConfigurationService = new ConfigurationService();
        }

        public void Schedule()
        {
            var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();

            Intent alarmIntent = new Intent(this, typeof(AlarmReceiver));
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.CancelCurrent);

            var checkTime = ConfigurationService.GetCheckTime().Value;
            var startTime = SystemClock.Instance.InTzdbSystemDefaultZone().GetCurrentDate() + new LocalTime(checkTime.hours, checkTime.minutes, 0);

            long startTimeInUnixUtcMillis = startTime.InZoneLeniently(DateTimeZoneProviders.Tzdb.GetSystemDefault()).ToInstant().ToUnixTimeMilliseconds();
            alarmManager.SetInexactRepeating(AlarmType.Rtc, startTimeInUnixUtcMillis, (long)Duration.FromDays(1).TotalMilliseconds, pendingIntent);
        }

        public void CheckBirthdays()
        {
            CheckForNextBirthdays(30);
            ConfigurationService.SaveLastCheckTime(DateTime.Now);
        }

        private void CheckForNextBirthdays(int daysInFuture)
        {
            ConfigurationService.SaveLastCheckTime(DateTime.Now);
            var nextBirthdays = birthdayService.GetNextBirthdays(daysInFuture);

            if (!nextBirthdays.Any())
            {
                return;
            }

            StringBuilder message = new StringBuilder();

            foreach (var birthday in nextBirthdays)
            {
                string date = birthday.ToString();
                if (birthday.IsToday())
                {
                    date = "Heute";
                }
                else if (birthday.IsTomorrow())
                {
                    date = "Morgen";
                }

                message.AppendLine($"{date} - {birthday.Name}");
            }

            var notification = notificationService.GetNotification($"Bald haben {nextBirthdays.Count()} Leute Geburtstag", message.ToString());
            notificationService.Notify(0, notification);
        }
    }
}