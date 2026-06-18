namespace MediCareMS.Helpers;

/// <summary>
/// Static lookup for Bangladesh Districts and their Thanas/Upazilas.
/// </summary>
public static class BangladeshGeoData
{
    private static readonly Dictionary<string, List<string>> _data = new()
    {
        ["Dhaka"] = new() { "Adabor", "Badda", "Bangshal", "Cantonment", "Chawkbazar", "Dhamrai", "Dohar", "Demra", "Dhanmondi", "Gendaria", "Gulshan", "Hazaribagh", "Jatrabari", "Kadamtali", "Kafrul", "Kalabagan", "Kamrangirchar", "Keraniganj", "Khilgaon", "Khilkhet", "Lalbagh", "Mirpur", "Mohammadpur", "Motijheel", "Mugda", "Nawabganj", "New Market", "Pallabi", "Paltan", "Ramna", "Rayer Bazar", "Sabujbagh", "Shah Ali", "Sher-e-Bangla Nagar", "Shyampur", "Sutrapur", "Tejgaon", "Turag", "Uttara", "Wari" },
        ["Chattogram"] = new() { "Akbarshah", "Anwara", "Bakalia", "Bandar", "Banshkhali", "Boalkhali", "Chandgaon", "Chandanaish", "Chawkbazar", "Chittagong Sadar", "Double Mooring", "Fatikchhari", "Halishahar", "Hathazari", "Khulshi", "Kotwali", "Lohagara", "Mirsharai", "Pahartali", "Panchlaish", "Patiya", "Potenga", "Rangunia", "Raozan", "Sandwip", "Satkania", "Sitakunda" },
        ["Rajshahi"] = new() { "Bagha", "Bagmara", "Boalia", "Charghat", "Durgapur", "Godagari", "Matihar", "Mohanpur", "Paba", "Puthia", "Rajpara", "Shah Makhdum", "Tanore" },
        ["Khulna"] = new() { "Batiaghata", "Dacope", "Daulatpur", "Dighalia", "Dumuria", "Khan Jahan Ali", "Koyra", "Paikgachha", "Phultala", "Rupsa", "Sonadanga", "Terokhada" },
        ["Sylhet"] = new() { "Balaganj", "Beanibazar", "Bishwanath", "Companiganj", "Fenchuganj", "Golapganj", "Gowainghat", "Jaintiapur", "Kanaighat", "Osmani Nagar", "South Surma", "Sylhet Sadar", "Zakiganj" },
        ["Barisal"] = new() { "Agailjhara", "Babuganj", "Bakerganj", "Banaripara", "Gaurnadi", "Hizla", "Mehendiganj", "Muladi", "Sadar", "Wazirpur" },
        ["Rangpur"] = new() { "Badarganj", "Gangachara", "Kaunia", "Mithapukur", "Pirgachha", "Pirganj", "Rangpur Sadar", "Taraganj" },
        ["Mymensingh"] = new() { "Bhaluka", "Dhobaura", "Fulbaria", "Gaffargaon", "Gauripur", "Haluaghat", "Ishwarganj", "Muktagachha", "Mymensingh Sadar", "Nandail", "Phulpur", "Trishal" },
        ["Comilla"] = new() { "Barura", "Brahmanpara", "Burichang", "Chandina", "Chauddagram", "Comilla Sadar", "Daudkandi", "Debidwar", "Homna", "Laksam", "Lalmai", "Meghna", "Monoharganj", "Muradnagar", "Nangalkot", "Titas" },
        ["Noakhali"] = new() { "Begumganj", "Chatkhil", "Companiganj", "Hatiya", "Kabirhat", "Noakhali Sadar", "Senbagh", "Sonaimuri", "Subarnachar" },
        ["Brahmanbaria"] = new() { "Akhaura", "Ashuganj", "Bancharampur", "Bijoynagar", "Brahmanbaria Sadar", "Kasba", "Nabinagar", "Nasirnagar", "Sarail" },
        ["Cox's Bazar"] = new() { "Chakaria", "Cox's Bazar Sadar", "Kutubdia", "Maheshkhali", "Pekua", "Ramu", "Teknaf", "Ukhia" },
        ["Feni"] = new() { "Chhagalnaiya", "Daganbhuiyan", "Feni Sadar", "Parshuram", "Phulgazi", "Sonagazi" },
        ["Chandpur"] = new() { "Chandpur Sadar", "Faridganj", "Haimchar", "Haziganj", "Kachua", "Matlab Dakshin", "Matlab Uttar", "Shahrasti" },
        ["Lakshmipur"] = new() { "Kamalnagar", "Lakshmipur Sadar", "Ramganj", "Ramgati", "Raipur" },
        ["Gazipur"] = new() { "Gazipur Sadar", "Kaliakair", "Kaliganj", "Kapasia", "Sreepur", "Tongi" },
        ["Narayanganj"] = new() { "Araihazar", "Bandar", "Narayanganj Sadar", "Rupganj", "Sonargaon" },
        ["Narsingdi"] = new() { "Belabo", "Monohardi", "Narsingdi Sadar", "Palash", "Raipura", "Shibpur" },
        ["Manikganj"] = new() { "Daulatpur", "Ghior", "Harirampur", "Manikganj Sadar", "Saturia", "Shivalaya", "Singair" },
        ["Munshiganj"] = new() { "Gazaria", "Loherajong", "Munshiganj Sadar", "Sirajdikhan", "Sreenagar", "Tongibari" },
        ["Tangail"] = new() { "Basail", "Bhuapur", "Delduar", "Dhanbari", "Ghatail", "Gopalpur", "Kalihati", "Madhupur", "Mirzapur", "Nagarpur", "Sakhipur", "Tangail Sadar" },
        ["Kishoreganj"] = new() { "Austagram", "Bajitpur", "Bhairab", "Hossainpur", "Itna", "Karimganj", "Katiadi", "Kishoreganj Sadar", "Kuliarchar", "Mithamain", "Nikli", "Pakundia", "Tarail" },
        ["Netrokona"] = new() { "Atpara", "Barhatta", "Durgapur", "Kendua", "Khaliajuri", "Kalmakanda", "Madan", "Mohanganj", "Netrokona Sadar", "Purbadhala" },
        ["Sherpur"] = new() { "Jhenaigati", "Nakla", "Nalitabari", "Sherpur Sadar", "Sreebardi" },
        ["Jamalpur"] = new() { "Bakshiganj", "Dewanganj", "Islampur", "Jamalpur Sadar", "Madarganj", "Melandaha", "Sarishabari" },
        ["Bogra"] = new() { "Adamdighi", "Bogra Sadar", "Dhunat", "Dhupchanchia", "Gabtali", "Kahaloo", "Nandigram", "Sariakandi", "Shahajanpur", "Sherpur", "Shibganj", "Sonatala" },
        ["Chapai Nawabganj"] = new() { "Bholahat", "Chapai Nawabganj Sadar", "Gomastapur", "Nachole", "Shibganj" },
        ["Natore"] = new() { "Bagatipara", "Baraigram", "Gurudaspur", "Lalpur", "Natore Sadar", "Singra" },
        ["Naogaon"] = new() { "Atrai", "Badalgachhi", "Dhamoirhat", "Mahadebpur", "Manda", "Mohadevpur", "Naogaon Sadar", "Niamatpur", "Patnitala", "Porsha", "Raninagar", "Sapahar" },
        ["Pabna"] = new() { "Atgharia", "Bera", "Bhangura", "Chatmohar", "Faridpur", "Ishwardi", "Pabna Sadar", "Santhia", "Sujanagar" },
        ["Sirajganj"] = new() { "Belkuchi", "Chauhali", "Kamarkhanda", "Kazipur", "Raiganj", "Shahjadpur", "Sirajganj Sadar", "Tarash", "Ullahpara" },
        ["Jessore"] = new() { "Abhaynagar", "Bagherpara", "Chaugachha", "Jhikargachha", "Jessore Sadar", "Keshabpur", "Manirampur", "Sharsha" },
        ["Satkhira"] = new() { "Assasuni", "Debhata", "Kalaroa", "Kaliganj", "Satkhira Sadar", "Shyamnagar", "Tala" },
        ["Bagerhat"] = new() { "Bagerhat Sadar", "Chitalmari", "Fakirhat", "Kachua", "Mollahat", "Mongla", "Morrelganj", "Rampal", "Sarankhola" },
        ["Narail"] = new() { "Kalia", "Lohagara", "Narail Sadar" },
        ["Magura"] = new() { "Magura Sadar", "Mohammadpur", "Shalikha", "Sreepur" },
        ["Jhenaidah"] = new() { "Harinakunda", "Jhenaidah Sadar", "Kaliganj", "Kotchandpur", "Maheshpur", "Shailkupa" },
        ["Kushtia"] = new() { "Bheramara", "Daulatpur", "Khoksa", "Kumarkhali", "Kushtia Sadar", "Mirpur" },
        ["Meherpur"] = new() { "Gangni", "Meherpur Sadar", "Mujibnagar" },
        ["Chuadanga"] = new() { "Alamdanga", "Chuadanga Sadar", "Damurhuda", "Jibannagar" },
        ["Dinajpur"] = new() { "Birampur", "Birganj", "Biral", "Bochaganj", "Chirirbandar", "Dinajpur Sadar", "Fulbari", "Ghoraghat", "Hakimpur", "Kaharole", "Khansama", "Nawabganj", "Parbatipur" },
        ["Gaibandha"] = new() { "Fulchhari", "Gaibandha Sadar", "Gobindaganj", "Palashbari", "Sadullapur", "Saghata", "Sundarganj" },
        ["Kurigram"] = new() { "Bhurungamari", "Char Rajibpur", "Chilmari", "Kurigram Sadar", "Nageshwari", "Phulbari", "Rajarhat", "Rajibpur", "Rowmari", "Ulipur" },
        ["Lalmonirhat"] = new() { "Aditmari", "Hatibandha", "Kaliganj", "Lalmonirhat Sadar", "Patgram" },
        ["Nilphamari"] = new() { "Dimla", "Domar", "Jaldhaka", "Kishoreganj", "Nilphamari Sadar", "Saidpur" },
        ["Panchagarh"] = new() { "Atwari", "Boda", "Debiganj", "Panchagarh Sadar", "Tetulia" },
        ["Thakurgaon"] = new() { "Baliadangi", "Haripur", "Pirganj", "Ranisankail", "Thakurgaon Sadar" },
        ["Sunamganj"] = new() { "Bishwamvarpur", "Chhatak", "Derai", "Dharamapasha", "Doarabazar", "Jagannathpur", "Jamalganj", "Sulla", "Sunamganj Sadar", "Tahirpur" },
        ["Habiganj"] = new() { "Ajmiriganj", "Bahubal", "Baniachong", "Chunarughat", "Habiganj Sadar", "Lakhai", "Madhabpur", "Nabiganj" },
        ["Moulvibazar"] = new() { "Barlekha", "Juri", "Kamalganj", "Kulaura", "Moulvibazar Sadar", "Rajnagar", "Sreemangal" },
        ["Patuakhali"] = new() { "Bauphal", "Dashmina", "Dumki", "Galachipa", "Kalapara", "Mirzaganj", "Patuakhali Sadar", "Rangabali" },
        ["Bhola"] = new() { "Bhola Sadar", "Borhanuddin", "Char Fasson", "Daulatkhan", "Lalmohan", "Manpura", "Tazumuddin" },
        ["Pirojpur"] = new() { "Bhandaria", "Kawkhali", "Mathbaria", "Nazirpur", "Nesarabad", "Pirojpur Sadar", "Zianagar" },
        ["Jhalokati"] = new() { "Jhalokati Sadar", "Kathalia", "Nalchity", "Rajapur" },
        ["Barguna"] = new() { "Amtali", "Bamna", "Barguna Sadar", "Betagi", "Pathorghata", "Taltali" },
        ["Gopalganj"] = new() { "Gopalganj Sadar", "Kashiani", "Kotalipara", "Muksudpur", "Tungipara" },
        ["Madaripur"] = new() { "Kalkini", "Madaripur Sadar", "Rajoir", "Shibchar" },
        ["Faridpur"] = new() { "Alfadanga", "Bhanga", "Boalmari", "Char Bhadrasan", "Faridpur Sadar", "Madhukhali", "Nagarkanda", "Sadarpur", "Saltha" },
        ["Shariatpur"] = new() { "Bhedarganj", "Damudya", "Gosairhat", "Naria", "Shariatpur Sadar", "Zajira" },
        ["Rajbari"] = new() { "Baliakandi", "Goalanda", "Kalukhali", "Pangsha", "Rajbari Sadar" },
        ["Khagrachhari"] = new() { "Dighinala", "Guimara", "Khagrachhari Sadar", "Lakshmichhari", "Mahalchhari", "Manikchhari", "Matiranga", "Panchhari", "Ramgarh" },
        ["Rangamati"] = new() { "Bagaichhari", "Barkal", "Belaichhari", "Juraichhari", "Kaptai", "Kaukhali", "Langadu", "Naniarchar", "Rajasthali", "Rangamati Sadar" },
        ["Bandarban"] = new() { "Ali Kadam", "Bandarban Sadar", "Lama", "Naikhongchhari", "Rowangchhari", "Ruma", "Thanchi" },
    };

    public static List<string> GetDistricts() => _data.Keys.OrderBy(d => d).ToList();

    public static List<string> GetThanas(string district)
    {
        if (string.IsNullOrWhiteSpace(district)) return new();
        return _data.TryGetValue(district, out var list)
            ? list.OrderBy(t => t).ToList()
            : new();
    }
}
