using System;
using System.Web.Services.Protocols;
using SSRSTest.RSWebReference;


namespace SSRSTest
{
    public static class ReportTester
    {

        public static bool ReportExists(string reportPath, string ssrsUrl)
        {
            var rs = new ReportExecutionService();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            rs.Url = ssrsUrl;

            byte[] result = null;
            string historyID = null;
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
            string encoding;
            string mimeType;
            string extension;
            Warning[] warnings = null;
            string[] streamIDs = null;
            string format = "XML";
            ParameterValue[] parameters = null;

            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            rs.ExecutionHeaderValue = execHeader;

            try
            {
                execInfo = rs.LoadReport(reportPath, historyID);
            }
            catch (SoapException e)
            {
                if (e.Detail.OuterXml.Contains("rsItemNotFound"))
                {
                    Console.WriteLine("Report: {0} not found", reportPath);
                }
                else
                {
                    Console.WriteLine(e.Detail.OuterXml);
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public static bool RenderReport(string reportPath, string ssrsUrl)
        {
            var rs = new ReportExecutionService();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            rs.Url = ssrsUrl;

            byte[] result = null;
            string historyID = null;
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
            string encoding;
            string mimeType;
            string extension;
            Warning[] warnings = null;
            string[] streamIDs = null;
            string format = "XML";
            ParameterValue[] parameters = null;

            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            rs.ExecutionHeaderValue = execHeader;

            try
            {
                execInfo = rs.LoadReport(reportPath, historyID);
            }
            catch (SoapException e)
            {
                if (e.Detail.OuterXml.Contains("rsItemNotFound"))
                {
                    Console.WriteLine("Report: {0} not found", reportPath);
                }
                else
                {
                    Console.WriteLine(e.Detail.OuterXml);
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            //check if this report requires parms
            if (execInfo.ParametersRequired)
            {
                //if it does, then iterate parms and create set of input parms with appropriate values
                parameters = new ParameterValue[execInfo.Parameters.Length];
                for (int i = 0; i < execInfo.Parameters.Length; i++)
                {
                    if (execInfo.Parameters[i].PromptUser)
                    {
                        parameters[i] = new ParameterValue();
                        parameters[i].Name = execInfo.Parameters[i].Name;

                        if (execInfo.Parameters[i].ValidValues != null && execInfo.Parameters[i].ValidValues.Length > 0)
                        {
                            for (int x = 0; x < execInfo.Parameters[i].ValidValues.Length; x++)
                            {
                                //problem setting parm value to null from valid values
                                //iterate till we get a non-null value
                                if (!string.IsNullOrEmpty(execInfo.Parameters[i].ValidValues[x].Value))
                                {
                                    parameters[i].Value = execInfo.Parameters[i].ValidValues[x].Value;
                                }
                            }
                            Console.WriteLine("Setting {0} to {1}", parameters[i].Name, parameters[i].Value);
                        }
                        else
                        {
                            switch (execInfo.Parameters[i].Type)
                            {
                                case ParameterTypeEnum.DateTime:
                                    parameters[i].Value = DateTime.Now.ToLongDateString();
                                    break;
                                case ParameterTypeEnum.Boolean:
                                    parameters[i].Value = "true";
                                    break;
                                case ParameterTypeEnum.Float:
                                    parameters[i].Value = "1000.00";
                                    break;
                                case ParameterTypeEnum.Integer:
                                    parameters[i].Value = "10";
                                    break;
                                case ParameterTypeEnum.String:
                                    parameters[i].Value = "AAA";
                                    break;
                                default:
                                    break;
                            }
                        }
                        Console.WriteLine("---- Parameter: {0}, Value: {1}", parameters[i].Name, parameters[i].Value.ToString());
                    }
                }
            }
            else
            {
                parameters = new ParameterValue[0];
            }

            rs.SetExecutionParameters(parameters, "en-us");
            String SessionId = rs.ExecutionHeaderValue.ExecutionID;

            try
            {
                rs.Timeout = 300000;
                result = rs.Render(format, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                execInfo = rs.GetExecutionInfo();
                Console.WriteLine("Execution date and time: {0}", execInfo.ExecutionDateTime);
            }
            catch (SoapException e)
            {
                Console.WriteLine(e.Detail.OuterXml);
                return false;
            }


            return true;
        }
    }
}
