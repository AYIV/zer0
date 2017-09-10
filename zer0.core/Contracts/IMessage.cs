﻿using System;

namespace zer0.core.Contracts
{
	public interface IMessage
	{
		Guid Id { get; }

		MessageType Type { get; }

		object Message { get; }

		IMessage Context { get; }

		bool HasContext { get; }

        IMessage SetContext(IMessage contextMessage);
    }

    public interface IChannelMessage : IMessage
    {
        string Channel { get; set; }
    }
}
