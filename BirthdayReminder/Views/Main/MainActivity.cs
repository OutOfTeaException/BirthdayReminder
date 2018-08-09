using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using BirthdayReminder.Extensions;
using Android.App.Job;
using Android.Support.V7.Widget;
using BirthdayReminder.Services;
using BirthdayReminder.Jobs;
using BirthdayReminder.Util;
using BirthdayReminder.Model;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;

namespace BirthdayReminder.Views.Main
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        private const int REQUEST_READ_CONTACTS = 42;
        private const int ACTIVITY_SETTINGS = 1;
        public const string TAG = "BirthdayReminder";

        private NotificationService notificationHelper;
        private BirthdayService birthdayService;
        private ConfigurationService configurationService;
        private View layout;
        private FloatingActionButton fab;

        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        BirthdayListAdapter birthdayListAdapter;
        private BirthdayList birthdayList = new BirthdayList();

        //TODO: Konfigurierbar machen
        private int daysInFuture = 30;
        private (int hour, int minute) checkTime = (19, 00);

        JobScheduler jobScheduler;
        JobScheduler JobScheduler
        {
            get
            {
                if (jobScheduler == null)
                {
                    jobScheduler = (JobScheduler)GetSystemService(JobSchedulerService);
                }
                return jobScheduler;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Services und Helper erstellen
            notificationHelper = new NotificationService(this);
            birthdayService = new BirthdayService(this);
            configurationService = new ConfigurationService();

            // View gedöns
            SetContentView(Resource.Layout.activity_main);
            layout = FindViewById(Resource.Id.main_layout);

            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);


            // Nächste Geburtstage ermitteln und anzeigen
            var nextBirthdays = GetNextBirthdays();
            birthdayList = new BirthdayList(nextBirthdays);
            birthdayListAdapter = new BirthdayListAdapter(birthdayList);
            mRecyclerView.SetAdapter(birthdayListAdapter);

            var checkTimeFromConfig = configurationService.GetCheckTime();
            if (checkTimeFromConfig == null)
            {
                checkTime = (07, 00);
            }
            else
            {
                checkTime = checkTimeFromConfig.Value;
            }

            // Job starten
            int scheduleResult = ScheduleJob();
            if (scheduleResult  != JobScheduler.ResultSuccess)
            {
                Log.Error("Job konnte nicht aktiviert werden...");
                Snackbar.Make(layout, "Job konnte nicht aktiviert werden... :(", Snackbar.LengthShort).Show();
            }

            Log.Info("Anwendung gestartet.");

            //TextView textView = FindViewById<TextView>(Resource.Id.textbox);
            //textView.Text = File.ReadAllText(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "log.txt"));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == REQUEST_READ_CONTACTS)
            {
                // Check if the only required permission has been granted
                if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
                {
                    // Berechtigung erteilt, nächste Geburtstage laden und anzeigen
                    var nextBirthdays = GetNextBirthdays();
                    birthdayList.Clear();
                    birthdayList.AddRange(nextBirthdays);
                    birthdayListAdapter.NotifyDataSetChanged();
                }
                else
                {
                    Snackbar.Make(fab, "Ohne die Berechtigung können keine Geburtstage ermittelt werden.", Snackbar.LengthShort).Show();
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                //TextView textView = FindViewById<TextView>(Resource.Id.textbox);
                //textView.Text = File.ReadAllText(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "log.txt"));
                var intent = new Intent(this, typeof(Settings.SettingsActivity));
                intent.PutExtra(Settings.SettingsActivity.PARAM_CHECK_TIME_HOUR, checkTime.hour);
                intent.PutExtra(Settings.SettingsActivity.PARAM_CHECK_TIME_MINUTE, checkTime.minute);
                StartActivityForResult(intent, ACTIVITY_SETTINGS);

                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == ACTIVITY_SETTINGS && resultCode == Result.Ok)
            {
                checkTime = (
                    data.Extras.GetInt(Settings.SettingsActivity.PARAM_CHECK_TIME_HOUR),
                    data.Extras.GetInt(Settings.SettingsActivity.PARAM_CHECK_TIME_MINUTE)
                    );

                configurationService.SaveCheckTime(checkTime);
                configurationService.RemoveLastCheckTime();
                ScheduleJob();
            }
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            //var notification = notificationHelper.GetNotification($"Test 123", "Hallo Welt. :)");
            //notificationHelper.Notify(0, notification);
        }

        private IList<Birthday> GetNextBirthdays()
        {
            // Check if the ReadContacts permission is already available.
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadContacts }, REQUEST_READ_CONTACTS);
                return new List<Birthday>();
            }
            else
            {
                //TODO: Konfigurierbar machen
                return birthdayService.GetNextBirthdays(daysInFuture);
            }
        }

        private int ScheduleJob()
        {
            JobScheduler.CancelAll();
            var jobParameters = new PersistableBundle();
            jobParameters.PutInt(BirthdayCheckJob.JOBPARAM_DAYS_IN_FUTURE, daysInFuture);
            jobParameters.PutIntArray(BirthdayCheckJob.JOBPARAM_CHECKTIME, new[] { checkTime.hour, checkTime.minute });

            var jobInfo = this.CreateJobBuilderUsingJobId<BirthdayCheckJob>(1)
                                 .SetExtras(jobParameters)
                                 .SetPeriodic(15 * 60 * 1000, 5 * 60 * 1000) // alle 15 min, 5 min Flex
                                 //.SetPeriodic(1 * 60 * 60 * 1000, 5 * 60 * 1000) // jede Stunde, 5 min Flex
                                 .SetPersisted(true)
                                 .Build();

            var scheduleResult = JobScheduler.Schedule(jobInfo);
            return scheduleResult;
        }
    }
 }

