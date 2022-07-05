
using System.Text;
using System.Text.Json;
using System.Web;

public class TwitchClipClient {

	public enum Quality {
		Unspecified,
		Lowest,
		Highest

	}

	private readonly HttpClient _Client;

	public TwitchClipClient(HttpClient client) {
		this._Client = client;
		_Client.BaseAddress = new Uri("https://gql.twitch.tv/gql");
	}


	public void SetClientId(string clientId) {
		if (_Client.DefaultRequestHeaders.Contains("Client-Id")) {
			_Client.DefaultRequestHeaders.Remove("Client-Id");
		}
		_Client.DefaultRequestHeaders.Add("Client-Id", clientId);
	}

	public async Task<ClipData> GetClipAsync(string clipSlug)
	{

		// check that defaultrequestheaders contains clientid
		if (!_Client.DefaultRequestHeaders.Contains("Client-Id")) {
			throw new System.Exception("Client-Id not set");
		}

		var query = "{" +
			"\"operationName\":\"VideoAccessToken_Clip\", "+ 
			$"\"variables\":{{\"slug\":\"{clipSlug}\"}}, " + 
			"\"extensions\":{" +
				"\"persistedQuery\":{ " +
					"\"version\":1," +
					"\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"" +
				"}" +
			"}" +
		"}";

		var myResponse = await _Client.PostAsync("", new StringContent(query, Encoding.UTF8, "application/json"));
		
		try {
			myResponse.EnsureSuccessStatusCode();
		} catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.BadRequest) {
			throw new System.Exception("Invalid Twitch Client ID");
		}

		var content = await myResponse.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<ClipData>(content)!;

	}

	public async Task<Uri> GetClipDownloadUri(string clipSlug, Quality quality = Quality.Unspecified)
	{

		var payload = await GetClipAsync(clipSlug);

		var url = quality switch {
			Quality.Highest => payload.data.clip.videoQualities.OrderByDescending(v => int.Parse(v.quality)).ThenByDescending(v => v.frameRate).First().sourceURL,
			Quality.Lowest => payload.data.clip.videoQualities.OrderBy(v => int.Parse(v.quality)).ThenBy(v => v.frameRate).First().sourceURL,
			_ => payload.data.clip.videoQualities.First().sourceURL
		};

		return new Uri($"{url}?sig={payload.data.clip.playbackAccessToken.signature}&token={HttpUtility.UrlEncode(payload.data.clip.playbackAccessToken.value)}");

	}


///  OBJECTS FOR SERIALIZATION

public class ClipData
{
	public Data data { get; set; } = new();
	public Extensions extensions { get; set; } = new();
}

public class Data
{
	public User user { get; set; } = new();
	public Clip clip { get; set; } = new();
}

public class User
{
	public string id { get; set; } = string.Empty;
	public Lastbroadcast lastBroadcast { get; set; } = new();
	public Broadcastsettings broadcastSettings { get; set; } = new();
	public object self { get; set; } = new();
	public object hosting { get; set; } = new();
	public object stream { get; set; } = new();
	public string __typename { get; set; } = string.Empty;
}

public class Lastbroadcast
{
	public string id { get; set; } = string.Empty;
	public Game game { get; set; } = new();
	public string __typename { get; set; } = string.Empty;
}

public class Game
{
	public string id { get; set; } = string.Empty;
	public string name { get; set; } = string.Empty;
	public string __typename { get; set; } = string.Empty;
}

public class Broadcastsettings
{
	public string id { get; set; } = string.Empty;
	public string language { get; set; } = string.Empty;
	public string __typename { get; set; } = string.Empty;
}

public class Clip
{
	public string id { get; set; } = string.Empty;
	public Playbackaccesstoken playbackAccessToken { get; set; } = new();
	public Videoquality[] videoQualities { get; set; } = new Videoquality[] { };
	public string __typename { get; set; } = string.Empty;
	public object videoOffsetSeconds { get; set; } = new();
	public int durationSeconds { get; set; }
	public object video { get; set; } = new();
}

public class Playbackaccesstoken
{
	public string signature { get; set; } = string.Empty;
	public string value { get; set; } = string.Empty;
	public string __typename { get; set; } = string.Empty;
}

public class Videoquality
{
	public int frameRate { get; set; }
	public string quality { get; set; } = string.Empty;
	public string sourceURL { get; set; } = string.Empty;
	public string __typename { get; set; } = string.Empty;
}

public class Extensions
{
	public int durationMilliseconds { get; set; }
	public string operationName { get; set; } = string.Empty;
	public string requestID { get; set; } = string.Empty;
}

}

