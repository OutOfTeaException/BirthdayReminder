using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BirthdayReminder
{
    public class NotificationHelper : ContextWrapper
    {
        public const string PRIMARY_CHANNEL = "default";

        NotificationManager manager;
        NotificationManager Manager
        {
            get
            {
                if (manager == null)
                {
                    manager = (NotificationManager)GetSystemService(NotificationService);
                }
                return manager;
            }
        }

        int SmallIcon => Android.Resource.Drawable.StatNotifyChat;

        public NotificationHelper(Context context) : base(context)
        {
            var channel = new NotificationChannel(PRIMARY_CHANNEL, PRIMARY_CHANNEL, NotificationImportance.Default);
            channel.LightColor = Color.Green;
            channel.EnableVibration(true);
            channel.LockscreenVisibility = NotificationVisibility.Private;
            
            Manager.CreateNotificationChannel(channel);
        }

        public Notification.Builder GetNotification(String title, String body)
        {
            Notification.BigTextStyle textStyle = new Notification.BigTextStyle();
            textStyle.BigText(body);

            return new Notification.Builder(ApplicationContext, PRIMARY_CHANNEL)
                     .SetContentTitle(title)
                     .SetContentText(body)
                     .SetSmallIcon(SmallIcon)
                     .SetAutoCancel(true)
                     .SetStyle(textStyle);
        }

        public void Notify(int id, Notification.Builder notification)
        {
            Manager.Notify(id, notification.Build());
        }
    }
}