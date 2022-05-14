using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.AspNetCore.Mvc;

using Zool.PetaKopi.Backend.Controllers.Dtos;


namespace Zool.PetaKopi.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class CoffeeController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public CoffeeController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<List<SimpleCoffeeDto>> Search(string? keyword, string? state, string? district)
    {
        var builder = new UriBuilder(Constants.ServerUrl);
        builder.Query += $"keyword={keyword}";
        builder.Query += $"&state={state}";
        builder.Query += $"&district={district}";

        var uri  = builder.ToString();
        var html = await _httpClient.GetStringAsync(uri);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        const string rowXPath = "/html/body/div[2]/div[2]/div[3]/div/div/div/table/tbody/tr";
        var          count    = htmlDoc.DocumentNode.SelectNodes(rowXPath).Count;

        var list = new List<SimpleCoffeeDto>(count);
        for (var i = 1; i <= count; i++)
        {
            var logoNode     = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[{i}]/td[1]/div/div[1]/a/img");
            var nameNode     = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[{i}]/td[1]/div/div[2]/div/a");
            var districtNode = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[{i}]/td[2]/div/a[1]");
            var stateNode    = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[{i}]/td[2]/div/a[2]");

            var shopLogoUri  = logoNode?.GetAttributeValue("src", "") ?? "";
            var shopName     = nameNode?.InnerText ?? "";
            var shopDistrict = districtNode?.InnerText ?? "";
            var shopState    = stateNode?.InnerText ?? "";
            var shopSlug     = nameNode?.GetAttributeValue("href", "/")[1..] ?? "";

            list.Add(
                new SimpleCoffeeDto(
                    shopLogoUri.Trim(),
                    shopName.Trim(),
                    shopDistrict.Trim(),
                    shopState.Trim(),
                    shopSlug.Trim()
                )
            );
        }

        return list;
    }

    [HttpGet("details/{slug}")]
    public async Task<CoffeeDto> GetDetails(string slug)
    {
        var builder = new UriBuilder(Constants.ServerUrl) { Path = $"/{slug}" };

        var uri  = builder.ToString();
        var html = await _httpClient.GetStringAsync(uri);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        const string rowXPath  = "/html/body/div[2]/div[2]/div[2]/div[2]/dl/div";
        const string logoXPath = "/html/body/div[2]/div[2]/div[2]/div[1]/div[1]/img";

        var logoUri  = htmlDoc.DocumentNode.SelectSingleNode(logoXPath)?.GetAttributeValue("src", "") ?? "";
        var name     = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[1]/dd")?.InnerText ?? "";
        var district = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[2]/dd/a[1]")?.InnerText ?? "";
        var state    = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[2]/dd/a[2]")?.InnerText ?? "";

        var linkNodesCount = htmlDoc.DocumentNode.SelectNodes($"{rowXPath}[3]/dd/div/a").Count;
        var links          = new List<string>(linkNodesCount);
        for (var i = 1; i <= linkNodesCount; i++)
        {
            var link = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[3]/dd/div/a[{i}]")
                              ?.GetAttributeValue("href", "");
            if (!string.IsNullOrWhiteSpace(link))
            {
                links.Add(link.Trim());
            }
        }

        var tagNodesCount = htmlDoc.DocumentNode.SelectNodes($"{rowXPath}[4]/dd/span").Count;
        var tags          = new List<string>(tagNodesCount);
        for (var i = 1; i <= tagNodesCount; i++)
        {
            var tag = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[4]/dd/span[{i}]")?.InnerText;
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tags.Add(tag.Trim());
            }
        }

        var submitInfo        = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[5]/dd")?.InnerText ?? "";
        var submitInfoArray   = Regex.Split(submitInfo, "\\son\\s");
        var submittedBy       = submitInfoArray.Length >= 1 ? submitInfoArray[0] : "";
        var submittedOnString = submitInfoArray.Length >= 2 ? submitInfoArray[1] : "";
        var submittedOn       = DateTime.Parse(submittedOnString);

        var latitude  = 0.0;
        var longitude = 0.0;
        var mapUrl    = htmlDoc.DocumentNode.SelectSingleNode($"{rowXPath}[6]/dd/iframe")?.GetAttributeValue("src", "");
        if (!string.IsNullOrWhiteSpace(mapUrl))
        {
            var latitudeStart   = mapUrl.IndexOf("!2d", StringComparison.Ordinal) + 3;
            var longitudeStart  = mapUrl.IndexOf("!3d", StringComparison.Ordinal) + 3;
            var latitudeString  = mapUrl[latitudeStart..];
            var longitudeString = mapUrl[longitudeStart..];
            var latitudeEnd     = latitudeString.IndexOf("!", StringComparison.Ordinal);
            var longitudeEnd    = longitudeString.IndexOf("!", StringComparison.Ordinal);
            latitude  = double.Parse(latitudeString[..latitudeEnd]);
            longitude = double.Parse(longitudeString[..longitudeEnd]);
        }

        return new CoffeeDto(
            logoUri.Trim(),
            name.Trim(),
            district.Trim(),
            state.Trim(),
            links,
            tags,
            submittedBy.Trim(),
            submittedOn,
            latitude,
            longitude
        );
    }
}
