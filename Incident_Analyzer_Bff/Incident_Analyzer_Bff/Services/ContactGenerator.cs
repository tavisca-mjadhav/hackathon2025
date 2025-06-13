using System.Text;

namespace Incident_Analyzer_Bff.Services
{
    public class ContactGenerator
    {
        private static readonly List<string> Names = new List<string>
    {
        "Siddhi Nalam", "Mahesh Jadhav", "Supriya Yadav", "Sachin Kadam", "Alpa Suparia", "Sonali Patni"
    };

        private static readonly Random random = new Random();

        public string GenerateRandomContacts(int count)
        {
            var result = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                string name = Names[random.Next(Names.Count)];
                string[] nameParts = name.Split(' ');

                string firstName = nameParts[0].ToLower();
                string lastName = nameParts.Length > 1 ? nameParts[1].ToLower() : "user";
                string email = $"{firstName}.{lastName}@chase.com";

                string contactNumber = GenerateRandomIndianPhoneNumber();

                result.Append($"name: {name}, contact number: {contactNumber}, email: {email}");
                if (i < count - 1)
                {
                    result.Append(", ");
                }
            }

            return result.ToString();
        }

        private static string GenerateRandomIndianPhoneNumber()
        {
            // Valid Indian mobile number starts with 6, 7, 8 or 9 and is 10 digits long
            int firstDigit = random.Next(6, 10);
            string remainingDigits = random.Next(100000000, 999999999).ToString();
            return firstDigit + remainingDigits;
        }


    }
}
