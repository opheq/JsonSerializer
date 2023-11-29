using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            Console.WriteLine($"Generation {clients.Count} persons completed. Full count with children is {personCount}");
            string fileName = $"C:\\Users\\ouch\\Desktop\\Persons.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new LongToStringConverter() }
            };
            using FileStream createStream = File.Create(fileName);
            JsonSerializer.Serialize(createStream, clients, options);
            createStream.Dispose();
        }

       

        private Person CreateNewRandomPerson(int id,ref int iterator, ref int personCount)
        {
            Person person = new Person(
                id,
                Guid.NewGuid(),       
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

        private string GetRandomFirstName()
        {
            int length = Rand.Next(3, 11);
            return new string(Enumerable.Repeat(Letters, length)
             .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        private string GetRandomLastName()
        {
            int length = Rand.Next(3, 11);
            return new string(Enumerable.Repeat(Letters, length)
             .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        private string[] GetRandomCreditCardNumbers()
        {
            int count = Rand.Next(1, 6);
            return Enumerable.Range(1, count)
             .Select(x => new string(Enumerable.Repeat(Numbers, 16)
              .Select(s => s[Rand.Next(s.Length)]).ToArray()))
             .ToArray();
        }

        private int GetRandomAge() => Rand.Next(18, 99);

        private string[] GetRandomPhones()
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

        private long GetRandomBirthDate(int minYear = 1930, int maxYear = 2000)
        {
            int year = Rand.Next(minYear, maxYear);
            int month = Rand.Next(1, 13);
            int day = Rand.Next(1, DateTime.DaysInMonth(year, month) + 1);
            DateTime date = new DateTime(year, month, day);
            long result = date.ToBinary();
            return result;
        }

        private double GetRandomSalary() => Math.Round(Rand.NextDouble() * 10000, 1);

        private bool GetRandomIsMarred() => Rand.Next(0,2) == 1 ? true : false;

        private Gender GetRandomGender() => Rand.Next(0,2) == 1 ? Gender.Male : Gender.Female;

        private Child[] GetRandomChildren(int id, ref int iterator, ref int personCount, Person parent)
        {
            int count = Rand.Next(0, 3);
            Child[] children = new Child[count];
            for(int i = 0; i < count; i++)
            {
                children[i] = new Child(
                    id + i + 1,
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

    public class LongToStringConverter : JsonConverter<long>
    {
        private const string DateFormat = "yyyy-MM-dd";

        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string dateString = reader.GetString();
            DateTime dateTime = DateTime.ParseExact(dateString, DateFormat, CultureInfo.InvariantCulture);
            return dateTime.ToBinary();
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            DateTime dateTime = DateTime.FromBinary(value);
            writer.WriteStringValue(dateTime.ToString(DateFormat));
        }
    }

    public class GenderToStringConverter : JsonConverter<Gender>
    {
        public override Gender Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() == "Male" ? Gender.Male : Gender.Female;
        }

        public override void Write(Utf8JsonWriter writer, Gender value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value == Gender.Male ? "Male" : "Female");
        }
    }

    public class CreditCardConverter : JsonConverter<string[]>
    {
        public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string jsonString = reader.GetString();
            string[] cardNumbers = jsonString.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            return cardNumbers;
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
        {
            string jsonString = string.Join("-", value);
            writer.WriteStringValue(jsonString);
        }
    }

    class Person
    {
        public int Id { get; set; }
        public Guid TransportId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int SequenceId { get; set; }
        [JsonConverter(typeof(CreditCardConverter))]
        public string[] CreditCardNumbers { get; set; }
        public int Age { get; set; }
        public string[] Phones { get; set; }
        [JsonConverter(typeof(LongToStringConverter))]
        public long BirthDate { get; set; }
        public double Salary { get; set; }
        public bool IsMarred { get; set; }
        [JsonConverter(typeof(GenderToStringConverter))]
        public Gender Gender { get; set; }
        public Child[] Children { get; set; }

        public Person(int id, Guid transportId, string firstName, string lastName, int sequenceId, string[] creditCardNumbers,
            int age, string[] phones, long birthDate, double salary, bool isMarred, Gender gender, Child[] children)
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
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [JsonConverter(typeof(LongToStringConverter))]
        public long BirthDate { get; set; }
        [JsonConverter(typeof(GenderToStringConverter))]
        public Gender Gender { get; set; }

        public Child(int id, string firstName, string lastName, long birthDate, Gender gender)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            Gender = gender;
        }
    }

    public enum Gender
    {
        Male,
        Female
    }
}