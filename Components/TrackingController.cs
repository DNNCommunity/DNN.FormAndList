using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Modules.UserDefinedTable.Interfaces;
using DotNetNuke.Services.Mail;
using MailPriority = DotNetNuke.Services.Mail.MailPriority;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class TrackingController
    {
        public enum Trigger
        {
            @New,
            Update,
            Delete
        }

        public static void OnAction(Trigger trigger, int rowId, UserDefinedTableController udtC)
        {
            if (ActionIsTriggered(trigger, udtC.Settings))
            {
                var ds = udtC.GetRow(rowId,true,true);
                ds.Tables.Add(udtC.Context());
                HandleAction(trigger, ds, udtC);
            }
        }

        static bool ActionIsTriggered(Trigger trigger, Settings  settings)
        {
            return 
                   ((trigger == Trigger.New && settings.TrackingTriggerOnNew) ||
                    ((trigger == Trigger.Update) && settings.TrackingTriggerOnUpdate) ||
                    ((trigger == Trigger.Delete) && settings.TrackingTriggerOnDelete));
        }


        static void HandleAction(Trigger trigger, DataSet data, UserDefinedTableController udtC)
        {
            var settings = udtC.Settings;
            var subject = settings.TrackingSubject;
            var message = settings.TrackingMessage;
            var from = GetEmailAddressList(settings.TrackingEmailFrom, data);
            if (from.Split(';').Length > 1)
            {
                from = (from.Split(';')[0]);
            }
            var mailTo = GetEmailAddressList(settings.TrackingEmailTo, data);
            var cc = GetEmailAddressList(settings.TrackingEmailCc, data);
            var bcc = GetEmailAddressList(settings.TrackingEmailBcc, data);
            var replyto = GetEmailAddressList(settings.TrackingEmailReplyTo, data);
            var script = settings.TrackingScript;

            var triggerMessage = string.Empty;
            switch (trigger)
            {
                case Trigger.New:
                    triggerMessage = settings.TrackingTextOnNew;
                    break;
                case Trigger.Update:
                    triggerMessage = settings.TrackingTextOnUpdate;
                    break;
                case Trigger.Delete:
                    triggerMessage = settings.TrackingTextOnDelete;
                    break;
            }


            if (script == "[AUTO]")
            {
                script = "~/DesktopModules/UserDefinedTable/XslStyleSheets/Tracking/Auto.xsl";
            }
            else
            {
                script = Globals.GetPortalSettings().HomeDirectory + script;
            }
            subject =
                ((new TokenReplace()).ReplaceEnvironmentTokens(subject, data.Tables[DataSetTableName.Data].Rows[0]));

            SendMail(from, mailTo, cc, bcc, replyto, subject, data.GetXml(), message, triggerMessage, script);
        }

     

        static string GetEmailAddressList(string mailto, DataSet data)
        {
            var addresses = new List<string>();
            foreach (var source in mailto.Split(';'))
            {
                if (source.StartsWith("[") && source.EndsWith("]"))
                {
                    var fieldtitle = source.Substring(1, source.Length - 2);
                    var rows =
                        data.Tables[DataSetTableName.Fields].Select(string.Format("{0}=\'{1}\'", FieldsTableColumn.Title,
                                                                                  fieldtitle));
                    if (rows.Length == 1)
                    {
                        fieldtitle = (string) (rows[0][FieldsTableColumn.Title]);
                        var type = DataType.ByName((string) (rows[0][FieldsTableColumn.Type]));
                        var emailAdressSource = type as IEmailAdressSource;
                        if (emailAdressSource != null)
                        {
                            addresses.Add((emailAdressSource).GetEmailAddress(fieldtitle,
                                                                                      data.Tables[DataSetTableName.Data]
                                                                                          .Rows[0]));
                        }
                    }
                }
                else
                {
                    addresses.Add(source);
                }
            }
            return (string.Join(";", addresses.ToArray()));
        }

        static void SendMail(string from, string mailTo, string cc, string bcc, string replyto, string subject,
                             string data, string message, string trigger, string script)
        {
            var xslTrans = new XslCompiledTransform();
            xslTrans.Load(HttpContext.Current.Server.MapPath(script));
            using (XmlReader xmlData = new XmlTextReader(new StringReader(data)))
            {
                using (var bodyTextWriter = new StringWriter())
                {
                    if (from == string.Empty)
                    {
                        from = Globals.GetPortalSettings().Email;
                    }

                    var xslArgs = new XsltArgumentList();
                    xslArgs.AddParam("message", "", message);
                    xslArgs.AddParam("trigger", "", trigger);
                    xslTrans.Transform(xmlData, xslArgs, bodyTextWriter);
                    var body = bodyTextWriter.ToString();
                    body = body.Replace("href=\"/",
                                        string.Format("href=\"http://{0}/",
                                                      Globals.GetPortalSettings().PortalAlias.HTTPAlias.Split('/')[0]));
                    body = body.Replace("src=\"/",
                                        string.Format("src=\"http://{0}/",
                                                      Globals.GetPortalSettings().PortalAlias.HTTPAlias.Split('/')[0]));

                    var noAttachments = new List<Attachment>();
                    Mail.SendMail(from, mailTo, cc, bcc, replyto, MailPriority.Normal, subject, MailFormat.Html,
                                  Encoding.UTF8, body, noAttachments, "", "", "", "", Host.EnableSMTPSSL);
                }
            }
        }
    }
}