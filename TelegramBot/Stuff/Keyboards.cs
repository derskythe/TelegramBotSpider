using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace TelegramBot.Stuff
{
    internal static class Keyboards
    {
        public static InlineKeyboardButton[][] GetInlineKeyboard(
            String type,
            long chatId,
            Dictionary<string, string> stringArray)
        {
            var keyboardInline = new InlineKeyboardButton[stringArray.Count][];

            var prefix = type + ";" + chatId + ";";
            int i = 0;
            foreach (var pair in stringArray)
            {
                var keyboardButtons = new InlineKeyboardButton[1];
                keyboardButtons[0] = new InlineKeyboardButton
                {
                    Text = pair.Key,
                    CallbackData = prefix + pair.Value
                };

                keyboardInline[i] = keyboardButtons;
                i++;
            }

            return keyboardInline;
        }

        
        public static InlineKeyboardButton[][] GetInlineKeyboardSingleLine(
            String type,
            long chatId,
            Dictionary<string, string> stringArray)
        {
            var keyboardInline = new InlineKeyboardButton[1][];

            var prefix = type + ";" + chatId + ";";
            int i = 0;
            var keyboardButtons = new InlineKeyboardButton[stringArray.Count];
            foreach (var pair in stringArray)
            {
                keyboardButtons[i] = new InlineKeyboardButton
                {
                    Text = pair.Key,
                    CallbackData = prefix + pair.Value
                };
                i++;
            }

            keyboardInline[0] = keyboardButtons;

            return keyboardInline;
        } 

        public static KeyboardButton[][] GetKeyboard(
            IReadOnlyList<ButtonType> stringArray)
        {
            var keyboardInline =
                    new KeyboardButton[Convert.ToInt32(Math.Ceiling(stringArray.Count / 2M))][];


            int i = 0;
            for (var row = 0; row < keyboardInline.Length; row++)
            {
                if (i + 1 < stringArray.Count)
                {
                    var keyboardButtons = new KeyboardButton[2];
                    keyboardButtons[0] = new KeyboardButton
                    {
                        Text = stringArray[i].Name,
                        RequestContact = stringArray[i].PhoneNumber,
                        RequestLocation = stringArray[i].Location
                    };
                    i++;
                    keyboardButtons[1] = new KeyboardButton
                    {
                        Text = stringArray[i].Name,
                        RequestContact = stringArray[i].PhoneNumber,
                        RequestLocation = stringArray[i].Location
                    };
                    keyboardInline[row] = keyboardButtons;
                    i++;
                }
                else if (i + 1 == stringArray.Count)
                {
                    var keyboardButtons = new KeyboardButton[1];
                    keyboardButtons[0] = new KeyboardButton
                    {
                        Text = stringArray[i].Name,
                        RequestContact = stringArray[i].PhoneNumber,
                        RequestLocation = stringArray[i].Location
                    };
                    i++;
                    keyboardInline[row] = keyboardButtons;
                }
                else
                {
                    break;
                }
            }

            return keyboardInline;
        }
    }
}
