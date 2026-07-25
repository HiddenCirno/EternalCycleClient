using EFT.Communications;

namespace Oracle.Utils
{
    /// <summary>
    /// 二次封装的游戏内提示方法
    /// </summary>
    internal class Notify
    {
        /// <summary>
        /// 弹出一个通知
        /// </summary>
        public static void Message(string message, ENotificationIconType notificationType = ENotificationIconType.Default, bool isMute = false)
        {
            if (!isMute)
            {
                NotificationManagerClass.DisplayMessageNotification(
                    message,
                    ENotificationDurationType.Default,
                    notificationType,
                    null
                );
            }
        }
        /// <summary>
        /// 受全局静默影响的弹出通知
        /// </summary>
        public static void Message(string message, ENotificationIconType notificationType = ENotificationIconType.Default)
        {
            NotificationManagerClass.DisplayMessageNotification(
                message,
                ENotificationDurationType.Default,
                notificationType,
                null
            );
        }
        /// <summary>
        /// 弹出一条普通的通知
        /// </summary>
        public static void Message(string message)
        {
            Message(message, ENotificationIconType.Default, false);
        }
        /// <summary>
        /// 弹出一条警告的通知
        /// </summary>
        public static void Warning(string message)
        {
            Message(message, ENotificationIconType.Alert, false);
        }
        /// <summary>
        /// 弹出一条金色的通知
        /// </summary>
        public static void Success(string message)
        {
            Message(message, ENotificationIconType.Quest, false);
        }
    }
}
