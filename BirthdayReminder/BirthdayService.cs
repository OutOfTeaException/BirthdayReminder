﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Provider;

namespace BirthdayReminder
{
    public class BirthdayService : ContextWrapper
    {
        public BirthdayService(Context context) : base(context)
        {
        }

        public IList<(String name, DateTime birthday)> GetNextBirthdays(int daysInFuture)
        {
            var birthdays = GetBirthdaysFromContacts();

            var nextBirthdays = from b in birthdays.Distinct()
                                 let bNow = new DateTime(DateTime.Today.Year, b.birthday.Month, b.birthday.Day)
                                 where bNow >= DateTime.Today && bNow <= DateTime.Today.AddDays(daysInFuture)
                                 orderby bNow
                                 select b;

            return nextBirthdays.ToList();
        }

        private IList<(String name, DateTime birthday)> GetBirthdaysFromContacts()
        {
            var birthdays = new List<(String, DateTime)>();

            var uri = ContactsContract.Data.ContentUri;
            string[] projection = {
               ContactsContract.Contacts.InterfaceConsts.Id,
               ContactsContract.Contacts.InterfaceConsts.DisplayName,
               ContactsContract.CommonDataKinds.Event.StartDate
            };

            string query = "data2=3"; // Type=Birthday

            using (var phones = ApplicationContext.ContentResolver.Query(uri, projection, query, null, null))
            {
                if (phones != null)
                {
                    while (phones.MoveToNext())
                    {
                        try
                        {
                            string name = phones.GetString(phones.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                            string birthdayValue = phones.GetString(phones.GetColumnIndex(ContactsContract.CommonDataKinds.Event.StartDate));

                            if (DateTime.TryParseExact(birthdayValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime birthday))
                            {
                                birthdays.Add((name, birthday));
                                //yield return (name, birthday);
                            }

                            //phoneContacts.Add(name);
                        }
                        catch (Exception ex)
                        {
                            //something wrong with one contact, may be display name is completely empty, decide what to do
                        }
                    }
                    phones.Close(); 
                }
            }

            return birthdays;
        }
    }
}