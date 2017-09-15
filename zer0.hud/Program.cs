using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using zer0.core;
using zer0.core.Contracts;
using zer0.core.Extensions;
using zer0.core.Messages;
using IMessage = zer0.core.Contracts.IMessage;

namespace zer0.hud
{
    class Program
    {
        static ConcurrentQueue<IMessage> raw = new ConcurrentQueue<IMessage>();

        static void Main(string[] args)
        {
            Console.Title = "zer0";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var lastCommand = (IMessage)null;
            ZeroCallback func = (message, module) =>
            {
                if (module is IChannel && message is IChannelMessage cmsg)
                    cmsg.Channel = module.Provider;

                message.SetContext(lastCommand);

                if (message.Type == MessageType.Command) lastCommand = message;

                raw.Enqueue(message);
                return true;
            };

            var factory = new Loader();
            var initializer = new ModuleInitializer(factory);

            var channels = initializer.Load(factory.GetInstances<IChannel>(), func);
            
            channels
                .OfType<ISelfManagingChannel>()
                .ForEach(x => x.Start());

            var modules = factory.Modules.Except(channels);

            initializer.Load(modules.OfType<IContextable>(), func);

            Sniff(channels, modules);

            while (
                !raw.Any() ||
                raw.TryPeek(out IMessage lastMessage) &&
                lastMessage.Type != MessageType.None &&
                ((string)lastMessage.Message) != "exit"
            )
                Thread.Sleep(1000);

            channels
                .ForEach(x => x.Process(
                    TextMessage.New("Zer0 is shutting down. Cya :)")
                ));
        }

        static void Sniff(IEnumerable<IModule> channels, IEnumerable<IModule> modules) => ThreadPool.QueueUserWorkItem(e =>
        {
            var mp = new MessageProcessor(channels);
            var cp = new CommandProcessor(modules);

            while (true)
            {
                Thread.Sleep(1000);

                cp.UtilizeActions();

                if (!raw.Any()) continue;
                if (!raw.TryDequeue(out IMessage result)) continue;

                if (result is ICommand command)
                {
                    cp.Process(command);
                    continue;
                }

                mp.Process((IChannelMessage)result);
            }
        });
    }
}
