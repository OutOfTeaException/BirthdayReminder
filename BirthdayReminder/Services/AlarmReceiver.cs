using System;
using Android.Content;

namespace BirthdayReminder.Services
{
    [BroadcastReceiver]
    public class AlarmReceiver : BroadcastReceiver
    {

        public override void OnReceive(Context context, Intent intent)
        {
            new BirthdayCheckService(context).CheckBirthdays();
        }
    }
}