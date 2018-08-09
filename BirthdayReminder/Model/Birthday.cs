using System;

namespace BirthdayReminder.Model
{
    public class Birthday
    {
        public string Name { get; private set; }
        public DateTime NextBirthday { get; private set; }

        private DateTime birthdate;
        

        public Birthday(string name, DateTime birthdate)
        {
            Name = name;
            this.birthdate = birthdate;

            NextBirthday = new DateTime(DateTime.Today.Year, birthdate.Month, birthdate.Day);
            if (NextBirthday < DateTime.Today)
            {
                NextBirthday = NextBirthday.AddYears(1);
            }
        }

        public bool IsToday()
        {
            return birthdate.Month == DateTime.Today.Month &&
                birthdate.Day == DateTime.Today.Day;
        }

        public bool IsTomorrow()
        {
            var tomorrow = DateTime.Today.AddDays(1);

            return birthdate.Month == tomorrow.Month &&
                birthdate.Day == tomorrow.Day;
        }

        public bool IsNextDays(int days)
        {
            var futureDate = DateTime.Today.AddDays(days);

            return NextBirthday <= futureDate;
        }

        public override string ToString()
        {
            return birthdate.ToString("dd.MM.");
        }
    }
}