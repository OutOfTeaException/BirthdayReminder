using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Util;

namespace BirthdayReminder
{
    [Service(Name = "BirthdayReminder.BirthdayCheckJob", Permission = "android.permission.BIND_JOB_SERVICE")]
    public class BirthdayCheckJob : JobService
    {
        private const string TAG = "BirthdayCheckJob";
        private NotificationHelper notificationHelper;
        private BirthdayService birthdayService;
        //TODO: Konfigurabel machen
        private (int hour, int minute) checkTime = (23, 5);

        public override bool OnStartJob(JobParameters jobParams)
        {
            Log.Info(TAG, "Starte BirthdayCheck Job...");

            if (notificationHelper == null)
            {
                notificationHelper = new NotificationHelper(this);
                birthdayService = new BirthdayService(this);
            }

            if (ShouldCheck(DateTime.Now))
            {

                Task.Run(() =>
                {
                    int checkDaysInFuture = jobParams.Extras.GetInt("daysInFuture", 30);
                    Log.Debug(TAG, "Job wird ausgeführt...");
                    CheckForNextBirthdays(checkDaysInFuture);

                    Log.Debug(TAG, "Job erfolgreich ausgeführt.");
                    JobFinished(jobParams, false);
                });

                Log.Debug(TAG, "Job gestartet.");
            }
            return true;
        }

        private bool ShouldCheck(DateTime now)
        {
            var checkDateTime = new DateTime(now.Year, now.Month, now.Day, checkTime.hour, checkTime.minute, 0);

            return checkDateTime <= now && checkDateTime >= now.AddMinutes(-10);
        }

        private void CheckForNextBirthdays(int daysInFuture)
        {
           
            var nextBirthdays = birthdayService.GetNextBirthdays(daysInFuture);

            StringBuilder message = new StringBuilder();

            foreach (var birthday in nextBirthdays)
            {
                message.AppendLine($"{birthday.birthday.ToString("dd.MM.")} - {birthday.name}");
            }

            var notification = notificationHelper.GetNotification($"Bald haben {nextBirthdays.Count()} Leute Geburtstag", message.ToString());
            notificationHelper.Notify(0, notification);
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            Log.Debug(TAG, "Job wurde gestoppt.");
            return false; // Don't Reschedule
        }
    }
}