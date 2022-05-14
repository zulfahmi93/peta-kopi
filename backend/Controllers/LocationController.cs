using System.Net.Http.Headers;
using System.Xml;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;


namespace Zool.PetaKopi.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public LocationController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<List<string>> GetCities(string state)
    {
        var builder = new UriBuilder(Constants.ServerUrl)
        {
            Path  = "/locations/cities",
            Query = $"state={state}&target=",
        };

        var uri = builder.ToString();
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(uri),
            Method     = HttpMethod.Get,
        };
        request.Headers.Add("Host", Constants.ServerUrl.Host);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("*/*"));
        var response = await _httpClient.SendAsync(request);
        var xml      = await response.Content.ReadAsStringAsync();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        
        const string rowXPath = "/turbo-stream/template/option";
        var          count    = xmlDoc.SelectNodes(rowXPath)?.Count;
        
        var list = new List<string>(count ?? 0);
        for (var i = 1; i <= count; i++)
        {
            var optionNode = xmlDoc.SelectSingleNode($"{rowXPath}[{i}]");
            if (optionNode?.Attributes?["value"] == null)
            {
                continue;
            }
        
            var city = optionNode.Attributes["value"]!.Value;
            list.Add(city);
        }

        return list;
    }
}
