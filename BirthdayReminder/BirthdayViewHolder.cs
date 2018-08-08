using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace BirthdayReminder
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