using System;
using System.Collections.Generic;
using BirthdayReminder.Model;

namespace BirthdayReminder.Views.Main
{
    public class BirthdayList : List<Birthday>
    {
        public BirthdayList()
        {
        }

        public BirthdayList(IList<Birthday> birthdays) : base(birthdays)
        {
        }
    }
}