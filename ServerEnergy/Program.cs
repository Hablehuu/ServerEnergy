using System.Net.Http.Headers;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/Kuukausidata", Handler.Kuukausidata);


app.Run();



class Handler
{
    public static async Task<Energyreturn> Kuukausidata()
    {
        
        //List<EnergyConsumption> energiat = new List<EnergyConsumption>();
        
        double[] energianKuukausiKulutus = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await client.GetAsync("https://helsinki-openapi.nuuka.cloud/api/v1.0/EnergyData/Daily/ListByProperty?Record=LocationName&SearchString=1000%20Hakaniemen%20kauppahalli&ReportingGroup=Electricity&StartTime=2019-01-01&EndTime=2019-12-31");
        var tulos = await response.Content.ReadAsStringAsync();
        List<EnergyConsumption> energiat = JsonConvert.DeserializeObject<List<EnergyConsumption>>(tulos);
        //haetaan data
        //var json = new WebClient().DownloadString("https://helsinki-openapi.nuuka.cloud/api/v1.0/EnergyData/Daily/ListByProperty?Record=LocationName&SearchString=1000%20Hakaniemen%20kauppahalli&ReportingGroup=Electricity&StartTime=2019-01-01&EndTime=2019-12-31");
        //parsi JSON data
        foreach (var kulutus in energiat)
        {
            
            //päivä data kuukausi dataksi
            energianKuukausiKulutus[kulutus.timestamp.Month] += kulutus.value;

        }
        /*
        string[] kulutukset = Regex.Split(json, @"(?<=[}])");
        kulutukset = kulutukset.SkipLast(1).ToArray();
        foreach (string kulutus in kulutukset)
        {
            //merkkijonoista poistetaan pilkut ja hakasulkeet
            string temp = kulutus.Remove(0, 1);
            //json objektit muutetaan C# objekteiksi ja lisätään listaan
            energiat.Add(JsonSerializer.Deserialize<EnergyConsumption>(temp));
            
            //päivä data kuukausi dataksi
            energianKuukausiKulutus[energiat.Last().timestamp.Month] += energiat.Last().value;

        }
        */
        //if lauseke jolla testataan onko jo olemassa lokitiedosto
        //jos ei ole niin sellainen luodaan
        string tiedostopolku = "./lokidata/" + DateTime.Now.ToString("yyyyMMddHH:mm") + ".csv";
        if(!File.Exists(tiedostopolku))
        {
            string header = "timestamp,reportingGroup,locationName,value,unit";
            StreamWriter lokitiedosto = new StreamWriter(tiedostopolku);
            lokitiedosto.WriteLine(header);
            foreach (var kulutus in energiat)
            {
                lokitiedosto.WriteLine(kulutus.timestamp + "," + kulutus.reportingGroup + "," + kulutus.locationName + "," + kulutus.value + "," + kulutus.unit);

            }

        }



        //taulukosta poistetaan ensimmäinen alkio
        energianKuukausiKulutus = energianKuukausiKulutus.Skip(1).ToArray();
        //luodaan palautettava data
        Energyreturn palautus = new Energyreturn();
        palautus.reportingGroup = energiat[0].reportingGroup;
        palautus.locationName = energiat[0].locationName;
        palautus.value = energianKuukausiKulutus;
        palautus.unit = energiat[0].unit;

        return palautus;
    }
}


//olio joka vastaa json oliota
public class EnergyConsumption
{
    public DateTime timestamp { get; set; }
    public string? reportingGroup { get; set; }
    public string? locationName { get; set; }
    public double value { get; set; }
    public string? unit { get; set; }
}

//olio joka palautetaan käyttöliittymälle
public class Energyreturn
{
    public string? reportingGroup { get; set; }
    public string? locationName { get; set; }
    public double[]? value { get; set; }
    public string? unit { get; set; }
}