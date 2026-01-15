using MediatR;

namespace Extraordinary.Shared.Message
{
    public class ProgressBarINotification : INotification
    {
        public string Action { get; set; } = "";
        public long MaxValue { get; set; }
        public long Value { get; set; }

        public static ProgressBarINotification Max(string action = "")
        {
            return new ProgressBarINotification() { Action = action, MaxValue = long.MaxValue, Value = long.MaxValue };
        }
        public static ProgressBarINotification Min(string action = "")
        {
            return new ProgressBarINotification() { Action = action, MaxValue = long.MaxValue, Value = 0 };
        }
    }
}
