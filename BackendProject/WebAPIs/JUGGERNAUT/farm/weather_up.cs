﻿using System.Xml;

namespace BackendProject.WebAPIs.JUGGERNAUT.farm
{
    public class weather_up
    {
        public static string? ProcessWeatherUp(Dictionary<string, string>? QueryParameters, string apiPath)
        {
            if (QueryParameters != null)
            {
                string? user = QueryParameters["user"];
                string? weather = QueryParameters["weather"];

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(weather))
                {
                    Directory.CreateDirectory($"{apiPath}/juggernaut/farm/User_Data");

                    if (File.Exists($"{apiPath}/juggernaut/farm/User_Data/{user}.xml"))
                    {
                        // Load the XML string into an XmlDocument
                        XmlDocument xmlDoc = new();
                        xmlDoc.Load($"{apiPath}/juggernaut/farm/User_Data/{user}.xml");

                        // Find the <weather> element
                        XmlElement? weatherElement = xmlDoc.SelectSingleNode("/xml/resources/weather") as XmlElement;

                        if (weatherElement != null)
                        {
                            // Replace the value of <weather> with a new value
                            weatherElement.InnerText = weather;
                            File.WriteAllText($"{apiPath}/juggernaut/farm/User_Data/{user}.xml", xmlDoc.OuterXml);
                        }
                    }

                    return string.Empty;
                }
            }

            return null;
        }
    }
}
