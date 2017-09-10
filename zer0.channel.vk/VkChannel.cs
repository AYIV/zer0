using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;
using zer0.core;
using zer0.core.Contracts;
using zer0.core.Extensions;
using zer0.core.Messages;

namespace zer0.channel.vk
{
	[Export(typeof(IChannel))]
	public class VkChannel : ChannelBase, ISelfManagingChannel
	{
		public override string Provider => "Vk";

		private VkApi _api;
		private ISet<string> _receivedMessages = new HashSet<string>();
		private IDictionary<string, string> _nameMap = new Dictionary<string, string>();
		
		public bool Sniffing { get; protected set; }
		
		protected override void SafeInit()
		{
			_api = new VkApi();
			_receivedMessages = new HashSet<string>();
			_nameMap = new Dictionary<string, string>();
		}

		public override bool Process(IMessage message) => true;

		public void Start()
		{
			Sniffing = true;

			//TODO: rewrite to task.run
			ThreadPool.QueueUserWorkItem(e =>
			{
				//TODO: move to init method.
				_api.Authorize(new ApiAuthParams
				{
					ApplicationId = 5255274,
					Login = "380997466776",
					Password = "2wsx#EDC4rvkontakte",
					Settings = Settings.All
				});

				while (Sniffing)
				{
					Thread.Sleep(1000);

					var unreadDialogs = UnreadDialogs(GetUnreadDialogs());
					if (unreadDialogs.All(x => _receivedMessages.Contains(x.lastid))) continue;

					var messages = unreadDialogs
						.SelectMany(x => FromJson(FromJson(GetUnreadMessages(x.userId, x.unread))).Messages)
						.ToList();

					if (messages.Count <= 0 && messages.All(x => _receivedMessages.Contains(x.Id.ToString()))) continue;

					messages.ForEach(x => _receivedMessages.Add(x.Id.ToString()));

					var userIds = unreadDialogs.Select(x => x.userId).Distinct().ToList();
					var missedUserNames = _nameMap.MissedKeys(userIds).ToList();
					if (missedUserNames.Count > 0)
					{
						GetUsersInfo(missedUserNames)
							.ForEach(x => _nameMap[x.userid] = x.fullname);
					}
					
					messages
						.GroupBy(x => x.UserId)
						.ForEach(x => ToZero(TextMessage.New($"{_nameMap[x.Key.ToString()]}\n{string.Join("\n", x.OrderBy(z => z.Date).Select(z => $"[{z.Date}] {z.Body}"))}")));
				}
			});
		}
		
		public void Stop() => Sniffing = false;

		public IEnumerable<(string userid, string fullname)> GetUsersInfo(IEnumerable<string> userIds)
		{
			var json = Invoke("users.get", ("user_ids", string.Join(",", userIds)));

			return JObject.Parse(json)["response"].Select(x => ($"{x["id"]}", $"{x["first_name"]} {x["last_name"]}"));
		}

		public string Invoke(string method, params (string key, string value)[] args)
			=> _api.Invoke(method, args.Concat(new[] { ("v", "5.64") }).ToDictionary(x => x.Item1, x => x.Item2));

		private string GetUnreadMessages(string userId, string lastMessage) => Invoke("messages.getHistory",
			("user_id", userId),
			("offset",	"0"),
			("count",	lastMessage)
		);

		private string GetUnreadDialogs() => Invoke("messages.getDialogs", 
			( "unread", "1" )
		);

		private VkResponse FromJson(string answer)
			=> new VkResponse(JObject.Parse(answer)["response"]) { RawJson = answer };

		/// <summary>
		/// Разобрать из json.
		/// </summary>
		/// <param name="response">Ответ сервера.</param>
		/// <returns>Объект типа MessagesGetObject</returns>
		public MessagesGetObject FromJson(VkResponse response) => new MessagesGetObject
		{
			TotalCount = response["count"],
			Messages = response["items"].ToReadOnlyCollectionOf<Message>(m => m),
			InRead = response["in_read"],
			OutRead = response["out_read"]
		};

		public IEnumerable<(string userId, string unread, string lastid)> UnreadDialogs(string json) => JObject
			.Parse(json)["response"]["items"]
			.Select(x =>
			(
				userId: x["message"]["user_id"].ToString(),
				inRead: x["unread"].ToString(),
				lastid: x["message"]["id"].ToString()
			));

		public override bool Supports(IMessage message)
		{
			throw new System.NotImplementedException();
		}
	}
}
