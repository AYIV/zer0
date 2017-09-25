﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Telegram.Bot;

using zer0.core;
using zer0.core.Contracts;
using zer0.core.Messages;
using IMessage = zer0.core.Contracts.IMessage;

namespace zer0.channel.telegram
{
	[Export(typeof(IModule))]
	public class BotApi : ChannelBase, ISelfManagingChannel
	{
		public override string Provider => "Telegram";

		//TODO: move to config
		private const string API_KEY = "316950397:AAHJp5F97664y6p7gFezSof2EvhboFcIfOw";
		private const string CHAT_ID_KEY = "chat_id";
		
		private ITelegramBotClient _client;
		private Map<Guid, long> _chatMap;

		protected override void SafeInit()
		{
			_chatMap = new Map<Guid, long>(Config.Key<long>(CHAT_ID_KEY));

			_client = new TelegramBotClient(API_KEY);
			_client.OnMessage += (s, e) =>
			{
				if (e?.Message.Type != Telegram.Bot.Types.Enums.MessageType.TextMessage) return;

				if (!_chatMap.IsInitializedProperly)
				{
					_chatMap[default(Guid)] = e.Message.Chat.Id;
					_chatMap.IsInitializedProperly = true;
				}

				var msg = Command.New(e.Message.Text);

				_chatMap[msg.Id] = e.Message.Chat.Id;

				ToZero(msg);
			};
		}

		public void Start() => Task.Run(() => _client.StartReceiving());

		public override bool Process(IMessage message)
		{
			if (message.Type != MessageType.Text) return false;
			
			return _client.SendTextMessageAsync(_chatMap[message.Id], message.Message.ToString()).Result != null;
		}

		public void Stop() => _client.StopReceiving();

        public override bool Supports(IMessage message) => message.Type == MessageType.Text;
	}

	internal sealed class Map<TKey, TValue> : Dictionary<TKey, TValue>
	{
		public bool IsInitializedProperly { get; set; } = true;

		public Map(TValue @default)
		{
			if (default(TValue).Equals(@default))
				IsInitializedProperly = false;

			this[default(TKey)] = @default;
		}

		public new TValue this[TKey key]
		{
			get => base[ContainsKey(key) ? key : default(TKey)];
			set => base[key] = value;
		}
	}
}
