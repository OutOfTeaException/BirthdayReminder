using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using System.Linq;
using System.Text;
using BirthdayReminder.Extensions;
using Android.App.Job;
using Android.Util;

namespace BirthdayReminder
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        private const int REQUEST_READ_CONTACTS = 42;
        public const string TAG = "BirthdayReminder";

        private NotificationHelper notificationHelper;
        private BirthdayService birthdayService;
        private View layout;
        private FloatingActionButton fab;
        //TODO: Konfigurierbar machen
        private int daysInFuture = 30;

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
            notificationHelper = new NotificationHelper(this);
            birthdayService = new BirthdayService(this);

            SetContentView(Resource.Layout.activity_main);
            layout = FindViewById(Resource.Id.main_layout);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            Log.Info(TAG, "Anwendung gestartet.");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == REQUEST_READ_CONTACTS)
            {
                 // Received permission result for camera permission.
                //Log.Info(TAG, "Received response for Location permission request.");

                // Check if the only required permission has been granted
                if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
                {
                    // Location permission has been granted, okay to retrieve the location of the device.
                    //Log.Info(TAG, "Location permission has now been granted.");
                    Snackbar.Make(fab, "Cool", Snackbar.LengthShort).Show();
                }
                else
                {
                    //Log.Info(TAG, "Location permission was NOT granted.");
                    Snackbar.Make(fab, "Nicht cool", Snackbar.LengthShort).Show();
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
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;

            // Check if the Camera permission is already available.
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != (int)Permission.Granted)
            {
                RequestPermissions();
            }
            else
            {

                //CheckForNextBirthdays();
                var jobParameters = new PersistableBundle();
                jobParameters.PutInt("daysInFuture", 30);

                var jobInfo = this.CreateJobBuilderUsingJobId<BirthdayCheckJob>(1)
                                     .SetExtras(jobParameters)
                                     .SetPeriodic(15 * 60 * 1000, 5 * 60 * 1000) // alle 15 min
                                     .Build();

                var scheduleResult = JobScheduler.Schedule(jobInfo);
                if (JobScheduler.ResultSuccess == scheduleResult)
                {
                    Log.Info(TAG, "Job gestartet!");
                    Snackbar.Make(view, "Job gestartet!", Snackbar.LengthShort).Show();
                }
                else
                {
                    Log.Error(TAG, "Job konnte nicht gestartet werden...");
                    Snackbar.Make(view, "Job konnte nicht gestartet werden... :(", Snackbar.LengthShort).Show();
                }
            }
        }

        private void CheckForNextBirthdays()
        {
            var nextBirthdays = birthdayService.GetNextBirthdays(30);

            StringBuilder message = new StringBuilder();

            foreach (var birthday in nextBirthdays)
            {
                message.AppendLine($"{birthday.birthday.ToString("dd.MM.")} - {birthday.name}");
            }

            var notification = notificationHelper.GetNotification($"Bald haben {nextBirthdays.Count()} Leute Geburtstag", message.ToString());
            notificationHelper.Notify(0, notification);
        }

        private void RequestPermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadContacts))
            {
                // Provide an additional rationale to the user if the permission was not granted
                // and the user would benefit from additional context for the use of the permission.
                // For example if the user has previously denied the permission.
                //Log.Info(TAG, "Displaying camera permission rationale to provide additional context.");

                var requiredPermissions = new String[] { Manifest.Permission.ReadContacts };
                Snackbar.Make(layout, "BLABLABLA", Snackbar.LengthIndefinite)
                        .SetAction("OK", new Action<View>(delegate (View obj)
                        {
                            ActivityCompat.RequestPermissions(this, requiredPermissions, REQUEST_READ_CONTACTS);
                        }))
                        .Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadContacts }, REQUEST_READ_CONTACTS);
            }
        }
    }
 }

