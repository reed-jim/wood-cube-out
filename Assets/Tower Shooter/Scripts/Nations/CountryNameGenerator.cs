using System;
using System.Collections.Generic;
using UnityEngine;

public class CountryNameGenerator : Singleton<CountryNameGenerator>
{
    [System.Serializable]
    public class NameData
    {
        public List<string> firstNames;
        public List<string> lastNames;

        public NameData(List<string> firstNames, List<string> lastNames)
        {
            this.firstNames = firstNames;
            this.lastNames = lastNames;
        }
    }

    [Header("Generator Settings")]
    [Tooltip("Select the country to pull names from.")]
    public Country selectedCountry = Country.UnitedStates;

    [Header("Fallback Settings")]
    [Tooltip("First names used when an unmapped or invalid country is provided.")]
    public List<string> fallbackFirstNames = new List<string> { "Alex", "Sam", "Jordan", "Taylor", "Morgan" };

    [Tooltip("Last names used when an unmapped or invalid country is provided.")]
    public List<string> fallbackLastNames = new List<string> { "Doe", "Smith", "Johnson", "Brown", "Taylor" };

    private Dictionary<Country, NameData> nameDatabase = new Dictionary<Country, NameData>()
    {
        {
            Country.UnitedStates, new NameData(
                new List<string> { "James", "Emma", "Liam", "Olivia", "Noah", "Sophia", "Ethan", "Ava" },
                new List<string> { "Smith", "Johnson", "Brown", "Garcia", "Miller", "Davis", "Rodriguez", "Wilson" }
            )
        },
        {
            Country.China, new NameData(
                new List<string> { "Wei", "Xiu Ying", "Li Wei", "Ying", "Ming", "Jian", "Xue", "Hao" },
                new List<string> { "Zhang", "Wang", "Chen", "Liu", "Yang", "Huang", "Zhao", "Wu" }
            )
        },
        {
            Country.Germany, new NameData(
                new List<string> { "Maximilian", "Sophia", "Lukas", "Emma", "Jonas", "Hannah", "Leon", "Mia" },
                new List<string> { "Müller", "Schmidt", "Schneider", "Fischer", "Weber", "Meyer", "Wagner", "Becker" }
            )
        },
        {
            Country.Japan, new NameData(
                new List<string> { "Ren", "Aoi", "Yuto", "Hana", "Kenji", "Yui", "Sora", "Akari" },
                new List<string> { "Sato", "Suzuki", "Takahashi", "Tanaka", "Watanabe", "Ito", "Yamamoto", "Nakamura" }
            )
        },
        {
            Country.UnitedKingdom, new NameData(
                new List<string> { "George", "Amelia", "Harry", "Isla", "Oliver", "Ava", "Jack", "Mia" },
                new List<string> { "Smith", "Jones", "Taylor", "Williams", "Brown", "Davies", "Evans", "Wilson" }
            )
        },
        {
            Country.India, new NameData(
                new List<string> { "Aarav", "Ananya", "Vivaan", "Dhiya", "Reyansh", "Saanvi", "Aditya", "Isha" },
                new List<string> { "Sharma", "Patel", "Kumar", "Singh", "Sharma", "Verma", "Gupta", "Mehta" }
            )
        },
        {
            Country.France, new NameData(
                new List<string> { "Gabriel", "Louise", "Léo", "Jade", "Raphaël", "Alice", "Arthur", "Lina" },
                new List<string> { "Martin", "Bernard", "Dubois", "Thomas", "Robert", "Richard", "Petit", "Durand" }
            )
        },
        {
            Country.Italy, new NameData(
                new List<string> { "Leonardo", "Sofia", "Francesco", "Giulia", "Alessandro", "Aurora", "Mattia", "Alice" },
                new List<string> { "Rossi", "Russo", "Ferrari", "Esposito", "Bianchi", "Romano", "Colombo", "Ricci" }
            )
        },
        {
            Country.Russia, new NameData(
                new List<string> { "Alexander", "Sofia", "Dmitry", "Maria", "Maxim", "Anna", "Artyom", "Victoria" },
                new List<string> { "Ivanov", "Smirnov", "Kuznetsov", "Popov", "Sokolov", "Lebedev", "Kozlov", "Novikov" }
            )
        },
        {
            Country.Brazil, new NameData(
                new List<string> { "Lucas", "Sophia", "Mateus", "Isabella", "Gabriel", "Manuela", "Bernardo", "Alice" },
                new List<string> { "Silva", "Santos", "Oliveira", "Souza", "Lima", "Pereira", "Ferreira", "Costa" }
            )
        },
        {
            Country.Canada, new NameData(
                new List<string> { "Liam", "Olivia", "Noah", "Charlotte", "Jackson", "Amelia", "Lucas", "Chloe" },
                new List<string> { "Tremblay", "Roy", "Gagnon", "Lee", "Smith", "Wilson", "Campbell", "Bouchard" }
            )
        },
        {
            Country.Australia, new NameData(
                new List<string> { "Oliver", "Charlotte", "Noah", "Mia", "William", "Isla", "Leo", "Ava" },
                new List<string> { "Smith", "Jones", "Williams", "Brown", "Wilson", "Taylor", "Johnson", "White" }
            )
        },
        {
            Country.Mexico, new NameData(
                new List<string> { "Santiago", "Sofia", "Mateo", "Valentina", "Sebastián", "Regina", "Leonardo", "Camila" },
                new List<string> { "Hernández", "García", "Martínez", "López", "González", "Pérez", "Rodríguez", "Sánchez" }
            )
        },
        {
            Country.Spain, new NameData(
                new List<string> { "Hugo", "Lucía", "Mateo", "Sofía", "Martín", "María", "Lucas", "Paula" },
                new List<string> { "García", "Rodríguez", "González", "Fernández", "López", "Martínez", "Sánchez", "Pérez" }
            )
        },
        {
            Country.SouthKorea, new NameData(
                new List<string> { "Min-jun", "Seo-yun", "Do-yun", "Ha-eun", "Seojun", "Ji-woo", "Eun-woo", "Su-ah" },
                new List<string> { "Kim", "Lee", "Park", "Choi", "Jung", "Kang", "Cho", "Yoon" }
            )
        },
        {
            Country.Turkey, new NameData(
                new List<string> { "Yusuf", "Zeynep", "Eren", "Elif", "Miraç", "Defne", "Ömer", "As linen" },
                new List<string> { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Yıldırım", "Aydın" }
            )
        },
        {
            Country.Indonesia, new NameData(
                new List<string> { "Budi", "Siti", "Agus", "Dewi", "Rizky", "Nur", "Andi", "Putri" },
                new List<string> { "Santoso", "Wijaya", "Lestari", "Hidayat", "Pratama", "Kusuma", "Saputra", "Utami" }
            )
        },
        {
            Country.Netherlands, new NameData(
                new List<string> { "Daan", "Emma", "Noah", "Sophie", "Lucas", "Julia", "Levi", "Mila" },
                new List<string> { "de Jong", "Jansen", "de Vries", "van de Berg", "Bakker", "van Dijk", "Visser", "Smit" }
            )
        },
        {
            Country.SaudiArabia, new NameData(
                new List<string> { "Mohammed", "Fatimah", "Omar", "Sara", "Abdullah", "Noura", "Ali", "Reem" },
                new List<string> { "Al-Harbi", "Al-Ghamdi", "Al-Dawsari", "Al-Qahtani", "Al-Otaibi", "Al-Zahrani", "Al-Shehri", "Al-Mald" }
            )
        },
        {
            Country.Switzerland, new NameData(
                new List<string> { "Noah", "Mia", "Liam", "Emma", "Matteo", "Elena", "Gabriel", "Sofia" },
                new List<string> { "Bachmann", "Staub", "Meier", "Rossi", "Müller", "Bernasconi", "Favre", "Keller" }
            )
        },
        {
            Country.Poland, new NameData(
                new List<string> { "Antoni", "Zofia", "Jan", "Julia", "Aleksander", "Maja", "Nikodem", "Hanna" },
                new List<string> { "Nowak", "Kowalski", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński", "Lewandowski", "Zieliński" }
            )
        },
        {
            Country.Taiwan, new NameData(
                new List<string> { "Po-Chun", "Ting-Yu", "Chia-Hao", "Ya-Ting", "Kuan-Yu", "Yu-Ting", "Yu-Xiang", "Ying-Xuan" },
                new List<string> { "Chen", "Lin", "Huang", "Chang", "Li", "Wang", "Wu", "Liu" }
            )
        },
        {
            Country.Ireland, new NameData(
                new List<string> { "Jack", "Grace", "Conor", "Aoife", "Daniel", "Emily", "Finn", "Fiadh" },
                new List<string> { "Murphy", "Kelly", "O'Brien", "Walsh", "Byrne", "Ryan", "O'Connor", "O'Neill" }
            )
        },
        {
            Country.Belgium, new NameData(
                new List<string> { "Arthur", "Olivia", "Noah", "Emma", "Liam", "Mila", "Louis", "Louise" },
                new List<string> { "Peeters", "Janssens", "Maes", "Jacobs", "Mertens", "Willems", "Claes", "Goossens" }
            )
        },
        {
            Country.Sweden, new NameData(
                new List<string> { "Lucas", "Alice", "Liam", "Maja", "William", "Vera", "Elias", "Alma" },
                new List<string> { "Andersson", "Johansson", "Karlsson", "Nilsson", "Eriksson", "Larsson", "Olsson", "Persson" }
            )
        },
        {
            Country.Israel, new NameData(
                new List<string> { "David", "Tamar", "Ariel", "Noa", "Lavi", "Maya", "Uri", "Avigail" },
                new List<string> { "Cohen", "Levi", "Mizrahi", "Peretz", "Biton", "Dahan", "Avraham", "Friedman" }
            )
        },
        {
            Country.Argentina, new NameData(
                new List<string> { "Mateo", "Emma", "Thiago", "Mia", "Bautista", "Isabella", "Felipe", "Catalina" },
                new List<string> { "González", "Rodríguez", "Gómez", "Fernández", "López", "Díaz", "Martínez", "Pérez" }
            )
        },
        {
            Country.Singapore, new NameData(
                new List<string> { "Ryan", "Chloe", "Ethan", "Rachel", "Jayden", "Megan", "Lucas", "Hannah" },
                new List<string> { "Tan", "Lee", "Lim", "Ng", "Wong", "Ong", "Chua", "Koh" }
            )
        },
        {
            Country.Austria, new NameData(
                new List<string> { "Paul", "Anna", "Jakob", "Laura", "Maximilian", "Marie", "Tobias", "Lena" },
                new List<string> { "Gruber", "Huber", "Bauer", "Pichler", "Steiner", "Moser", "Mayer", "Hofer" }
            )
        },
        {
            Country.UnitedArabEmirates, new NameData(
                new List<string> { "Zayed", "Mariam", "Rashid", "Shamma", "Sultan", "Fatima", "Hamdan", "Aisha" },
                new List<string> { "Al-Maktoum", "Al-Mansoori", "Al-Nuaimi", "Al-Raisi", "Al-Zaabi", "Al-Ali", "Al-Shamsi", "Al-Kait" }
            )
        },
        {
            Country.Norway, new NameData(
                new List<string> { "Oliver", "Nora", "Jakob", "Astrid", "Emil", "Emma", "Noah", "Sofie" },
                new List<string> { "Hansen", "Johansen", "Olsen", "Larsen", "Berg", "Haugen", "Hagen", "Jakobsen" }
            )
        },
        {
            Country.Thailand, new NameData(
                new List<string> { "Somchai", "Mali", "Kittisak", "Anong", "Niran", "Siriporn", "Arthit", "Kanya" },
                new List<string> { "Saetang", "Suwannarat", "Phungphon", "Wongsuwan", "Chaiyasing", "Saelim", "Thongchai", "Bunsri" }
            )
        },
        {
            Country.Colombia, new NameData(
                new List<string> { "Samuel", "Salomé", "Santiago", "Isabella", "Jerónimo", "Mariana", "Matías", "Gabriela" },
                new List<string> { "Rodríguez", "Gómez", "Morales", "Jiménez", "García", "Martínez", "López", "Hernández" }
            )
        },
        {
            Country.Vietnam, new NameData(
                new List<string> { "Van Minh", "Thi Hoa", "Hoang Nam", "Phuong Thao", "Duc Anh", "Khanh Linh", "Quang Huy", "Bao Ngoc" },
                new List<string> { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Huynh", "Vu", "Dang" }
            )
        },
        {
            Country.Malaysia, new NameData(
                new List<string> { "Muhammad", "Nurul", "Ahmad", "Siti", "Daniel", "Aina", "Adam", "Nur" },
                new List<string> { "Lee", "Tan", "Ismail", "Abdullah", "Wong", "Chong", "Lim", "Muthusamy" }
            )
        },
        {
            Country.Philippines, new NameData(
                new List<string> { "Juan", "Maria", "Mark", "Angela", "Gabriel", "Samantha", "Angelo", "Princess" },
                new List<string> { "Dela Cruz", "Santos", "Reyes", "Cruz", "Bautista", "Ocampo", "Garcia", "Mendoza" }
            )
        },
        {
            Country.Bangladesh, new NameData(
                new List<string> { "Arif", "Nusrat", "Tanvir", "Sadia", "Shakib", "Tasnim", "Mehedi", "Fariha" },
                new List<string> { "Rahman", "Jahan", "Ahmed", "Islam", "Hossain", "Chowdhury", "Khan", "Ali" }
            )
        },
        {
            Country.Denmark, new NameData(
                new List<string> { "William", "Ella", "Noah", "Freja", "Oscar", "Alma", "Lucas", "Clara" },
                new List<string> { "Nielsen", "Jensen", "Hansen", "Pedersen", "Andersen", "Christensen", "Larsen", "Sørensen" }
            )
        },
        {
            Country.Romania, new NameData(
                new List<string> { "Andrei", "Maria", "Stefan", "Elena", "Alexandru", "Ioana", "Gabriel", "Sofia" },
                new List<string> { "Popa", "Radu", "Dumitru", "Stan", "Stanciu", "Ionescu", "Popescu", "Gheorghe" }
            )
        },
        {
            Country.SouthAfrica, new NameData(
                new List<string> { "Siyabonga", "Chloe", "Lethabo", "Liam", "Junior", "Melokuhle", "Bandile", "Amahle" },
                new List<string> { "Nkosi", "van der Merwe", "Ndlovu", "Smith", "Khumalo", "Botha", "Sithole", "Coetzee" }
            )
        },
        {
            Country.HongKong, new NameData(
                new List<string> { "Ka-Hao", "Wing-Yan", "Chun-Yin", "Hiu-Tung", "Lok-Yin", "Sze-Man", "Tsz-Hin", "Ying-Chi" },
                new List<string> { "Chan", "Wong", "Lee", "Cheung", "Lau", "Chan", "Leung", "Wu" }
            )
        },
        {
            Country.CzechRepublic, new NameData(
                new List<string> { "Jan", "Eliška", "Tomáš", "Adéla", "Jakub", "Anna", "Matyáš", "Tereza" },
                new List<string> { "Novák", "Svobodová", "Novotný", "Dvořáková", "Černý", "Procházka", "Kučera", "Veselý" }
            )
        },
        {
            Country.Egypt, new NameData(
                new List<string> { "Youssef", "Mariam", "Omar", "Nour", "Ahmed", "Fatma", "Mohamed", "Hana" },
                new List<string> { "Mohamed", "Ahmed", "Mahmoud", "Hassan", "Ibrahim", "Ali", "El-Sayed", "Mustafa" }
            )
        },
        {
            Country.Chile, new NameData(
                new List<string> { "Mateo", "Sofia", "Benjamín", "Isabella", "Lucas", "Emilia", "Tomas", "Agustina" },
                new List<string> { "Muñoz", "Rojas", "Díaz", "Pérez", "Soto", "Contreras", "Silva", "Sepúlveda" }
            )
        },
        {
            Country.Pakistan, new NameData(
                new List<string> { "Muhammad", "Fatima", "Ahmed", "Ayesha", "Ali", "Zainab", "Umar", "Hoorain" },
                new List<string> { "Khan", "Malik", "Raza", "Ahmed", "Chaudhry", "Shah", "Bhatti", "Iqbal" }
            )
        },
        {
            Country.Peru, new NameData(
                new List<string> { "Thiago", "Mia", "Liam", "Luana", "Gael", "Camila", "Mateo", "Alessia" },
                new List<string> { "Quispe", "Flores", "Sánchez", "Rodríguez", "García", "Rojas", "Díaz", "Reyes" }
            )
        },
        {
            Country.Portugal, new NameData(
                new List<string> { "Francisco", "Maria", "João", "Leonor", "Afonso", "Matilde", "Dinis", "Carolina" },
                new List<string> { "Silva", "Santos", "Ferreira", "Pereira", "Oliveira", "Costa", "Rodrigues", "Martins" }
            )
        },
        {
            Country.Nigeria, new NameData(
                new List<string> { "Chidubem", "Aminat", "Emeka", "Zainab", "Oluwaseun", "Fatima", "Tobi", "Chiamaka" },
                new List<string> { "Okafor", "Adebayo", "Nwosu", "Abubakar", "Balogun", "Eke", "Bello", "Okonkwo" }
            )
        },
        {
            Country.Kazakhstan, new NameData(
                new List<string> { "Alihan", "Madina", "Islam", "Aruzhan", "Nursultan", "Ayala", "Amir", "Rayana" },
                new List<string> { "Nurmagambetov", "Saduakasova", "Omarov", "Bolatova", "Zhakupov", "Kim", "Serikov", "Ibraev" }
            )
        },
        {
            Country.Finland, new NameData(
                new List<string> { "Eino", "Aino", "Oliver", "Venla", "Väinö", "Sofia", "Eeli", "Aada" },
                new List<string> { "Korhonen", "Virtanen", "Mäkinen", "Nieminen", "Mäkelä", "Hämäläinen", "Laine", "Heikkinen" }
            )
        }
    };

    /// <summary>
    /// Returns a randomly combined full name from the specified country enum.
    /// Falls back gracefully if the enum or key is not found.
    /// </summary>
    public string GetRandomName(Country country)
    {
        // Check if the enum integer value is defined in the enum type
        if (!Enum.IsDefined(typeof(Country), country))
        {
            Debug.LogWarning($"Country enum value '{(int)country}' is not defined. Using fallback.");
            return GetFallbackName();
        }

        // Check if the country exists in the internal dictionary
        if (nameDatabase.TryGetValue(country, out NameData data))
        {
            if (data.firstNames != null && data.firstNames.Count > 0 &&
                data.lastNames != null && data.lastNames.Count > 0)
            {
                int randomFirstIndex = UnityEngine.Random.Range(0, data.firstNames.Count);
                int randomLastIndex = UnityEngine.Random.Range(0, data.lastNames.Count);

                string firstName = data.firstNames[randomFirstIndex];
                string lastName = data.lastNames[randomLastIndex];

                // East Asian cultural order (Surname First)
                if (country == Country.China || country == Country.Japan ||
                    country == Country.SouthKorea || country == Country.Vietnam ||
                    country == Country.HongKong || country == Country.Taiwan)
                {
                    return $"{lastName} {firstName}";
                }

                return $"{firstName} {lastName}";
            }
        }

        Debug.LogWarning($"Name database does not contain entries for '{country}'. Using fallback.");
        return GetFallbackName();
    }

    /// <summary>
    /// Overload that string-parses a country name (e.g. "Iceland") and falls back if unsupported.
    /// </summary>
    public string GetRandomName(string countryName)
    {
        if (Enum.TryParse(countryName, true, out Country parsedCountry))
        {
            return GetRandomName(parsedCountry);
        }

        Debug.LogWarning($"Country string '{countryName}' could not be matched to an enum. Using fallback.");
        return GetFallbackName();
    }

    /// <summary>
    /// Overload that uses the inspector-selected country.
    /// </summary>
    public string GetRandomName()
    {
        return GetRandomName(selectedCountry);
    }

    /// <summary>
    /// Generates a name from the fallback lists or returns a default placeholder.
    /// </summary>
    private string GetFallbackName()
    {
        if (fallbackFirstNames != null && fallbackFirstNames.Count > 0 &&
            fallbackLastNames != null && fallbackLastNames.Count > 0)
        {
            string first = fallbackFirstNames[UnityEngine.Random.Range(0, fallbackFirstNames.Count)];
            string last = fallbackLastNames[UnityEngine.Random.Range(0, fallbackLastNames.Count)];
            return $"{first} {last}";
        }

        return "John Doe";
    }

    [ContextMenu("Test Random Name")]
    private void TestRandomName()
    {
        string randomName = GetRandomName();
        Debug.Log($"[{selectedCountry}] Generated Name: {randomName}");
    }

    [ContextMenu("Test Fallback (Invalid Enum)")]
    private void TestInvalidEnumFallback()
    {
        Country invalidCountry = (Country)999;
        string randomName = GetRandomName(invalidCountry);
        Debug.Log($"[Invalid Enum] Generated Name: {randomName}");
    }

    [ContextMenu("Test Fallback (Unmapped String)")]
    private void TestStringFallback()
    {
        string randomName = GetRandomName("Iceland");
        Debug.Log($"[String Input 'Iceland'] Generated Name: {randomName}");
    }
}
