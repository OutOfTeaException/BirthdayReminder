using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using System.Collections.Generic;
using Android.Provider;
using Android.Database;
using Android;
using Android.Support.V4.Content;
using Android.Content.PM;
using Android.Support.V4.App;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Content;

namespace BirthdayReminder
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        private const int REQUEST_READ_CONTACTS = 42;

        private NotificationHelper notificationHelper;
        private View layout;
        private FloatingActionButton fab;
        private int daysInFuture = 30;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            notificationHelper = new NotificationHelper(this);

            SetContentView(Resource.Layout.activity_main);
            layout = FindViewById(Resource.Id.main_layout);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
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
                var birthdays = GetBirthdaysFromContacts();

                var birthdaysToday = from b in birthdays.Distinct()
                                     let bNow = new DateTime(DateTime.Today.Year, b.birthday.Month, b.birthday.Day)
                                     where bNow >= DateTime.Today && bNow <= DateTime.Today.AddDays(daysInFuture)
                                     orderby bNow
                                     select b;

                StringBuilder message = new StringBuilder();

                foreach (var b in birthdaysToday)
                {
                    message.AppendLine($"{b.birthday.ToString("dd.MM.")} - {b.name}");
                }

                var notification = notificationHelper.GetNotification($"Bald haben {birthdaysToday.Count()} Leute Geburtstag", message.ToString());
                notificationHelper.Notify(0, notification);
            }
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
                Snackbar.Make(layout,
                               "BLABLABLA",
                               Snackbar.LengthIndefinite)
                        .SetAction("OK",
                                   new Action<View>(delegate (View obj) {
                                       ActivityCompat.RequestPermissions(this, requiredPermissions, REQUEST_READ_CONTACTS);
                                   }
                        )
                ).Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadContacts }, REQUEST_READ_CONTACTS);
            }
        }

        private IList<(String name, DateTime birthday)> GetBirthdaysFromContacts()
        {
            var birthdays = new List<(String, DateTime)>();

            var uri = ContactsContract.Data.ContentUri;
            string[] projection = {
               ContactsContract.Contacts.InterfaceConsts.Id,
               ContactsContract.Contacts.InterfaceConsts.DisplayName,
               ContactsContract.CommonDataKinds.Event.StartDate
            };

            string query = "data2=3"; // Type=Birthday

            using (var phones = ApplicationContext.ContentResolver.Query(uri, projection, query, null, null))
            {
                if (phones != null)
                {
                    while (phones.MoveToNext())
                    {
                        try
                        {
                            string name = phones.GetString(phones.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                            string birthdayValue = phones.GetString(phones.GetColumnIndex(ContactsContract.CommonDataKinds.Event.StartDate));

                            if (DateTime.TryParseExact(birthdayValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime birthday))
                            {
                                birthdays.Add((name, birthday));
                                //yield return (name, birthday);
                            }

                            //phoneContacts.Add(name);
                        }
                        catch (Exception ex)
                        {
                            //something wrong with one contact, may be display name is completely empty, decide what to do
                        }
                    }
                    phones.Close(); //not really neccessary, we have "using" above
                }
                //else we cannot get to phones, decide what to do
            }

            return birthdays;
        }
    }
 }

