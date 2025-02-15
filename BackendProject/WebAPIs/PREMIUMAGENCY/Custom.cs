using CustomLogger;
using HttpMultipartParser;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Custom
    {
        public static string? setUserEventCustomPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            string? output = null;
            switch (eventId)
            {
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/setUserEventCustom.xml"))
                        output = File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/setUserEventCustom.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - SetUserEventCustom sent for MikuLiveEvent {eventId}!");


                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - SetUserEventCustom unhandled for eventId {eventId}");
                        return null;
                    }
            }

            return output;
        }

        public static string? getUserEventCustomRequestPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/qagetUserEventCustom.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/qagetUserEventCustom.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom sent for QA MikuLiveEvent {eventId}!");
                    break;
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/localgetUserEventCustom.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/localgetUserEventCustom.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom sent for LOCAL MikuLiveEvent {eventId}!");
                    break;
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/getUserEventCustom.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/getUserEventCustom.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom sent for PUBLIC MikuLiveEvent {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustom unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }

            return null;
        }

        public static string? getUserEventCustomRequestListPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "92":
                    if (File.Exists($"{workpath}/eventController/MikeLiveJukebox/getUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikeLiveJukebox/getUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for PUBLIC MikeLiveJukebox {eventId}!");
                    break;
                case "119":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/qagetUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/qagetUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for QA iDOLM@ASTERs {eventId}!");
                    break;
                case "148":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/getUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/getUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for PUBLIC iDOLM@ASTERs {eventId}!");
                    break;
                case "157":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/localgetUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/localgetUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for LOCAL iDOLM@ASTERs {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustomList unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }

            return null;
        }
    }
}