using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;
using Schedule.Domain.Models.CreateModels;
using Schedule.Services.Abstractions;
using Schedule.Services.HostedServices.BaseHostedServices;

namespace Schedule.Services.HostedServices.NotificationsHostedServices;

public class NotificationsSenderHostedService : ScopedProcessor
{
    private readonly IHttpClientFactory _httpClientsFactory;
    private readonly string _tgBotToken;
    private readonly string _adminId;
    private readonly string _callbackUrl;

    private class TgAnswer
    {
        public bool Ok { get; set; }
    }
    
    public NotificationsSenderHostedService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration,
        IHttpClientFactory httpClientsFactory)
        : base(serviceScopeFactory, TimeSpan.FromSeconds(20))
    {
        _httpClientsFactory = httpClientsFactory;
        _tgBotToken = configuration["TgBotToken"];
        _adminId = configuration["AdminId"];
        _callbackUrl = configuration["CallbackUrl"];
    }

    protected override async Task ProcessInScope(IServiceProvider scope)
    {
        var notificationService = scope.GetRequiredService<IBaseService<DbNotification>>();
        var subscribersService = scope.GetRequiredService<IBaseService<DbSubscriber>>();

        try
        {
            var notifications = await notificationService.GetAllAsync();
            if (notifications.StatusCode != HttpStatusCode.OK)
                return;

            using var client = _httpClientsFactory.CreateClient();
            foreach (var notification in notifications.Data!
                         .Where(n => !n.HasBeenSent)
                     )
            {
                var subscriber = await subscribersService
                    .FindByAsync(s => s.Id == notification.SubscriberId);
                if (subscriber.StatusCode != HttpStatusCode.OK)
                    throw new Exception("Subscriber not found");

                using var response = await client.PostAsync($"{_callbackUrl}/Notify/{_tgBotToken}",
                    new StringContent(JsonConvert.SerializeObject(
                        new NotifyModel
                        {
                            Type = notification.Type,
                            Message = notification.Message,
                            ChatId = subscriber.Data!.TgChatId!.Value,
                            UserId = subscriber.Data!.TgUserId!.Value
                        }), Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Error while sending notification");

                var updateResult = await notificationService.UpdateAsync(
                    new NotificationCreateModel
                    {
                        HasBeenSent = true,
                        Message = notification.Message,
                        SubscriberId = notification.SubscriberId,
                        CreatedDateTime = notification.CreatedDateTime,
                        SentDateTime = DateTime.Now,
                        Type = notification.Type
                    }, notification.Id);
                if (updateResult.StatusCode != HttpStatusCode.OK || !updateResult.Data)
                    throw new Exception("Error while updating notification");
            }
        }
        catch (Exception ex)
        {
            var adminChatId = await subscribersService
                .FindByAsync(s => s.Id == _adminId);

            if (adminChatId.StatusCode != HttpStatusCode.OK)
                throw new Exception("Admin not found");

            await SendToTgDirect(adminChatId.Data!.TgChatId!.Value,
                "Произошла внутренняя ошибка: " + ex.Message + "\n" + ex.InnerException?.Message);
        }
    }

    private async Task SendToTgDirect(long chatId, string text)
    {
        using var client = _httpClientsFactory.CreateClient();
        using var response = await client.PostAsync("https://api.telegram.org/bot" + _tgBotToken + "/sendMessage",
            new StringContent(JsonConvert.SerializeObject(new
            {
                chat_id = chatId,
                text = text
            }), Encoding.UTF8, "application/json"));
        var content = JsonConvert.DeserializeObject<TgAnswer>(await response.Content.ReadAsStringAsync());
        if (content is null || !content.Ok)
            throw new Exception("Error while sending message");
    }
}