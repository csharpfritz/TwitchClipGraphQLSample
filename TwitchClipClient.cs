
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

		var query = "{\"operationName\":\"VideoAccessToken_Clip\", "+ 
			$"\"variables\":{{\"slug\":\"{clipSlug}\"}}, " + 
			"\"extensions\":{\"persistedQuery\":{ " +
				"\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}";

		var myResponse = await _Client.PostAsync("", new StringContent(query, Encoding.UTF8, "application/json"));

		var content = await myResponse.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<ClipData>(content);

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
	public Data data { get; set; }
	public Extensions extensions { get; set; }
}

public class Data
{
	public User user { get; set; }
	public Clip clip { get; set; }
}

public class User
{
	public string id { get; set; }
	public Lastbroadcast lastBroadcast { get; set; }
	public Broadcastsettings broadcastSettings { get; set; }
	public object self { get; set; }
	public object hosting { get; set; }
	public object stream { get; set; }
	public string __typename { get; set; }
}

public class Lastbroadcast
{
	public string id { get; set; }
	public Game game { get; set; }
	public string __typename { get; set; }
}

public class Game
{
	public string id { get; set; }
	public string name { get; set; }
	public string __typename { get; set; }
}

public class Broadcastsettings
{
	public string id { get; set; }
	public string language { get; set; }
	public string __typename { get; set; }
}

public class Clip
{
	public string id { get; set; }
	public Playbackaccesstoken playbackAccessToken { get; set; }
	public Videoquality[] videoQualities { get; set; }
	public string __typename { get; set; }
	public object videoOffsetSeconds { get; set; }
	public int durationSeconds { get; set; }
	public object video { get; set; }
}

public class Playbackaccesstoken
{
	public string signature { get; set; }
	public string value { get; set; }
	public string __typename { get; set; }
}

public class Videoquality
{
	public int frameRate { get; set; }
	public string quality { get; set; }
	public string sourceURL { get; set; }
	public string __typename { get; set; }
}

public class Extensions
{
	public int durationMilliseconds { get; set; }
	public string operationName { get; set; }
	public string requestID { get; set; }
}

}

