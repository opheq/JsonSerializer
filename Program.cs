using System;
using System.Diagnostics;
using System.Text.Json;

namespace JsonSerialization
{

    public static class MainApp
    {
        public static void Main(string[] args)
        {
            PersonGenerator personGenerator = new PersonGenerator();
            personGenerator.Main(args);
        }
    }

    public class PersonGenerator
    {
        const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        const string Numbers = "0123456789";
        const int PersonCount = 10000;
        const int MinYear = 1930;
        const int MaxYear = 2000;

        public Random Rand;

        public void Main(string[] args)
        {
            Rand = new Random();
            List<Person> clients = new List<Person>();
            int personCount = PersonCount;
            Console.WriteLine($"Start generation {PersonCount} persons");
            for (int i = 0; i < personCount; i++)
            {
                clients.Add(CreateNewRandomPerson(i, ref i, ref personCount));
            }
            Console.WriteLine($"Generation {PersonCount} persons completed. Full count with children is {personCount}");
        }

        private Person CreateNewRandomPerson(int id,ref int iterator, ref int personCount)
        {
            Person person = new Person(
                id,
                new Guid(),
                GetRandomFirstName(),
                GetRandomLastName(),
                id,
                GetRandomCreditCardNumbers(),
                GetRandomAge(),
                GetRandomPhones(),
                GetRandomBirthDate(MinYear, MaxYear),
                GetRandomSalary(),
                GetRandomIsMarred(),
                GetRandomGender(),
                null
                );
            person.Children = GetRandomChildren(id,ref iterator, ref personCount, person);
            return person;
        }

        private String GetRandomFirstName()
        {
            int length = Rand.Next(3, 11);
            return new string(Enumerable.Repeat(Letters, length)
             .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        private String GetRandomLastName()
        {
            int length = Rand.Next(3, 11);
            return new string(Enumerable.Repeat(Letters, length)
             .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        private String[] GetRandomCreditCardNumbers()
        {
            int count = Rand.Next(1, 6);
            return Enumerable.Range(1, count)
             .Select(x => new string(Enumerable.Repeat(Numbers, 12)
              .Select(s => s[Rand.Next(s.Length)]).ToArray()))
             .ToArray();
        }

        private Int32 GetRandomAge() => Rand.Next(18, 99);

        private String[] GetRandomPhones()
        {
            int length = Rand.Next(0, 6);
            string[] phones = new string[length];
            for (int i = 0; i < length; i++)
            {
                long number = Rand.NextInt64(80000000000, 90000000000);
                phones[i] = number.ToString();
            }
            return phones;
        }

        private Int64 GetRandomBirthDate(int minYear, int maxYear)
        {
            int year = Rand.Next(1930, 2000);
            int month = Rand.Next(1, 13);
            int day = Rand.Next(1, DateTime.DaysInMonth(year, month) + 1);
            DateTime date = new DateTime(year, month, day);
            long result = date.Ticks;

            return result;
        }

        private Double GetRandomSalary() => Math.Round(Rand.NextDouble() * 10000, 1);

        private Boolean GetRandomIsMarred() => Rand.Next(0,2) == 1 ? true : false;

        private Gender GetRandomGender() => Rand.Next(0,2) == 1 ? Gender.Male : Gender.Female;

        private Child[] GetRandomChildren(int id, ref int iterator, ref int personCount, Person parent)
        {
            int count = Rand.Next(0, 3);
            Child[] children = new Child[count];
            for(int i = 0; i < count; i++)
            {
                children[i] = new Child(
                    id,
                    GetRandomFirstName(),
                    GetRandomLastName(),
                    GetRandomBirthDate(DateTime.FromBinary(parent.BirthDate).Year + 18, MaxYear + 23),
                    GetRandomGender());
            }
            personCount += count;
            iterator += count;
            return children;
        }
    }

    class Person
    {
        public Int32 Id { get; set; }
        public Guid TransportId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Int32 SequenceId { get; set; }
        public String[] CreditCardNumbers { get; set; }
        public Int32 Age { get; set; }
        public String[] Phones { get; set; }
        public Int64 BirthDate { get; set; }
        public Double Salary { get; set; }
        public Boolean IsMarred { get; set; }
        public Gender Gender { get; set; }
        public Child[] Children { get; set; }

        public Person(Int32 id, Guid transportId, String firstName, String lastName, Int32 sequenceId, String[] creditCardNumbers, 
            Int32 age, String[] phones, Int64 birthDate, Double salary, Boolean isMarred, Gender gender, Child[] children)
        {
            Id = id;
            TransportId = transportId;
            FirstName = firstName;
            LastName = lastName;
            SequenceId = sequenceId;
            CreditCardNumbers = creditCardNumbers;
            Age = age;
            Phones = phones;
            BirthDate = birthDate;
            Salary = salary;
            IsMarred = isMarred;
            Gender = gender;
            Children = children;
        }
    }

    class Child
    {
        public Int32 Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Int64 BirthDate { get; set; }
        public Gender Gender { get; set; }

        public Child(Int32 id, String firstName, String lastName, Int64 birthDate, Gender gender)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            Gender = gender;
        }
    }

    enum Gender
    {
        Male,
        Female
    }
}