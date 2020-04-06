using System;
using Android.App;
using Android.Content;
using Android.Graphics;

namespace BirthdayReminder.Services
{
    public class NotificationService : ContextWrapper
    {
        public const string PRIMARY_CHANNEL = "default";

        private NotificationManager manager;

        int SmallIcon => Resource.Drawable.birthday;

        public NotificationService(Context context) : base(context)
        {
            manager = (NotificationManager)GetSystemService(NotificationService);
            var channel = new NotificationChannel(PRIMARY_CHANNEL, PRIMARY_CHANNEL, NotificationImportance.Default);
            channel.LightColor = Color.Green;
            channel.EnableVibration(true);
            channel.LockscreenVisibility = NotificationVisibility.Private;
            
            manager.CreateNotificationChannel(channel);
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
            manager.Notify(id, notification.Build());
        }
    }
}