using System;
using Android.Support.V7.Widget;
using Android.Views;

namespace BirthdayReminder.Views.Main
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

            string date = birthdayList[position].ToString();
            if (birthdayList[position].IsToday())
            {
                date = "Heute";
            }
            else if (birthdayList[position].IsTomorrow())
            {
                date = "Morgen";
            }

            viewHolder.Birthday.Text = date;
        }

        public override int ItemCount => birthdayList.Count;
    }
}