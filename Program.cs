var clientID = "kimne78kx3ncx6brgo4mv6wki5h1ko"; 
var clipSlug = "CrowdedPreciousLemurTF2John-Wp-pEG1PwtrBejlh";

var client = new HttpClient();

var twitchClient = new TwitchClipClient(client);
twitchClient.SetClientId(clientID);
var clipUri = await twitchClient.GetClipDownloadUri(clipSlug);

System.Console.WriteLine(clipUri.ToString());

