using System;
using System.Collections.Generic;
using BirthdayReminder.Model;

namespace BirthdayReminder.Views.Main
{
    public class BirthdayList : List<BirthdayInfo>
    {
        public BirthdayList()
        {
        }

        public BirthdayList(IList<BirthdayInfo> birthdays) : base(birthdays)
        {
        }
    }
}