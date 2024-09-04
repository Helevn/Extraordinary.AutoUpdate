using Extraordinary.App.ViewModels;
using MediatR;

namespace Extraordinary.App.MessageHandler
{
    internal class ProgressBarINotificationHandler : INotificationHandler<ProgressBarINotification>
    {
        private readonly RealtimeViewModel _realtimeViewModel;

        public ProgressBarINotificationHandler(RealtimeViewModel realtimeViewModel)
        {
            this._realtimeViewModel = realtimeViewModel;
        }

        public Task Handle(ProgressBarINotification notification, CancellationToken cancellationToken)
        {
            UIHelper.RunInUIThread(pl =>
            {
                _realtimeViewModel.RefreshProgressBar(notification.Action, notification.MaxValue, notification.Value);
            });
            return Task.CompletedTask;
        }
    }
}
