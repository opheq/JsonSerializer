using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonSerialization
{

    public enum Gender
    {
        Male,
        Female
    }

    public static class MainApp
    {
        public static void Main()
        {
            SerializeTest serializeTest = new();
            serializeTest.GeneratePersons();
            serializeTest.Serialize();
            serializeTest.ClearPersons();
            serializeTest.Deserialize();
            serializeTest.GetStatistics();
        }
    }

    public class SerializeTest
    {
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
        private const int PersonCount = 10000;
        private const int MinYear = 1930;
        private const int MaxYear = 2000;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new LongToStringConverter(), new GenderToStringConverter() }
        };

        private List<Person>? _persons;

        public Random Rand = new();


        #region GenerateRandomMethods

        private Person CreateNewRandomPerson(int id, ref int iterator, ref int personCount)
        {
            Person person = new(
                id,
                Guid.NewGuid(),
                GenerateRandomFirstName(),
                GenerateRandomLastName(),
                id,
                GenerateRandomCreditCardNumbers(),
                GenerateRandomPhones(),
                GenerateRandomBirthDate(MinYear, MaxYear),
                GenerateRandomSalary(),
                GenerateRandomIsMarred(),
                GenerateRandomGender()
                );
            person.Age = GenerateRandomAge(person.BirthDate);
            person.Children = GenerateRandomChildren(ref iterator, ref personCount, person);
            return person;
        }

        private string GenerateRandomFirstName()
        {
            int length = Rand.Next(3, 11);
            return new string(Enumerable.Repeat(Letters, length)
                .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        private string GenerateRandomLastName()
        {
            int length = Rand.Next(3, 11);
            return new string(Enumerable.Repeat(Letters, length)
                .Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        private string[] GenerateRandomCreditCardNumbers()
        {
            int count = Rand.Next(1, 6);
            return Enumerable.Range(1, count)
                .Select(x => new string(Enumerable.Repeat(Numbers, 16)
                .Select(s => s[Rand.Next(s.Length)]).ToArray()))
                .ToArray();
        }

        private int GenerateRandomAge(long birthDate) => (int)((((DateTime.Now.Year - DateTime.FromBinary(birthDate).Year) * 12) + (DateTime.Now.Month - DateTime.FromBinary(birthDate).Month)) / 12.0);

        private string[] GenerateRandomPhones()
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

        private long GenerateRandomBirthDate(int minYear = MinYear, int maxYear = MaxYear)
        {
            int year = Rand.Next(minYear, maxYear);
            int month = Rand.Next(1, 13);
            int day = Rand.Next(1, DateTime.DaysInMonth(year, month) + 1);
            DateTime date = new(year, month, day);
            long result = date.ToBinary();
            return result;
        }

        private double GenerateRandomSalary() => Math.Round(Rand.NextDouble() * 10000, 1);

        private bool GenerateRandomIsMarred() => Rand.Next(0, 2) == 1;

        private Gender GenerateRandomGender() => Rand.Next(0, 2) == 1 ? Gender.Male : Gender.Female;

        private Child[] GenerateRandomChildren(ref int iterator, ref int personCount, Person parent)
        {
            int count = Rand.Next(0, 3);
            Child[] children = new Child[count];
            for (int i = 0; i < count; i++)
            {
                children[i] = new Child(
                    parent.Id + i + 1,
                    GenerateRandomFirstName(),
                    GenerateRandomLastName(),
                    GenerateRandomBirthDate(DateTime.FromBinary(parent.BirthDate).Year + 18, MaxYear + 18),
                    GenerateRandomGender());
            }
            personCount += count;
            iterator += count;
            return children;
        }

        #endregion GenerateRandomMethods

        public void GeneratePersons()
        {
            _persons = new List<Person>();
            int personCount = PersonCount;
            Console.WriteLine($"Start generation {PersonCount} persons");
            for (int i = 0; i < personCount; i++)
            {
                _persons.Add(CreateNewRandomPerson(i, ref i, ref personCount));
            }
            Console.WriteLine($"Generation {_persons.Count} persons completed. Full count with children is {personCount}");
        }

        public void Serialize()
        {
            string fileName = $"C:\\Users\\{Environment.UserName}\\Desktop\\Persons.json";
            using FileStream createStream = File.Create(fileName);
            JsonSerializer.Serialize(createStream, _persons, _jsonOptions);
            createStream.Dispose();
        }

        public void ClearPersons() => _persons?.Clear();

        public void Deserialize()
        {
            string fileName = $"C:\\Users\\{Environment.UserName}\\Desktop\\Persons.json";
            using FileStream readStream = File.OpenRead(fileName);
            _persons = JsonSerializer.Deserialize<List<Person>>(readStream, _jsonOptions);
            readStream.Dispose();
        }

        public void GetStatistics()
        {
            if (_persons == null)
                return;
            int count = 0;
            int averageChildAge = 0;
            int childCount = 0;
            int creditCardCount = 0;
            foreach (var person in _persons)
            {
                creditCardCount += person.CreditCardNumbers.Length;
                count++;
                if (person.Children.Length > 0)
                {
                    foreach (var child in person.Children)
                    {
                        var birthDate = DateTime.FromBinary(child.BirthDate);
                        averageChildAge += (int)((((DateTime.Now.Year - birthDate.Year) * 12) + (DateTime.Now.Month - birthDate.Month)) / 12.0);
                        childCount++;
                    }
                }
            }
            averageChildAge /= childCount;
            Console.WriteLine($"Person count - {count}\nCredit card count - {creditCardCount}\nAverage child age - {averageChildAge}");
        }
    }

    #region JsonConverters

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

    #endregion JsonConverters

    class Person
    {
        public int Id { get; set; }
        public Guid TransportId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int SequenceId { get; set; }
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
            string[] phones, long birthDate, double salary, bool isMarred, Gender gender)
        {
            Id = id;
            TransportId = transportId;
            FirstName = firstName;
            LastName = lastName;
            SequenceId = sequenceId;
            CreditCardNumbers = creditCardNumbers;
            Age = 0;
            Phones = phones;
            BirthDate = birthDate;
            Salary = salary;
            IsMarred = isMarred;
            Gender = gender;
            Children = Array.Empty<Child>();
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
}