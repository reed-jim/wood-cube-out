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

    private static readonly HashSet<Country> EasternNamingOrderCountries = new HashSet<Country>
    {
        Country.China, Country.Japan, Country.SouthKorea, Country.NorthKorea,
        Country.Vietnam, Country.HongKong, Country.Taiwan, Country.Macao, Country.Hungary
    };

    private Dictionary<Country, NameData> nameDatabase = new Dictionary<Country, NameData>()
    {
        { Country.Andorra, new NameData(new List<string> { "Marc", "Pol", "Laia", "Júlia", "Arnau", "Martí" }, new List<string> { "Vidal", "Serra", "Riba", "Casals", "Bosch", "Puig" }) },
        { Country.UnitedArabEmirates, new NameData(new List<string> { "Zayed", "Mariam", "Rashid", "Shamma", "Sultan", "Fatima" }, new List<string> { "Al-Maktoum", "Al-Mansoori", "Al-Nuaimi", "Al-Raisi", "Al-Zaabi", "Al-Ali" }) },
        { Country.Afghanistan, new NameData(new List<string> { "Ahmad", "Fatima", "Mohammad", "Zahra", "Bilal", "Soreya" }, new List<string> { "Khan", "Ahmadi", "Mohammadi", "Popal", "Durrani", "Karimi" }) },
        { Country.AntiguaAndBarbuda, new NameData(new List<string> { "Kofi", "Aaliyah", "Devon", "Tia", "Kareem", "Chloe" }, new List<string> { "Joseph", "Thomas", "Browne", "Williams", "James", "Benjamin" }) },
        { Country.Anguilla, new NameData(new List<string> { "Jalen", "Kiana", "Tariq", "Aria", "Malik", "Brianna" }, new List<string> { "Richardson", "Gumbs", "Hodge", "Hughes", "Webster", "Proctor" }) },
        { Country.Albania, new NameData(new List<string> { "Arben", "Teuta", "Erion", "Pranvera", "Bledar", "Anila" }, new List<string> { "Hoxha", "Shehu", "Basha", "Krasniqi", "Gashi", "Leka" }) },
        { Country.Armenia, new NameData(new List<string> { "Arman", "Anahit", "Davit", "Gayane", "Gor", "Nare" }, new List<string> { "Sargsyan", "Harutyunyan", "Hovhannisyan", "Hakobyan", "Grigoryan", "Petrosyan" }) },
        { Country.Angola, new NameData(new List<string> { "Mateus", "Esperança", "João", "Ana", "Manuel", "Ndalu" }, new List<string> { "Dos Santos", "Gonçalves", "Neto", "Manuel", "Antonio", "Ferreira" }) },
        { Country.Antarctica, new NameData(new List<string> { "Scott", "Amundsen", "Shackleton", "Mawson", "Byrd", "Elena" }, new List<string> { "Frost", "Snow", "Ice", "Winter", "Glacier", "Storm" }) },
        { Country.Argentina, new NameData(new List<string> { "Mateo", "Emma", "Thiago", "Mia", "Bautista", "Isabella" }, new List<string> { "González", "Rodríguez", "Gómez", "Fernández", "López", "Díaz" }) },
        { Country.AmericanSamoa, new NameData(new List<string> { "Sione", "Sina", "Tavita", "Tala", "Kelepi", "Lupe" }, new List<string> { "Tuitele", "Soliai", "Galea'i", "Mata'utia", "Fa'amuina", "Tuiasosopo" }) },
        { Country.Austria, new NameData(new List<string> { "Paul", "Anna", "Jakob", "Laura", "Maximilian", "Marie" }, new List<string> { "Gruber", "Huber", "Bauer", "Pichler", "Steiner", "Moser" }) },
        { Country.Australia, new NameData(new List<string> { "Oliver", "Charlotte", "Noah", "Mia", "William", "Isla" }, new List<string> { "Smith", "Jones", "Williams", "Brown", "Wilson", "Taylor" }) },
        { Country.Aruba, new NameData(new List<string> { "Kevin", "Kiara", "Gianni", "Xenia", "Denzell", "Alyssa" }, new List<string> { "Kro surface", "Wever", "Tromp", "Maduro", "Franken", "Croes" }) },
        { Country.AlandIslands, new NameData(new List<string> { "Emil", "Ida", "Elias", "Elin", "Filip", "Saga" }, new List<string> { "Jansson", "Karlsson", "Mattsson", "Eriksson", "Sundblom", "Lindholm" }) },
        { Country.Azerbaijan, new NameData(new List<string> { "Ali", "Leyla", "Elvin", "Aysel", "Rashad", "Nigar" }, new List<string> { "Mammadov", "Aliyev", "Hasanov", "Huseynov", "Guliyev", "Ismayilov" }) },
        { Country.BosniaAndHerzegovina, new NameData(new List<string> { "Amar", "Amina", "Luka", "Sara", "Tarik", "Emina" }, new List<string> { "Hodžić", "Hadžić", "Kovačević", "Marić", "Babic", "Delic" }) },
        { Country.Barbados, new NameData(new List<string> { "Kareem", "Rihanna", "Devon", "Aaliyah", "Dario", "Tia" }, new List<string> { "Haynes", "Clarke", "Hunte", "Boyce", "Springer", "Ford" }) },
        { Country.Bangladesh, new NameData(new List<string> { "Arif", "Nusrat", "Tanvir", "Sadia", "Shakib", "Tasnim" }, new List<string> { "Rahman", "Jahan", "Ahmed", "Islam", "Hossain", "Chowdhury" }) },
        { Country.Belgium, new NameData(new List<string> { "Arthur", "Olivia", "Noah", "Emma", "Liam", "Mila" }, new List<string> { "Peeters", "Janssens", "Maes", "Jacobs", "Mertens", "Willems" }) },
        { Country.BurkinaFaso, new NameData(new List<string> { "Moussa", "Awa", "Ibrahim", "Fatoumata", "Oumar", "Aminata" }, new List<string> { "Ouédraogo", "Compabré", "Traoré", "Kaboré", "Sow", "Zongo" }) },
        { Country.Bulgaria, new NameData(new List<string> { "Georgi", "Victoria", "Alexander", "Maria", "Dimitar", "Nikol" }, new List<string> { "Ivanov", "Georgiev", "Dimitrov", "Petrov", "Nikolov", "Stoyanov" }) },
        { Country.Bahrain, new NameData(new List<string> { "Mohammed", "Fatima", "Ali", "Zahra", "Youssef", "Maryam" }, new List<string> { "Al-Khalifa", "Al-Baharna", "Fakhro", "Al-Mahmood", "Al-Sayed", "Al-Kooheji" }) },
        { Country.Burundi, new NameData(new List<string> { "Jean", "Aline", "Fabrice", "Inès", "Thierry", "Chantal" }, new List<string> { "Nkurunziza", "Ndayishimiye", "Nijimbere", "Ndikumasabo", "Irakoze", "Bizimana" }) },
        { Country.Benin, new NameData(new List<string> { "Koffi", "Ahouefa", "Sèna", "Fèmi", "Gilles", "Nadège" }, new List<string> { "Dossou", "Soglo", "Houngbo", "Bio", "Mensah", "Agbossa" }) },
        { Country.SaintBarthelemy, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Blondin", "Questel", "Lédée", "Magras", "Brin", "Greaux" }) },
        { Country.Bermuda, new NameData(new List<string> { "Tyree", "Aaliyah", "Kevon", "Maya", "Dante", "Sierra" }, new List<string> { "Outerbridge", "Furbert", "Smith", "Tucker", "Simons", "Zuill" }) },
        { Country.Brunei, new NameData(new List<string> { "Aziz", "Nurul", "Hafiz", "Aisyah", "Khairul", "Siti" }, new List<string> { "Haji", "Bakar", "Hassan", "Ahmad", "Ibrahim", "Sulaiman" }) },
        { Country.Bolivia, new NameData(new List<string> { "Mateo", "Sofia", "Thiago", "Valentina", "Joaquín", "Luciana" }, new List<string> { "Mamani", "Quispe", "Flores", "Vargas", "Rojas", "Gutiérrez" }) },
        { Country.BonaireSintEustatiusAndSaba, new NameData(new List<string> { "Jaden", "Sharifa", "Dangelo", "Kiara", "Gerson", "Tamara" }, new List<string> { "Martis", "Janga", "Winklaar", "Statia", "Lopes", "Dirksz" }) },
        { Country.Brazil, new NameData(new List<string> { "Lucas", "Sophia", "Mateus", "Isabella", "Gabriel", "Manuela" }, new List<string> { "Silva", "Santos", "Oliveira", "Souza", "Lima", "Pereira" }) },
        { Country.Bahamas, new NameData(new List<string> { "Kevaughn", "Tynesha", "Malik", "Anaya", "Deangelo", "Kendra" }, new List<string> { "Rolle", "Strachan", "Ferguson", "Bethel", "Symonette", "Stubbs" }) },
        { Country.Bhutan, new NameData(new List<string> { "Jigme", "Pema", "Tenzin", "Dechen", "Karma", "Deki" }, new List<string> { "Dorji", "Wangchuk", "Gyeltshen", "Norbu", "Tshering", "Zangmo" }) },
        { Country.BouvetIsland, new NameData(new List<string> { "Lars", "Astrid", "Sverre", "Freja", "Magnus", "Ingrid" }, new List<string> { "Amundsen", "Nansen", "Christensen", "Grytviken", "Kerguelen", "Polar" }) },
        { Country.Botswana, new NameData(new List<string> { "Kabo", "Mpho", "Tshepo", "Lesedi", "Thabo", "Kealeboga" }, new List<string> { "Kgosi", "Mogae", "Khama", "Molefe", "Phumaphi", "Seboni" }) },
        { Country.Belarus, new NameData(new List<string> { "Maxim", "Darya", "Artyom", "Alena", "Uladzislau", "Katsiaryna" }, new List<string> { "Ivanov", "Kozlov", "Kovalenko", "Morozov", "Novik", "Zhuk" }) },
        { Country.Belize, new NameData(new List<string> { "Giovanni", "Brianna", "Darren", "Tanya", "Calvin", "Alyssa" }, new List<string> { "Martinez", "Perez", "Castillo", "Musa", "Usher", "August" }) },
        { Country.Canada, new NameData(new List<string> { "Liam", "Olivia", "Noah", "Charlotte", "Jackson", "Amelia" }, new List<string> { "Tremblay", "Roy", "Gagnon", "Lee", "Smith", "Wilson" }) },
        { Country.CocosIslands, new NameData(new List<string> { "Mohammad", "Siti", "Hafiz", "Aisyah", "Azman", "Nur" }, new List<string> { "Clunies-Ross", "Macrae", "Anthoney", "Payne", "Cumpston", "Minkom" }) },
        { Country.DemocraticRepublicOfTheCongo, new NameData(new List<string> { "Dieudonné", "Grâce", "Ephraim", "Dorcas", "Glody", "Merveille" }, new List<string> { "Kabila", "Mukendi", "Tshisekedi", "Lumbu", "Mbuyi", "Kasonga" }) },
        { Country.CentralAfricanRepublic, new NameData(new List<string> { "Dieudonné", "Florence", "Sylvestre", "Chantal", "Rodrigue", "Prisca" }, new List<string> { "Bozizé", "Banda", "Touadéra", "Dacko", "Kambou", "Samba" }) },
        { Country.RepublicOfTheCongo, new NameData(new List<string> { "Brice", "Gael", "Merveille", "Marlyne", "Hervé", "Syntyche" }, new List<string> { "Sassou", "Matingou", "Ngouabi", "Okombi", "Makosso", "Moussa" }) },
        { Country.Switzerland, new NameData(new List<string> { "Noah", "Mia", "Liam", "Emma", "Matteo", "Elena" }, new List<string> { "Bachmann", "Staub", "Meier", "Rossi", "Müller", "Bernasconi" }) },
        { Country.IvoryCoast, new NameData(new List<string> { "Kouassi", "Awa", "Yao", "Aminata", "Koffi", "Fatou" }, new List<string> { "Kouadio", "Koné", "Traoré", "Bamba", "Coulibaly", "Diallo" }) },
        { Country.CookIslands, new NameData(new List<string> { "Tama", "Teura", "Moana", "Ina", "Aroha", "Mere" }, new List<string> { "Marsters", "Henry", "Browne", "Heather", "Rangi", "Wichman" }) },
        { Country.Chile, new NameData(new List<string> { "Mateo", "Sofia", "Benjamín", "Isabella", "Lucas", "Emilia" }, new List<string> { "Muñoz", "Rojas", "Díaz", "Pérez", "Soto", "Contreras" }) },
        { Country.Cameroon, new NameData(new List<string> { "Junior", "Brenda", "Franck", "Audrey", "Patrick", "Inès" }, new List<string> { "Mbappe", "Etoo", "Nkamhoua", "Atangana", "Biya", "Fongang" }) },
        { Country.China, new NameData(new List<string> { "Wei", "Xiu Ying", "Li Wei", "Ying", "Ming", "Jian" }, new List<string> { "Zhang", "Wang", "Chen", "Liu", "Yang", "Huang" }) },
        { Country.Colombia, new NameData(new List<string> { "Samuel", "Salomé", "Santiago", "Isabella", "Jerónimo", "Mariana" }, new List<string> { "Rodríguez", "Gómez", "Morales", "Jiménez", "García", "Martínez" }) },
        { Country.CostaRica, new NameData(new List<string> { "Gabriel", "Jimena", "Felipe", "Fiorella", "Andrés", "Camila" }, new List<string> { "Mora", "Rojas", "Jiménez", "Vargas", "Araya", "Castro" }) },
        { Country.Cuba, new NameData(new List<string> { "Alejandro", "Camila", "Yoan", "Lisandra", "Maikel", "Yanet" }, new List<string> { "Rodríguez", "Pérez", "González", "Hernández", "García", "Martínez" }) },
        { Country.CapeVerde, new NameData(new List<string> { "Gilson", "Neuza", "Edmilson", "Kenia", "Jair", "Catia" }, new List<string> { "Silva", "Semedo", "Furtado", "Tavares", "Lopes", "Pires" }) },
        { Country.Curacao, new NameData(new List<string> { "Dangelo", "Kiara", "Gianni", "Xenia", "Gerson", "Alyssa" }, new List<string> { "Martina", "Bernardus", "Gijsbertha", "Girigorie", "Mercera", "Pietersz" }) },
        { Country.ChristmasIsland, new NameData(new List<string> { "Wei", "Mei", "Ahmad", "Siti", "John", "Sarah" }, new List<string> { "Tan", "Lee", "Chung", "Kamal", "Smith", "Taylor" }) },
        { Country.Cyprus, new NameData(new List<string> { "Andreas", "Eleni", "Giorgos", "Maria", "Nikos", "Christina" }, new List<string> { "Georgiou", "Charalambous", "Constantinou", "Christodoulou", "Ioannou", "Andreou" }) },
        { Country.CzechRepublic, new NameData(new List<string> { "Jan", "Eliška", "Tomáš", "Adéla", "Jakub", "Anna" }, new List<string> { "Novák", "Svobodová", "Novotný", "Dvořáková", "Černý", "Procházka" }) },
        { Country.Germany, new NameData(new List<string> { "Maximilian", "Sophia", "Lukas", "Emma", "Jonas", "Hannah" }, new List<string> { "Müller", "Schmidt", "Schneider", "Fischer", "Weber", "Meyer" }) },
        { Country.Djibouti, new NameData(new List<string> { "Mousse", "Fatouma", "Aden", "Amina", "Hassan", "Khadra" }, new List<string> { "Farah", "Guelleh", "Ali", "Abdillahi", "Warsama", "Mohamed" }) },
        { Country.Denmark, new NameData(new List<string> { "William", "Ella", "Noah", "Freja", "Oscar", "Alma" }, new List<string> { "Nielsen", "Jensen", "Hansen", "Pedersen", "Andersen", "Christensen" }) },
        { Country.Dominica, new NameData(new List<string> { "Jaden", "Brianna", "Kareem", "Aaliyah", "Devon", "Tia" }, new List<string> { "Charles", "John", "Joseph", "Pierre", "Paul", "Williams" }) },
        { Country.DominicanRepublic, new NameData(new List<string> { "Kelvin", "Yocaira", "Jhonson", "Genesis", "Marlon", "Anyelina" }, new List<string> { "Rodríguez", "Pérez", "Sánchez", "Martínez", "Peña", "García" }) },
        { Country.Algeria, new NameData(new List<string> { "Mohamed", "Amel", "Yassine", "Meriem", "Khaled", "Ines" }, new List<string> { "Saidi", "Benali", "Brahimi", "Haddad", "Belkacem", "Khelifi" }) },
        { Country.Ecuador, new NameData(new List<string> { "Mateo", "Dome", "Thiago", "Mia", "Matías", "Valentina" }, new List<string> { "Zambra", "Torres", "Vera", "Sánchez", "Cedeño", "López" }) },
        { Country.Estonia, new NameData(new List<string> { "Robin", "Sofia", "Rasmus", "Mia", "Oliver", "Emma" }, new List<string> { "Tamm", "Saar", "Sepp", "Mägi", "Kask", "Kukk" }) },
        { Country.Egypt, new NameData(new List<string> { "Youssef", "Mariam", "Omar", "Nour", "Ahmed", "Fatma" }, new List<string> { "Mohamed", "Ahmed", "Mahmoud", "Hassan", "Ibrahim", "Ali" }) },
        { Country.WesternSahara, new NameData(new List<string> { "Brahim", "Salka", "El-Mami", "Laila", "Sidati", "Maimouna" }, new List<string> { "Salem", "Mohamed", "Brahim", "Ahmed", "Embarek", "Baba" }) },
        { Country.Eritrea, new NameData(new List<string> { "Yemane", "Luwam", "Mebrahtu", "Semhar", "Amaniel", "Helen" }, new List<string> { "Tesfay", "Gebrehiwet", "Tekle", "Berhane", "Haile", "Habte" }) },
        { Country.Spain, new NameData(new List<string> { "Hugo", "Lucía", "Mateo", "Sofía", "Martín", "María" }, new List<string> { "García", "Rodríguez", "González", "Fernández", "López", "Martínez" }) },
        { Country.Ethiopia, new NameData(new List<string> { "Abebe", "Tigist", "Dawit", "Hiwot", "Yohannes", "Mazen" }, new List<string> { "Tekle", "Bekele", "Tadesse", "Alemu", "Worku", "Kebede" }) },
        { Country.Finland, new NameData(new List<string> { "Eino", "Aino", "Oliver", "Venla", "Väinö", "Sofia" }, new List<string> { "Korhonen", "Virtanen", "Mäkinen", "Nieminen", "Mäkelä", "Hämäläinen" }) },
        { Country.Fiji, new NameData(new List<string> { "Jone", "Mere", "Sisa", "Litiana", "Filimoni", "Vinaina" }, new List<string> { "Bainimarama", "Prasad", "Chand", "Tawake", "Lal", "Nakarawa" }) },
        { Country.FalklandIslands, new NameData(new List<string> { "Oliver", "Emily", "Jack", "Chloe", "James", "Sophie" }, new List<string> { "Short", "McPhee", "Miller", "Peck", "Clarke", "Binnie" }) },
        { Country.Micronesia, new NameData(new List<string> { "Kioshi", "Kezia", "Junior", "Maylani", "Kenis", "Ruth" }, new List<string> { "Mori", "Christian", "Peter", "Panuelo", "Figir", "Fritz" }) },
        { Country.FaroeIslands, new NameData(new List<string> { "Bárður", "Sára", "Jónas", "Elin", "Hanus", "Lív" }, new List<string> { "Hansen", "Johansen", "Jacobsen", "Olsen", "Petersen", "Joensen" }) },
        { Country.France, new NameData(new List<string> { "Gabriel", "Louise", "Léo", "Jade", "Raphaël", "Alice" }, new List<string> { "Martin", "Bernard", "Dubois", "Thomas", "Robert", "Richard" }) },
        { Country.Gabon, new NameData(new List<string> { "Brice", "Inès", "Franck", "Audrey", "Gael", "Merveille" }, new List<string> { "Bongo", "Ondo", "Obame", "Mba", "Nguema", "Kombila" }) },
        { Country.UnitedKingdom, new NameData(new List<string> { "George", "Amelia", "Harry", "Isla", "Oliver", "Ava" }, new List<string> { "Smith", "Jones", "Taylor", "Williams", "Brown", "Davies" }) },
        { Country.Grenada, new NameData(new List<string> { "Kareem", "Aaliyah", "Devon", "Tia", "Malik", "Chloe" }, new List<string> { "Mitchell", "Thomas", "Charles", "Joseph", "Peters", "Noel" }) },
        { Country.Georgia, new NameData(new List<string> { "Giorgi", "Nino", "Luka", "Mariam", "Davit", "Ana" }, new List<string> { "Beridze", "Kapanadze", "Gelašvili", "Maisuradze", "Gorgadze", "Shengelia" }) },
        { Country.FrenchGuiana, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Jean-Pierre", "Alexandre", "Prevot", "Serrano", "Mendoza", "Felix" }) },
        { Country.Guernsey, new NameData(new List<string> { "George", "Amelia", "Harry", "Isla", "Oliver", "Ava" }, new List<string> { "Le Page", "De La Mare", "Falla", "Martel", "Le Poidevin", "Girard" }) },
        { Country.Ghana, new NameData(new List<string> { "Kwame", "Ama", "Kofi", "Akosua", "Yaw", "Yaa" }, new List<string> { "Mensah", "Osei", "Appiah", "Agyei", "Owusu", "Boateng" }) },
        { Country.Gibraltar, new NameData(new List<string> { "Liam", "Chloe", "James", "Sophie", "Daniel", "Emily" }, new List<string> { "Picardo", "Garcia", "Isola", "Canepa", "Vernet", "Bruzon" }) },
        { Country.Greenland, new NameData(new List<string> { "Malik", "Pipaluk", "Inuk", "Nivi", "Minik", "Ivaana" }, new List<string> { "Jensen", "Nielsen", "Olsen", "Petersen", "Møller", "Hansen" }) },
        { Country.Gambia, new NameData(new List<string> { "Ebrima", "Fatou", "Lamin", "Mariama", "Bakary", "Isatou" }, new List<string> { "Jallow", "Bah", "Ceesay", "Touray", "Camara", "Sanyang" }) },
        { Country.Guinea, new NameData(new List<string> { "Mamadou", "Aissatou", "Ibrahima", "Fatoumata", "Alpha", "Mariama" }, new List<string> { "Diallo", "Camara", "Bah", "Barry", "Soumah", "Conde" }) },
        { Country.Guadeloupe, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Joseph", "Celestine", "Gustave", "Borel", "Clavier", "Lubin" }) },
        { Country.EquatorialGuinea, new NameData(new List<string> { "Teodoro", "Constancia", "Ruslan", "Eulalia", "Diosdado", "Manuela" }, new List<string> { "Obiang", "Nguema", "Mba", "Ondo", "Nsue", "Esono" }) },
        { Country.Greece, new NameData(new List<string> { "Giorgos", "Maria", "Dimitris", "Eleni", "Kostas", "Aikaterini" }, new List<string> { "Papadopoulos", "Papadakis", "Ioannidis", "Georgiou", "Oikonomou", "Nikolaidis" }) },
        { Country.SouthGeorgiaAndSouthSandwichIslands, new NameData(new List<string> { "James", "Sarah", "William", "Emma", "Thomas", "Hannah" }, new List<string> { "Cook", "Larson", "Smith", "Wilson", "Bell", "Davis" }) },
        { Country.Guatemala, new NameData(new List<string> { "Carlos", "Sofia", "Diego", "María", "José", "Lucía" }, new List<string> { "López", "García", "Morales", "Hernández", "Pérez", "Gómez" }) },
        { Country.Guam, new NameData(new List<string> { "Brandon", "Ha'ani", "Jesse", "Kalea", "Justin", "Mariah" }, new List<string> { "Camacho", "Santos", "Cruz", "Quidachay", "San Nicolas", "Guerrero" }) },
        { Country.GuineaBissau, new NameData(new List<string> { "Bacar", "Fatumata", "Adama", "Aissatu", "Malam", "Binta" }, new List<string> { "Gomez", "Silva", "Djalo", "Sanha", "Balde", "Pereira" }) },
        { Country.Guyana, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Persaud", "Singh", "Khan", "Thomas", "Joseph", "Williams" }) },
        { Country.HongKong, new NameData(new List<string> { "Ka-Hao", "Wing-Yan", "Chun-Yin", "Hiu-Tung", "Lok-Yin", "Sze-Man" }, new List<string> { "Chan", "Wong", "Lee", "Cheung", "Lau", "Leung" }) },
        { Country.HeardIslandAndMcDonaldIslands, new NameData(new List<string> { "Jack", "Olivia", "Oliver", "Charlotte", "Henry", "Amelia" }, new List<string> { "Heard", "McDonald", "Smith", "Jones", "Taylor", "Brown" }) },
        { Country.Honduras, new NameData(new List<string> { "Carlos", "Sofia", "Diego", "María", "José", "Lucía" }, new List<string> { "Hernández", "López", "Martínez", "Rodríguez", "García", "Mejía" }) },
        { Country.Croatia, new NameData(new List<string> { "Luka", "Mia", "David", "Lucija", "Jakov", "Ema" }, new List<string> { "Horvat", "Kovačević", "Babić", "Marić", "Jurić", "Novak" }) },
        { Country.Haiti, new NameData(new List<string> { "Jean", "Florence", "Pierre", "Mirlande", "Michel", "Esther" }, new List<string> { "Pierre", "Jean", "Joseph", "Charles", "Louis", "Paul" }) },
        { Country.Hungary, new NameData(new List<string> { "Bence", "Hanna", "Máté", "Anna", "Levente", "Lili" }, new List<string> { "Nagy", "Kovács", "Tóth", "Szabó", "Horváth", "Varga" }) },
        { Country.Indonesia, new NameData(new List<string> { "Budi", "Siti", "Agus", "Dewi", "Rizky", "Nur" }, new List<string> { "Santoso", "Wijaya", "Lestari", "Hidayat", "Pratama", "Kusuma" }) },
        { Country.Ireland, new NameData(new List<string> { "Jack", "Grace", "Conor", "Aoife", "Daniel", "Emily" }, new List<string> { "Murphy", "Kelly", "O'Brien", "Walsh", "Byrne", "Ryan" }) },
        { Country.Israel, new NameData(new List<string> { "David", "Tamar", "Ariel", "Noa", "Lavi", "Maya" }, new List<string> { "Cohen", "Levi", "Mizrahi", "Peretz", "Biton", "Dahan" }) },
        { Country.IsleOfMan, new NameData(new List<string> { "Liam", "Chloe", "James", "Sophie", "Daniel", "Emily" }, new List<string> { "Kelly", "Corlett", "Craine", "Ququay", "Teare", "Cannell" }) },
        { Country.India, new NameData(new List<string> { "Aarav", "Ananya", "Vivaan", "Dhiya", "Reyansh", "Saanvi" }, new List<string> { "Sharma", "Patel", "Kumar", "Singh", "Verma", "Gupta" }) },
        { Country.BritishIndianOceanTerritory, new NameData(new List<string> { "James", "Sarah", "William", "Emma", "Thomas", "Hannah" }, new List<string> { "Smith", "Jones", "Taylor", "Williams", "Brown", "Davies" }) },
        { Country.Iraq, new NameData(new List<string> { "Ali", "Zahra", "Hussein", "Fatima", "Omar", "Maryam" }, new List<string> { "Al-Musa", "Al-Maliki", "Al-Saadi", "Al-Abadi", "Al-Janabi", "Al-Tamimi" }) },
        { Country.Iran, new NameData(new List<string> { "Ali", "Zahra", "Mohammad", "Fatemeh", "Amir", "Maryam" }, new List<string> { "Hosseini", "Mohammadi", "Rezai", "Ahmadi", "Ghasemi", "Mousavi" }) },
        { Country.Iceland, new NameData(new List<string> { "Aron", "Embla", "Alexander", "Sara", "Viktor", "Katrín" }, new List<string> { "Jónsson", "Sigurðardóttir", "Gunnarsson", "Guðmundsdóttir", "Einarsson", "Jónsdóttir" }) },
        { Country.Italy, new NameData(new List<string> { "Leonardo", "Sofia", "Francesco", "Giulia", "Alessandro", "Aurora" }, new List<string> { "Rossi", "Russo", "Ferrari", "Esposito", "Bianchi", "Romano" }) },
        { Country.Jersey, new NameData(new List<string> { "Liam", "Chloe", "James", "Sophie", "Daniel", "Emily" }, new List<string> { "Le Quesne", "De Gruchy", "Le Feuvre", "Gallichan", "Baudains", "Hamming" }) },
        { Country.Jamaica, new NameData(new List<string> { "Tariq", "Shanique", "Rohan", "Tanya", "Usain", "Nia" }, new List<string> { "Campbell", "Brown", "Williams", "Thompson", "Johnson", "Gordon" }) },
        { Country.Jordan, new NameData(new List<string> { "Omar", "Rania", "Ahmad", "Salma", "Youssef", "Farah" }, new List<string> { "Al-Majali", "Al-Tarawneh", "Al-Fayez", "Al-Zu'bi", "Al-Nsour", "Al-Obeidat" }) },
        { Country.Japan, new NameData(new List<string> { "Ren", "Aoi", "Yuto", "Hana", "Kenji", "Yui" }, new List<string> { "Sato", "Suzuki", "Takahashi", "Tanaka", "Watanabe", "Ito" }) },
        { Country.Kenya, new NameData(new List<string> { "Brian", "Faith", "Emmanuel", "Mercy", "Victor", "Joy" }, new List<string> { "Mwangi", "Maina", "Kamau", "Kipkorir", "Ochieng", "Njoroge" }) },
        { Country.Kyrgyzstan, new NameData(new List<string> { "Azamat", "Aida", "Bekzod", "Madina", "Ulan", "Aizhan" }, new List<string> { "Asanov", "Jusupov", "Ismailov", "Sydykov", "Ibraimov", "Nurbekov" }) },
        { Country.Cambodia, new NameData(new List<string> { "Sokha", "Bopha", "Vannak", "Srey", "Chanthou", "Kalyan" }, new List<string> { "Sok", "Chan", "Heng", "Chea", "Keo", "Meng" }) },
        { Country.Kiribati, new NameData(new List<string> { "Teburoro", "Kiri", "Ioteba", "Tekee", "Bikenibeu", "Mere" }, new List<string> { "Tong", "Tito", "Tabai", "Taneti", "Uriam", "Teannaki" }) },
        { Country.Comoros, new NameData(new List<string> { "Said", "Amina", "Ahmed", "Fatima", "Mou administrative", "Zalata" }, new List<string> { "Mohamed", "Ali", "Ahamada", "Said", "Soilihi", "Youssouf" }) },
        { Country.SaintKittsAndNevis, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Liburd", "Nisbett", "Powell", "Williams", "Morton", "Brand" }) },
        { Country.NorthKorea, new NameData(new List<string> { "Min-jun", "Seo-yun", "Do-yun", "Ha-eun", "Seojun", "Ji-woo" }, new List<string> { "Kim", "Pak", "Ri", "Choe", "Jang", "An" }) },
        { Country.SouthKorea, new NameData(new List<string> { "Min-jun", "Seo-yun", "Do-yun", "Ha-eun", "Seojun", "Ji-woo" }, new List<string> { "Kim", "Lee", "Park", "Choi", "Jung", "Kang" }) },
        { Country.Kuwait, new NameData(new List<string> { "Fahad", "Noura", "Youssef", "Sarah", "Bader", "Dana" }, new List<string> { "Al-Sabah", "Al-Enezi", "Al-Mutairi", "Al-Ajmi", "Al-Rashedi", "Al-Fadhli" }) },
        { Country.CaymanIslands, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Ebanks", "Bodden", "Miller", "Bush", "McLaughlin", "Rivers" }) },
        { Country.Kazakhstan, new NameData(new List<string> { "Alihan", "Madina", "Islam", "Aruzhan", "Nursultan", "Ayala" }, new List<string> { "Nurmagambetov", "Saduakasova", "Omarov", "Bolatova", "Zhakupov", "Kim" }) },
        { Country.Laos, new NameData(new List<string> { "Somchai", "Champa", "Thongloune", "Khamkeo", "Bounmy", "Malee" }, new List<string> { "Phomvihane", "Sisalem", "Vongxay", "Inthavong", "Sengchanh", "Phanthavong" }) },
        { Country.Lebanon, new NameData(new List<string> { "Charbel", "Nour", "Jad", "Yasmine", "Elie", "Lea" }, new List<string> { "Khoury", "Haddad", "Ghanem", "Aoun", "Hariri", "Sarkis" }) },
        { Country.SaintLucia, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Joseph", "Charles", "Pierre", "Emmanuel", "James", "Jn Baptiste" }) },
        { Country.Liechtenstein, new NameData(new List<string> { "Maximilian", "Sophia", "Lukas", "Emma", "Jonas", "Hannah" }, new List<string> { "Büchel", "Frick", "Gasser", "Marxer", "Vogt", "Wohlwend" }) },
        { Country.SriLanka, new NameData(new List<string> { "Pathum", "Kavindi", "Kasun", "Nipuni", "Nuwan", "Dilini" }, new List<string> { "Perera", "Fernando", "Silva", "Bandara", "Jayasinghe", "De Silva" }) },
        { Country.Liberia, new NameData(new List<string> { "Emmanuel", "Blessing", "Moses", "Princess", "Junior", "Grace" }, new List<string> { "Weah", "Kollie", "Flomo", "Sirleaf", "Kamara", "Taylor" }) },
        { Country.Lesotho, new NameData(new List<string> { "Teboho", "Mpho", "Thabo", "Litseho", "Keketso", "Palesa" }, new List<string> { "Mofokeng", "Molapo", "Moshoeshoe", "Phoofolo", "Sekhonyana", "Maseribane" }) },
        { Country.Lithuania, new NameData(new List<string> { "Lukas", "Ieva", "Matas", "Gabija", "Nojus", "Emilija" }, new List<string> { "Kazlauskas", "Jankauskas", "Petrauskas", "Stankevičius", "Vasiliauskas", "Žukauskas" }) },
        { Country.Luxembourg, new NameData(new List<string> { "Luka", "Emma", "Leo", "Mia", "Louis", "Sophie" }, new List<string> { "Schmit", "Weber", "Muller", "Hoffmann", "Wagner", "Thill" }) },
        { Country.Latvia, new NameData(new List<string> { "Roberts", "Sofija", "Oskars", "Marta", "Markuss", "Emīlija" }, new List<string> { "Bērziņš", "Kalniņš", "Ozoliņš", "Jansons", "Ozols", "Liepiņš" }) },
        { Country.Libya, new NameData(new List<string> { "Mohamed", "Fatima", "Ahmed", "Aisha", "Omar", "Maryam" }, new List<string> { "Al-Gaddafi", "Al-Senussi", "Al-Boraesh", "Al-Fitouri", "Al-Warfali", "Al-Tarhouni" }) },
        { Country.Morocco, new NameData(new List<string> { "Amine", "Salma", "Youssef", "Khadija", "Omar", "Imane" }, new List<string> { "Alaoui", "Benjelloun", "El-Amrani", "El-Idrissi", "Chraibi", "Berrada" }) },
        { Country.Monaco, new NameData(new List<string> { "Louis", "Charlotte", "Antoine", "Camille", "Julien", "Océane" }, new List<string> { "Grimaldi", "Pastor", "Crovetto", "Marquet", "Gastaldi", "Lorenzi" }) },
        { Country.Moldova, new NameData(new List<string> { "Ion", "Ana", "Andrei", "Elena", "Dumitru", "Maria" }, new List<string> { "Rusu", "Ceban", "Cojocaru", "Munteanu", "Popa", "Sirbu" }) },
        { Country.Montenegro, new NameData(new List<string> { "Luka", "Milica", "Marko", "Anja", "Nikola", "Sara" }, new List<string> { "Popović", "Marković", "Radulović", "Kovačević", "Ivanović", "Vuković" }) },
        { Country.SaintMartin, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Richardson", "Fleming", "Gumbs", "Arnell", "Webster", "Hodge" }) },
        { Country.Madagascar, new NameData(new List<string> { "Andry", "Mialy", "Tojo", "Fitia", "Hery", "Tsoa" }, new List<string> { "Rakotoarisoa", "Ranaivo", "Randrianarisoa", "Razafindrakoto", "Rasolofo", "Andriamahatana" }) },
        { Country.MarshallIslands, new NameData(new List<string> { "Kessai", "Lani", "Litokwa", "Miriam", "Jurelang", "Mera" }, new List<string> { "Kabuai", "Note", "Heine", "Zedkaia", "DeBrum", "Loeak" }) },
        { Country.NorthMacedonia, new NameData(new List<string> { "Luka", "Jana", "Marko", "Marija", "Filip", "Ana" }, new List<string> { "Stojanovski", "Nikolov", "Trajkovski", "Petrovski", "Ivanov", "Ristevski" }) },
        { Country.Mali, new NameData(new List<string> { "Mamadou", "Fatoumata", "Ibrahima", "Aminata", "Oumar", "Awa" }, new List<string> { "Keita", "Coulibaly", "Traoré", "Diakité", "Sidibé", "Maïga" }) },
        { Country.Myanmar, new NameData(new List<string> { "Aung", "Thida", "Kyaw", "May", "Min", "Su" }, new List<string> { "San", "Oo", "Myint", "Tun", "Win", "Lwin" }) },
        { Country.Mongolia, new NameData(new List<string> { "Bat-Erdene", "Khulan", "Temuulen", "Nominerdene", "Anand", "Enkhjin" }, new List<string> { "Enkhbold", "Batbold", "Elbegdorj", "Battulga", "Bayar", "Gankhuyag" }) },
        { Country.Macao, new NameData(new List<string> { "Ka-Hao", "Wing-Yan", "Chun-Yin", "Hiu-Tung", "Lok-Yin", "Sze-Man" }, new List<string> { "Ho", "Leong", "Tam", "Sousa", "Chao", "Pang" }) },
        { Country.NorthernMarianaIslands, new NameData(new List<string> { "Brandon", "Ha'ani", "Jesse", "Kalea", "Justin", "Mariah" }, new List<string> { "Sablan", "Torres", "Camacho", "Palacios", "Guerrero", "Deleon Guerrero" }) },
        { Country.Martinique, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Marie-Luce", "Joseph", "Jean-Baptiste", "Nicolet", "Césaire", "Laguerre" }) },
        { Country.Mauritania, new NameData(new List<string> { "Mohamed", "Mariem", "Ahmed", "Fatimetou", "Sidi", "Aichetou" }, new List<string> { "Ould", "Mint", "Abeid", "Haidara", "Sall", "Kane" }) },
        { Country.Montserrat, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Osborne", "Bramble", "Ryan", "Meade", "Allen", "Cassell" }) },
        { Country.Malta, new NameData(new List<string> { "Luca", "Emma", "Liam", "Maya", "Noah", "Elena" }, new List<string> { "Borg", "Camilleri", "Vella", "Farrugia", "Zammit", "Galea" }) },
        { Country.Mauritius, new NameData(new List<string> { "Kevin", "Anusha", "Ketan", "Pooja", "Jason", "Jessica" }, new List<string> { "Seegobin", "Ramgoolam", "Jugnauth", "Curpen", "Appadoo", "Beeharry" }) },
        { Country.Maldives, new NameData(new List<string> { "Mohamed", "Aishath", "Ahmed", "Mariyam", "Ali", "Fatimat" }, new List<string> { "Nasheed", "Gayoom", "Waheed", "Solih", "Didi", "Zahir" }) },
        { Country.Malawi, new NameData(new List<string> { "Chisomo", "Thoko", "Limbani", "Tione", "Tiyamike", "Mwayi" }, new List<string> { "Banda", "Phiri", "Moyo", "Tembo", "Mwenefumbo", "Chirwa" }) },
        { Country.Mexico, new NameData(new List<string> { "Santiago", "Sofia", "Mateo", "Valentina", "Sebastián", "Regina" }, new List<string> { "Hernández", "García", "Martínez", "López", "González", "Pérez" }) },
        { Country.Malaysia, new NameData(new List<string> { "Muhammad", "Nurul", "Ahmad", "Siti", "Daniel", "Aina" }, new List<string> { "Lee", "Tan", "Ismail", "Abdullah", "Wong", "Chong" }) },
        { Country.Mozambique, new NameData(new List<string> { "Mateus", "Esperança", "João", "Ana", "Manuel", "Ndalu" }, new List<string> { "Machel", "Chissano", "Gebuza", "Nyusi", "Mondlane", "Couto" }) },
        { Country.Namibia, new NameData(new List<string> { "Jonas", "Linekela", "Tugela", "Ndapewa", "Kavena", "Penehafo" }, new List<string> { "Shaningwa", "Geingob", "Nujoma", "Pohamba", "Tjongarero", "Katjavivi" }) },
        { Country.NewCaledonia, new NameData(new List<string> { "Antoine", "Wamytan", "Julien", "Kiam", "Mathieu", "Marie" }, new List<string> { "Tjibaou", "Mapou", "Lalié", "Frogier", "Backes", "Gomès" }) },
        { Country.Niger, new NameData(new List<string> { "Ibrahim", "Aichatou", "Moussa", "Fatouma", "Abdoulaye", "Hadiza" }, new List<string> { "Issoufou", "Tiani", "Hamani", "Seyni", "Oumarou", "Kountché" }) },
        { Country.NorfolkIsland, new NameData(new List<string> { "Oliver", "Charlotte", "Noah", "Mia", "William", "Isla" }, new List<string> { "Nobbs", "Buffett", "Christian", "Quintal", "Adams", "Evans" }) },
        { Country.Nigeria, new NameData(new List<string> { "Chidubem", "Aminat", "Emeka", "Zainab", "Oluwaseun", "Fatima" }, new List<string> { "Okafor", "Adebayo", "Nwosu", "Abubakar", "Balogun", "Eke" }) },
        { Country.Nicaragua, new NameData(new List<string> { "Carlos", "Sofia", "Diego", "María", "José", "Lucía" }, new List<string> { "López", "García", "Martínez", "González", "Hernández", "Pérez" }) },
        { Country.Netherlands, new NameData(new List<string> { "Daan", "Emma", "Noah", "Sophie", "Lucas", "Julia" }, new List<string> { "de Jong", "Jansen", "de Vries", "van de Berg", "Bakker", "van Dijk" }) },
        { Country.Norway, new NameData(new List<string> { "Oliver", "Nora", "Jakob", "Astrid", "Emil", "Emma" }, new List<string> { "Hansen", "Johansen", "Olsen", "Larsen", "Berg", "Haugen" }) },
        { Country.Nepal, new NameData(new List<string> { "Aarav", "Aarya", "Bibek", "Pooja", "Rohan", "Saraswati" }, new List<string> { "Shrestha", "Thapa", "Gurung", "Tamang", "Sharma", "Bhatt" }) },
        { Country.Nauru, new NameData(new List<string> { "Kieren", "Eboni", "Landon", "Ruth", "Kennan", "Charmaine" }, new List<string> { "Deireragea", "Waqa", "Adeang", "Dowiyogo", "Harris", "Clodumar" }) },
        { Country.Niue, new NameData(new List<string> { "Togia", "Siniva", "Fapu", "Sione", "Malia", "Apolo" }, new List<string> { "Tagelagi", "Talagi", "Rex", "Lakatani", "Jackson", "Viviani" }) },
        { Country.NewZealand, new NameData(new List<string> { "Oliver", "Isla", "Noah", "Charlotte", "Leo", "Amelia" }, new List<string> { "Smith", "Jones", "Williams", "Brown", "Wilson", "Taylor" }) },
        { Country.Oman, new NameData(new List<string> { "Sultan", "Fatma", "Haitham", "Maryam", "Qaboos", "Aisha" }, new List<string> { "Al-Said", "Al-Busaidi", "Al-Harthy", "Al-Balushi", "Al-Maamari", "Al-Lawati" }) },
        { Country.Panama, new NameData(new List<string> { "Mateo", "Sofia", "Thiago", "Valentina", "Joaquín", "Luciana" }, new List<string> { "Rodríguez", "González", "García", "Martínez", "López", "Pérez" }) },
        { Country.Peru, new NameData(new List<string> { "Thiago", "Mia", "Liam", "Luana", "Gael", "Camila" }, new List<string> { "Quispe", "Flores", "Sánchez", "Rodríguez", "García", "Rojas" }) },
        { Country.FrenchPolynesia, new NameData(new List<string> { "Moana", "Hina", "Teiva", "Poe", "Manoa", "Vaea" }, new List<string> { "Temaru", "Flosse", "Tong Sang", "Fritch", "Brotherson", "Teai" }) },
        { Country.PapuaNewGuinea, new NameData(new List<string> { "John", "Mary", "Peter", "Grace", "David", "Ruth" }, new List<string> { "Marape", "O'Neill", "Somare", "Wingti", "Chan", "Namaliu" }) },
        { Country.Philippines, new NameData(new List<string> { "Juan", "Maria", "Mark", "Angela", "Gabriel", "Samantha" }, new List<string> { "Dela Cruz", "Santos", "Reyes", "Cruz", "Bautista", "Ocampo" }) },
        { Country.Pakistan, new NameData(new List<string> { "Muhammad", "Fatima", "Ahmed", "Ayesha", "Ali", "Zainab" }, new List<string> { "Khan", "Malik", "Raza", "Ahmed", "Chaudhry", "Shah" }) },
        { Country.Poland, new NameData(new List<string> { "Antoni", "Zofia", "Jan", "Julia", "Aleksander", "Maja" }, new List<string> { "Nowak", "Kowalski", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński" }) },
        { Country.SaintPierreAndMiquelon, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Briand", "Disnard", "Artano", "Goraguer", "Claireaux", "Paturel" }) },
        { Country.Pitcairn, new NameData(new List<string> { "Christian", "Emily", "Warren", "Brenda", "Shawn", "Meralda" }, new List<string> { "Christian", "Warren", "Young", "Brown", "McKoy", "Coffin" }) },
        { Country.PuertoRico, new NameData(new List<string> { "Mateo", "Sofia", "Thiago", "Valentina", "Sebastián", "Victoria" }, new List<string> { "Rodríguez", "Rivera", "Ortiz", "Torres", "López", "González" }) },
        { Country.Palestine, new NameData(new List<string> { "Ahmad", "Fatima", "Mohammad", "Maryam", "Youssef", "Nour" }, new List<string> { "Al-Masri", "Al-Khatib", "Al-Ahmad", "Barghouti", "Tamimi", "Al-Husseini" }) },
        { Country.Portugal, new NameData(new List<string> { "Francisco", "Maria", "João", "Leonor", "Afonso", "Matilde" }, new List<string> { "Silva", "Santos", "Ferreira", "Pereira", "Oliveira", "Costa" }) },
        { Country.Palau, new NameData(new List<string> { "Surangel", "Keani", "Tommy", "Kesia", "Johnson", "Miriam" }, new List<string> { "Whipps", "Toribiong", "Remengesau", "Nakamura", "Koshiba", "Chin" }) },
        { Country.Paraguay, new NameData(new List<string> { "Mateo", "Sofia", "Thiago", "Valentina", "Joaquín", "Luciana" }, new List<string> { "González", "Benítez", "Martínez", "López", "Giménez", "Vera" }) },
        { Country.Qatar, new NameData(new List<string> { "Tamim", "Moza", "Hamad", "Maryam", "Jassim", "Fatima" }, new List<string> { "Al-Thani", "Al-Kuwari", "Al-Attiyah", "Al-Marri", "Al-Sulaiti", "Al-Hajri" }) },
        { Country.Reunion, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Payet", "Hoarau", "Grondin", "Lebon", "Fontaine", "Diallo" }) },
        { Country.Romania, new NameData(new List<string> { "Andrei", "Maria", "Stefan", "Elena", "Alexandru", "Ioana" }, new List<string> { "Popa", "Radu", "Dumitru", "Stan", "Stanciu", "Ionescu" }) },
        { Country.Serbia, new NameData(new List<string> { "Luka", "Milica", "Stefan", "Sofija", "Lazar", "Ana" }, new List<string> { "Jovanović", "Petrović", "Nikolić", "Ilić", "Đorđević", "Pavlović" }) },
        { Country.Russia, new NameData(new List<string> { "Alexander", "Sofia", "Dmitry", "Maria", "Maxim", "Anna" }, new List<string> { "Ivanov", "Smirnov", "Kuznetsov", "Popov", "Sokolov", "Lebedev" }) },
        { Country.Rwanda, new NameData(new List<string> { "Jean", "Aline", "Patrick", "Inès", "Eric", "Chantal" }, new List<string> { "Kagame", "Habimana", "Nkurunziza", "Uwase", "Mugisha", "Niyonsaba" }) },
        { Country.SaudiArabia, new NameData(new List<string> { "Mohammed", "Fatimah", "Omar", "Sara", "Abdullah", "Noura" }, new List<string> { "Al-Harbi", "Al-Ghamdi", "Al-Dawsari", "Al-Qahtani", "Al-Otaibi", "Al-Zahrani" }) },
        { Country.SolomonIslands, new NameData(new List<string> { "Manasseh", "Grace", "Gordon", "Joy", "Peter", "Ruth" }, new List<string> { "Sogavare", "Houenipwela", "Kenilorea", "Lilo", "Kemakeza", "Kekea" }) },
        { Country.Seychelles, new NameData(new List<string> { "Wavel", "Chantal", "Danny", "Inès", "Patrick", "Audrey" }, new List<string> { "Ramkalawan", "Faure", "Michel", "Rene", "Mancham", "Adam" }) },
        { Country.Sudan, new NameData(new List<string> { "Mohamed", "Fatima", "Ahmed", "Aisha", "Omar", "Maryam" }, new List<string> { "Al-Burhan", "Dagalo", "Hamdok", "Al-Bashir", "Mahdi", "Mirghani" }) },
        { Country.Sweden, new NameData(new List<string> { "Lucas", "Alice", "Liam", "Maja", "William", "Vera" }, new List<string> { "Andersson", "Johansson", "Karlsson", "Nilsson", "Eriksson", "Larsson" }) },
        { Country.Singapore, new NameData(new List<string> { "Ryan", "Chloe", "Ethan", "Rachel", "Jayden", "Megan" }, new List<string> { "Tan", "Lee", "Lim", "Ng", "Wong", "Ong" }) },
        { Country.SaintHelena, new NameData(new List<string> { "Oliver", "Emily", "Jack", "Chloe", "James", "Sophie" }, new List<string> { "Henry", "Thomas", "Benjamin", "Yon", "George", "Stevens" }) },
        { Country.Slovenia, new NameData(new List<string> { "Luka", "Ema", "Filip", "Zala", "Nik", "Mia" }, new List<string> { "Novak", "Horvat", "Krajnc", "Kovačič", "Zupančič", "Potočnik" }) },
        { Country.SvalbardAndJanMayen, new NameData(new List<string> { "Lars", "Astrid", "Sverre", "Freja", "Magnus", "Ingrid" }, new List<string> { "Amundsen", "Nansen", "Christensen", "Hansen", "Olsen", "Larsen" }) },
        { Country.Slovakia, new NameData(new List<string> { "Jakub", "Ema", "Adam", "Sofia", "Michal", "Nina" }, new List<string> { "Varga", "Tóth", "Nagy", "Baláž", "Szabó", "Molnár" }) },
        { Country.SierraLeone, new NameData(new List<string> { "Julius", "Amina", "Kabu", "Fatmata", "Sahr", "Zainab" }, new List<string> { "Bio", "Koroma", "Kamara", "Bangura", "Kargbo", "Sesay" }) },
        { Country.SanMarino, new NameData(new List<string> { "Luca", "Sofia", "Alessandro", "Giulia", "Matteo", "Aurora" }, new List<string> { "Valentini", "Rossi", "Cecchetti", "Giacomini", "Belluzzi", "Felici" }) },
        { Country.Senegal, new NameData(new List<string> { "Mamadou", "Fatou", "Ibrahima", "Aminata", "Ousmane", "Awa" }, new List<string> { "Diop", "Ndiaye", "Sow", "Fall", "Faye", "Gaye" }) },
        { Country.Somalia, new NameData(new List<string> { "Hassan", "Fartuun", "Abdi", "Amina", "Mohamed", "Farhiya" }, new List<string> { "Mohamud", "Abdi", "Farah", "Ali", "Hassan", "Jama" }) },
        { Country.Suriname, new NameData(new List<string> { "Darryl", "Shareen", "Sergio", "Anushka", "Gino", "Xaviera" }, new List<string> { "Santokhi", "Bouterse", "Brunswijk", "Adhin", "Pollack", "Emanuels" }) },
        { Country.SouthSudan, new NameData(new List<string> { "Salva", "Nyaguoa", "Riek", "Achol", "Deng", "Nyandeng" }, new List<string> { "Kiir", "Machar", "Deng", "Kuol", "Lagu", "Garang" }) },
        { Country.SaoTomeAndPrincipe, new NameData(new List<string> { "Carlos", "Nieves", "Manuel", "Esperança", "João", "Ana" }, new List<string> { "Trovoada", "Pinto", "da Silva", "Neves", "Carvalho", "Sousa" }) },
        { Country.ElSalvador, new NameData(new List<string> { "Nayib", "Sofia", "Mateo", "Valentina", "Sebastián", "Camila" }, new List<string> { "Bukele", "Hernández", "García", "Martínez", "López", "González" }) },
        { Country.SintMaarten, new NameData(new List<string> { "Dangelo", "Kiara", "Gianni", "Xenia", "Gerson", "Alyssa" }, new List<string> { "Marlin", "Heyliger", "Richardson", "Arrindell", "Jacobs", "Geerlings" }) },
        { Country.Syria, new NameData(new List<string> { "Omar", "Aisha", "Ahmed", "Fatima", "Youssef", "Nour" }, new List<string> { "Al-Assad", "Al-Khatib", "Al-Masri", "Al-Ahmad", "Al-Hassan", "Al-Hussein" }) },
        { Country.Eswatini, new NameData(new List<string> { "Mswati", "Nokuthula", "Sibusiso", "Temaswati", "Bheki", "Nomcebo" }, new List<string> { "Dlamini", "Shongwe", "Mamba", "Nxumalo", "Fakudze", "Tsabedze" }) },
        { Country.TurksAndCaicosIslands, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Misick", "Ewing", "Robinson", "Skippings", "Butterfield", "Forbes" }) },
        { Country.Chad, new NameData(new List<string> { "Mahamat", "Amina", "Idriss", "Fatouma", "Abakar", "Khadidja" }, new List<string> { "Déby", "Habré", "Tombalbaye", "Koumakoye", "Yoadimnadji", "Nadingar" }) },
        { Country.FrenchSouthernTerritories, new NameData(new List<string> { "Antoine", "Chloé", "Mathieu", "Océane", "Julien", "Camille" }, new List<string> { "Martin", "Bernard", "Dubois", "Thomas", "Robert", "Richard" }) },
        { Country.Togo, new NameData(new List<string> { "Koffi", "Awa", "Yao", "Aminata", "Komi", "Fatou" }, new List<string> { "Gnassingbé", "Olympio", "Eyadéma", "Adedze", "Klassou", "Dogbé" }) },
        { Country.Thailand, new NameData(new List<string> { "Somchai", "Mali", "Kittisak", "Anong", "Niran", "Siriporn" }, new List<string> { "Saetang", "Suwannarat", "Phungphon", "Wongsuwan", "Chaiyasing", "Saelim" }) },
        { Country.Tajikistan, new NameData(new List<string> { "Emomali", "Tahmina", "Rustam", "Farzona", "Somon", "Nigina" }, new List<string> { "Rahmon", "Rasulzoda", "Nazarov", "Sharipov", "Karimov", "Saidov" }) },
        { Country.Tokelau, new NameData(new List<string> { "Kuresa", "Lina", "Alesana", "Malia", "Eroni", "Salesa" }, new List<string> { "Nasau", "Ulu", "Perez", "Gaualofa", "Toloa", "Patelesio" }) },
        { Country.TimorLeste, new NameData(new List<string> { "Xanana", "Bella", "Taur", "Inês", "José", "Lucia" }, new List<string> { "Gusmão", "Matan Ruak", "Ramos-Horta", "Alkatiri", "da Silva", "Ximenes" }) },
        { Country.Turkmenistan, new NameData(new List<string> { "Serdar", "Gurbanguly", "Merdan", "Ayna", "Begench", "Gozel" }, new List<string> { "Berdimuhamedov", "Niyazov", "Meredov", "Atayev", "Durdyyev", "Orazov" }) },
        { Country.Tunisia, new NameData(new List<string> { "Youssef", "Amel", "Mohamed", "Meriem", "Khaled", "Ines" }, new List<string> { "Trabelsi", "Ben Ali", "Ghannouchi", "Chahed", "Saied", "Essid" }) },
        { Country.Tonga, new NameData(new List<string> { "Sione", "Sia", "Tevita", "Mele", "Aisea", "Anaseini" }, new List<string> { "Tupou", "Pohiva", "Tu'i'onetoa", "Sevele", "Palu", "Kovalenko" }) },
        { Country.Turkey, new NameData(new List<string> { "Yusuf", "Zeynep", "Eren", "Elif", "Miraç", "Defne" }, new List<string> { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız" }) },
        { Country.TrinidadAndTobago, new NameData(new List<string> { "Kareem", "Aaliyah", "Devon", "Tia", "Malik", "Chloe" }, new List<string> { "Maharaj", "Mohammed", "Persad", "Williams", "Charles", "Baptiste" }) },
        { Country.Tuvalu, new NameData(new List<string> { "Kausea", "Talia", "Enele", "Mere", "Willy", "Litiana" }, new List<string> { "Natano", "Sopoaga", "Telavi", "Toafa", "Talake", "Ielemia" }) },
        { Country.Taiwan, new NameData(new List<string> { "Po-Chun", "Ting-Yu", "Chia-Hao", "Ya-Ting", "Kuan-Yu", "Yu-Ting" }, new List<string> { "Chen", "Lin", "Huang", "Chang", "Li", "Wang" }) },
        { Country.Tanzania, new NameData(new List<string> { "Juma", "Rehema", "Baraka", "Amina", "Emanuel", "Neema" }, new List<string> { "Magufuli", "Hassan", "Kikwete", "Mkapa", "Mwinyi", "Nyerere" }) },
        { Country.Ukraine, new NameData(new List<string> { "Artem", "Sofia", "Maxim", "Yeva", "Bogdan", "Solomiia" }, new List<string> { "Melnyk", "Shevchenko", "Boyko", "Kovalenko", "Bondarenko", "Tkachenko" }) },
        { Country.Uganda, new NameData(new List<string> { "Kiprotich", "Patience", "Moses", "Mercy", "Brian", "Faith" }, new List<string> { "Museveni", "Kiir", "Okeny", "Okello", "Muwanga", "Kateregga" }) },
        { Country.UnitedStatesOutlyingIslands, new NameData(new List<string> { "James", "Emma", "Liam", "Olivia", "Noah", "Sophia" }, new List<string> { "Smith", "Johnson", "Brown", "Garcia", "Miller", "Davis" }) },
        { Country.UnitedStates, new NameData(new List<string> { "James", "Emma", "Liam", "Olivia", "Noah", "Sophia" }, new List<string> { "Smith", "Johnson", "Brown", "Garcia", "Miller", "Davis" }) },
        { Country.Uruguay, new NameData(new List<string> { "Mateo", "Sofia", "Thiago", "Valentina", "Joaquín", "Luciana" }, new List<string> { "González", "Rodríguez", "Pérez", "Fernández", "García", "López" }) },
        { Country.Uzbekistan, new NameData(new List<string> { "Jasur", "Madina", "Bekzod", "Sevinch", "Aziz", "Shahnoza" }, new List<string> { "Karimov", "Mirziyoyev", "Yuldashev", "Abdullaev", "Rakhimov", "Sadykov" }) },
        { Country.VaticanCity, new NameData(new List<string> { "Francesco", "Pietro", "Giovanni", "Paolo", "Matteo", "Marco" }, new List<string> { "Parolin", "Viganò", "Loverde", "Becciu", "Re", "Harvey" }) },
        { Country.SaintVincentAndTheGrenadines, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Gonsalves", "Eustace", "Friday", "Browne", "Thomas", "Williams" }) },
        { Country.Venezuela, new NameData(new List<string> { "Mateo", "Sofia", "Thiago", "Valentina", "Santiago", "Camila" }, new List<string> { "González", "Rodríguez", "Pérez", "Hernández", "García", "Martínez" }) },
        { Country.BritishVirginIslands, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Fahie", "Wheatley", "Smith", "O'Neal", "Penn", "Fraser" }) },
        { Country.USVirginIslands, new NameData(new List<string> { "Devon", "Aaliyah", "Kareem", "Tia", "Malik", "Chloe" }, new List<string> { "Bryan", "Roach", "De Jongh", "Turnbull", "Schneider", "Mapp" }) },
        { Country.Vietnam, new NameData(new List<string> { "Van Minh", "Thi Hoa", "Hoang Nam", "Phuong Thao", "Duc Anh", "Khanh Linh" }, new List<string> { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Huynh" }) },
        { Country.Vanuatu, new NameData(new List<string> { "Sato", "Litia", "Charlot", "Mere", "Ishmael", "Rose" }, new List<string> { "Kilman", "Salwai", "Lini", "Natapei", "Korman", "Vohor" }) },
        { Country.WallisAndFutuna, new NameData(new List<string> { "Sione", "Malia", "Petelo", "Soane", "Visesio", "Elena" }, new List<string> { "Likituki", "Vaitootai", "Ute", "Maniua", "Taufana", "Kulimoetoke" }) },
        { Country.Samoa, new NameData(new List<string> { "Sione", "Sina", "Tavita", "Tala", "Kelepi", "Lupe" }, new List<string> { "Mata'afa", "Malielegaoi", "Tofilau", "Tui Atua", "Efi", "Tamasese" }) },
        { Country.Kosovo, new NameData(new List<string> { "Arben", "Teuta", "Erion", "Pranvera", "Bledar", "Anila" }, new List<string> { "Krasniqi", "Gashi", "Berisha", "Morina", "Shala", "Hoti" }) },
        { Country.Yemen, new NameData(new List<string> { "Ali", "Fatima", "Ahmed", "Aisha", "Omar", "Maryam" }, new List<string> { "Al-Ahmar", "Al-Houthi", "Al-Afash", "Al-Zindani", "Al-Iryani", "Al-Sallal" }) },
        { Country.Mayotte, new NameData(new List<string> { "Moussa", "Fatima", "Ali", "Aminata", "Ibrahima", "Zalata" }, new List<string> { "Said", "Bamana", "Ousseni", "Ali", "Ahamada", "Abdallah" }) },
        { Country.SouthAfrica, new NameData(new List<string> { "Siyabonga", "Chloe", "Lethabo", "Liam", "Junior", "Melokuhle" }, new List<string> { "Nkosi", "van der Merwe", "Ndlovu", "Smith", "Khumalo", "Botha" }) },
        { Country.Zambia, new NameData(new List<string> { "Chileshe", "Mutinta", "Mulenga", "Thandiwe", "Kondwani", "Mapalo" }, new List<string> { "Banda", "Phiri", "Tembo", "Lungua", "Kaunda", "Chiluba" }) },
        { Country.Zimbabwe, new NameData(new List<string> { "Tinashe", "Rudo", "Tendai", "Chipo", "Farai", "Nyasha" }, new List<string> { "Mugabe", "Mnangagwa", "Tsvangirai", "Moyo", "Ncube", "Sibanda" }) }
    };

    /// <summary>
    /// Returns a randomly combined name from the specified country.
    /// </summary>
    public string GetRandomName(Country country)
    {
        NameData data = nameDatabase[country];

        string firstName = data.firstNames[UnityEngine.Random.Range(0, data.firstNames.Count)];
        string lastName = data.lastNames[UnityEngine.Random.Range(0, data.lastNames.Count)];

        if (EasternNamingOrderCountries.Contains(country))
        {
            return $"{lastName} {firstName}";
        }

        return $"{firstName} {lastName}";
    }

    /// <summary>
    /// Overload that parses string country names.
    /// </summary>
    public string GetRandomName(string countryName)
    {
        Country parsedCountry = (Country)Enum.Parse(typeof(Country), countryName, true);
        return GetRandomName(parsedCountry);
    }

    /// <summary>
    /// Overload that uses the inspector-selected country.
    /// </summary>
    public string GetRandomName()
    {
        return GetRandomName(selectedCountry);
    }

    [ContextMenu("Test Random Name")]
    private void TestRandomName()
    {
        string randomName = GetRandomName();
        Debug.Log($"[{selectedCountry}] Generated Name: {randomName}");
    }
}