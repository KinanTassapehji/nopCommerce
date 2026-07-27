using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Feed.ChatGptShopping.Services;

/// <summary>
/// Represents a schedule task to synchronize product feed
/// </summary>
public class ChatGptShoppingSyncTask : IScheduleTask
{
    #region Fields

    protected readonly ChatGptShoppingService _chatGptShoppingService;
    protected readonly ChatGptShoppingSettings _chatGptShoppingSettings;

    #endregion

    #region Ctor

    public ChatGptShoppingSyncTask(ChatGptShoppingService chatGptShoppingService,
        ChatGptShoppingSettings chatGptShoppingSettings)
    {
        _chatGptShoppingService = chatGptShoppingService;
        _chatGptShoppingSettings = chatGptShoppingSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Execute task
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task ExecuteAsync()
    {
        await _chatGptShoppingService.GenerateChatGptFeedAsync();
    }

    #endregion
}
