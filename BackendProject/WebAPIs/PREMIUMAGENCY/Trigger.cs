using CustomLogger;
using HttpMultipartParser;
using System.Text;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Trigger
    {
        public static string? getEventTriggerRequestPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                #region MikuLiveJack
                //MikuLiveJack -- Appears on top Music Survey Jukebox in Theatre
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for LOCAL MikuLiveJack {eventId}!");
                    break;
                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for QA MikuLiveJack {eventId}!");
                    break;
                case "90":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC MikuLiveJack {eventId}!");
                    break;
                #endregion
                //MikuLiveJukebox
                case "91":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC MikuLiveJukebox {eventId}!");
                    break;
                //MikuLiveEvent
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC MikuLiveEvent {eventId}!");
                    break;
                //iDOLM@ASTERs
                case "148":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for iDOLM@ASTERs {eventId}!");
                    break;
                //DJMusic
                case "197":
                    if (File.Exists($"{workpath}/eventController/DJMusic/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/DJMusic/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC DJMusic {eventId}!");
                    break;
                //hc_gallery
                case "264":
                    if (File.Exists($"{workpath}/eventController/hc_gallery/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/hc_gallery/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;
                //RollyCafe1F
                case "174":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/localgetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/localgetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for LOCAL RollyCafe1F {eventId}!");
                    break;
                case "179":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/qagetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/qagetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for QA RollyCafe1F {eventId}!");
                    break;
                case "201":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;

                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetEventTrigger unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }

            return null;
        }

        public static string? confirmEventTriggerRequestPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "124":
                    if (File.Exists($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL Rainbow {eventId}!");
                    break;
                case "72":
                    if (File.Exists($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Rainbow {eventId}!");
                    break;
                case "98":
                    if (File.Exists($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for Rainbow {eventId}!");
                    break;
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for Local MikuLiveJack {eventId}!");
                    break;
                case "55":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveJack {eventId}!");
                    break;
                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveEvent {eventId}!");
                    break;
                case "90":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJack {eventId}!");
                    break;
                case "91":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJukebox {eventId}!");
                    break;
                //MikuLiveEvent
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveEvent {eventId}!");
                    break;
                //MikuLiveJack
                case "110":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJack {eventId}!");
                    break;
                //iDOLM@ASTER
                case "148":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for iDOLM@ASTERs {eventId}!");
                    break;
                //DJMusic
                case "197":
                    if (File.Exists($"{workpath}/eventController/DJMusic/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/DJMusic/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC DJMusic {eventId}!");
                    break;
                //hc_gallery
                case "264":
                    if (File.Exists($"{workpath}/eventController/hc_gallery/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/hc_gallery/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;
                //RollyCafe1F
                case "174":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL RollyCafe1F {eventId}!");
                    break;
                case "179":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA RollyCafe1F {eventId}!");
                    break;
                case "201":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }

            return null;
        }
    }
}
