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
    public class BirthdayListAdapter : RecyclerView.Adapter
    {
        private BirthdayList birthdayList;

        public BirthdayListAdapter(BirthdayList birthdayList)
        {
            this.birthdayList = birthdayList;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View viewItem = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.BirthdayCardView, parent, false);

            BirthdayViewHolder viewHolder = new BirthdayViewHolder(viewItem);
            return viewHolder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            BirthdayViewHolder viewHolder = holder as BirthdayViewHolder;

            viewHolder.Name.Text = birthdayList[position].Name;
            viewHolder.Birthday.Text = birthdayList[position].Birthday.ToString("dd.MM.");
        }

        public override int ItemCount => birthdayList.Count;
    }
}