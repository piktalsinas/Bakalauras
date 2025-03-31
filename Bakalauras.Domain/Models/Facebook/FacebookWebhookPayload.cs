
namespace Bakalauras.Domain.Models.Facebook
{
    public class FacebookWebhookPayload
    {
        public string Object { get; set; }
        public List<FacebookEntry> Entry { get; set; }
    }

    public class FacebookEntry
    {
        public long Time { get; set; }
        public string Id { get; set; }
        public List<FacebookMessaging> Messaging { get; set; }
    }

    public class FacebookMessaging
    {
        public FacebookSender Sender { get; set; }
        public FacebookRecipient Recipient { get; set; }
        public long Timestamp { get; set; }
        public FacebookMessage Message { get; set; }
        public FacebookPostback Postback { get; set; }
    }

    public class FacebookSender { public string Id { get; set; } }
    public class FacebookRecipient { public string Id { get; set; } }
    public class FacebookMessage { public string Mid { get; set; } public string Text { get; set; } }
    public class FacebookPostback { public string Payload { get; set; } }
}
