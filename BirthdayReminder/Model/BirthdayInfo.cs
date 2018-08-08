using System;

namespace BirthdayReminder.Model
{
    public class BirthdayInfo
    {
        public string Name { get; }
        public DateTime Birthday { get; }

        public BirthdayInfo(string name, DateTime birthday)
        {
            Name = name;
            Birthday = birthday;
        }
    }
}