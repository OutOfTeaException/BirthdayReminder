using System;
using System.Collections.Generic;
using System.Linq;


namespace BirthdayReminder
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