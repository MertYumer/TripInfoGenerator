namespace TripInfoGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class StartUp
    {
        public static List<string> towns = new List<string>() {
                "Blagoevgrad",
                "Burgas",
                "Varna",
                "Veliko Tarnovo",
                "Vidin",
                "Vratsa",
                "Gabrovo",
                "Dobrich",
                "Kardzhali",
                "Kyustendil",
                "Lovech",
                "Montana",
                "Pazardzhik",
                "Pernik",
                "Pleven",
                "Plovdiv",
                "Razgrad",
                "Ruse",
                "Silistra",
                "Sliven",
                "Smolyan",
                "Sofia",
                "Stara Zagora",
                "Targovishte",
                "Haskovo",
                "Shumen",
                "Yambol"};
        public static List<string> urls = new List<string>();
        public static List<Trip> trips = new List<Trip>();
        public static string filePath = "../../../trips-info.json";

        public static void Main()
        {
            GenerateUrls();
            SendRequestAndReadResponse();
            WriteResultToFile();
            DisplayJson();
        }

        private static void GenerateUrls()
        {
            //Generate url for every pair between the towns
            for (int i = 0; i < towns.Count; i++)
            {
                for (int j = i + 1; j < towns.Count; j++)
                {
                    urls.Add($"https://www.google.com/maps/dir/{towns[i]}/{towns[j]}");
                }
            }
        }

        private static void SendRequestAndReadResponse()
        {
            for (int i = 0; i < urls.Count; i++)
            {
                //Get towns names from url
                var originMatch = Regex.Match(urls[i], @"dir\/([A-Z][a-z]+( [A-Z][a-z]+)?)\/");
                var origin = originMatch.Groups[1].Value;

                var destinationMatch = Regex.Match(urls[i], @"\/([A-Z][a-z]+( [A-Z][a-z]+)?)$");
                var destination = destinationMatch.Groups[1].Value;

                WebRequest request = WebRequest.Create(urls[i]);
                WebResponse response = request.GetResponse();

                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();

                    //Get distance and estimated time from response
                    var distanceMatch = Regex.Match(responseFromServer, @"(\d+(,\d+)?) км");
                    var distance = double.Parse(distanceMatch.Groups[1].Value);

                    var durationMatch = Regex.Match(responseFromServer, @"([0-5][0-9]) мин|(0[0-9]|1[0-9]|2[0-3]|[0-9]) ч ([0-5][0-9]) мин");
                    var duration = durationMatch.Value;

                    //Create object from type Trip and add it to collection
                    var trip = new Trip
                    {
                        Origin = origin,
                        Destination = destination,
                        Distance = distance,
                        Duration = duration,
                    };

                    trips.Add(trip);

                    WriteCompletionStatus(trip, i);

                    response.Close();
                }

                //You have to add pause between requests with appropriate interval, otherwise the program gives WebException - Error(429): Too many requests
                Thread.Sleep(30000);
            }
        }

        private static void WriteCompletionStatus(Trip trip, int i)
        {
            double percent = (double)((i + 1) * 100 / urls.Count);
            double percentCompletedRequests = Math.Ceiling(percent);
            Console.Clear();
            Console.WriteLine($"             Completion - {percentCompletedRequests}%");

            var loadedPart = new string('|', (int)(percentCompletedRequests * 0.4));
            var remainingPart = new string('.', (int)((100 - percentCompletedRequests) * 0.4));

            Console.WriteLine($"[{loadedPart}{remainingPart}]");

            Console.WriteLine($"       Remaining requests - {urls.Count - (i + 1)}/{urls.Count}");
        }

        private static void WriteResultToFile()
        {
            using (FileStream fs = File.Create(filePath))
            {
                //Serialize the collection and add the JSON object to the given file
                var jsonResult = JsonConvert.SerializeObject(trips, Formatting.Indented);
                var tripsInfo = new UTF8Encoding(true).GetBytes(jsonResult);
                fs.Write(tripsInfo);
            }
        }

        private static void DisplayJson()
        {
            var jsonContent = File.ReadAllText(filePath);
            var tripsInfo = JToken.Parse(jsonContent).ToString(Formatting.Indented);
            Console.WriteLine(tripsInfo);
        }

        public static void Tick()
        {
            Console.WriteLine("Tick: {0}", DateTime.Now.ToString("h:mm:ss"));
        }
    }
}