using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Audiotica.Core.Extensions;

namespace Audiotica.Core.Windows.Helpers
{
    /// <summary>
    ///     MessageService makes it easy to send strongly typed messages
    ///     between the foreground and background processes.
    /// </summary>
    /// <remarks>
    ///     JSON is used as the underlying serialization mechanism,
    ///     but you don't need to know JSON formatting to create new
    ///     messages.
    ///     See some of the related Message implementations which are
    ///     simple data objects serialized through the standard DataContract
    ///     interface.
    /// </remarks>
    public static class MessageHelper
    {
        private const string MessageBody = "MessageBody";

        public static void SendMessageToForeground<T>(T message)
        {
            var payload = new ValueSet
            {
                {MessageBody, message.SerializeToJsonWithTypeInfo()}
            };
            BackgroundMediaPlayer.SendMessageToForeground(payload);
        }

        public static void SendMessageToBackground<T>(T message)
        {
            var payload = new ValueSet
            {
                {MessageBody, message.SerializeToJsonWithTypeInfo()}
            };
            BackgroundMediaPlayer.SendMessageToBackground(payload);
        }

        public static object ParseMessage(ValueSet valueSet)
        {
            object messageBodyValue;

            // Get message payload
            return valueSet.TryGetValue(MessageBody, out messageBodyValue)
                ? messageBodyValue.ToString().TryDeserializeJsonWithTypeInfo()
                : null;
        }
    }
}