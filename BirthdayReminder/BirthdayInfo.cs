using System;

namespace BirthdayReminder
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