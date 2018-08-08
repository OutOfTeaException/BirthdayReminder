using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace BirthdayReminder.Views.Main
{
    public class BirthdayViewHolder : RecyclerView.ViewHolder
    {
        public TextView Name { get; set; }
        public TextView Birthday { get; set; }

        public BirthdayViewHolder(View itemView) : base(itemView)
        {
            Name = itemView.FindViewById<TextView>(Resource.Id.name);
            Birthday = itemView.FindViewById<TextView>(Resource.Id.birthday);
        }
    }
}